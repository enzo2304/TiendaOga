/* =====================================================================
   TIENDA OGA - Etapa 2 Backup y restauración (SQL Server / T-SQL)
   IMPORTANTE: estos procedimientos se crean en la base MASTER, no en
   TiendaOgaDB. La restauración no puede ejecutarse desde un procedimiento
   que viva dentro de la base que se está reemplazando.
   El script es re-ejecutable (CREATE OR ALTER).
   Requiere un login con permisos de sysadmin o equivalentes.
   ===================================================================== */

   /*CORRERLO EN MASTER */
USE master;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------
   1) CREAR BACKUP
      @ruta           : carpeta de destino (si es NULL, la de la instancia)
      @nombre_archivo : nombre completo con fecha y hora, ej:
                        Backup_Ventas_12102026_173800.bak
                        (si es NULL: TiendaOgaDB_yyyyMMdd_HHmmss.bak)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_backup_crear
    @ruta           NVARCHAR(260) = NULL,
    @nombre_archivo NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @bd SYSNAME = N'TiendaOgaDB';

    IF DB_ID(@bd) IS NULL
        THROW 50100, 'La base TiendaOgaDB no existe en este servidor.', 1;

    SET @nombre_archivo = NULLIF(LTRIM(RTRIM(@nombre_archivo)), N'');

    IF @nombre_archivo IS NULL
        SET @nombre_archivo = @bd + N'_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + N'.bak';

    IF @nombre_archivo LIKE N'%[\/:*?"<>|]%'
        THROW 50101, 'El nombre del archivo tiene caracteres no permitidos.', 1;

    IF RIGHT(@nombre_archivo, 4) <> N'.bak'
        SET @nombre_archivo = @nombre_archivo + N'.bak';

    IF @ruta IS NULL OR LTRIM(RTRIM(@ruta)) = N''
        SET @ruta = ISNULL(CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(260)), N'C:\Backups\');

    IF RIGHT(@ruta, 1) <> N'\' SET @ruta = @ruta + N'\';

    DECLARE @archivo  NVARCHAR(400) = @ruta + @nombre_archivo;
    DECLARE @etiqueta NVARCHAR(128) = CONCAT(@bd, N' - ', FORMAT(GETDATE(), 'dd/MM/yyyy HH:mm:ss'));

    BACKUP DATABASE @bd TO DISK = @archivo WITH INIT, CHECKSUM, NAME = @etiqueta;

    RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;

    SELECT @archivo AS archivo_generado, GETDATE() AS fecha_backup;
END;
GO

/* ---------------------------------------------------------------------
   2) INFORMACIÓN DE UN ARCHIVO .BAK (base de origen, fecha, versión, tipo)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_backup_info
    @archivo NVARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    RESTORE HEADERONLY FROM DISK = @archivo;
END;
GO

/* ---------------------------------------------------------------------
   3) VERSIÓN DEL SERVIDOR (para validar la compatibilidad antes de restaurar)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_servidor_info
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) AS version_mayor,
           (SELECT compatibility_level FROM sys.databases WHERE name = N'model')       AS nivel_servidor,
           (SELECT compatibility_level FROM sys.databases WHERE name = N'TiendaOgaDB') AS nivel_actual;
END;
GO

/* ---------------------------------------------------------------------
   4) TAMAÑO DE LA BASE (pantalla de estado)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_base_info
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(SUM(CAST(size AS BIGINT)) * 8.0 / 1024 AS DECIMAL(12,1)) AS tamanio_mb
    FROM sys.master_files
    WHERE database_id = DB_ID(N'TiendaOgaDB');
END;
GO

/* ---------------------------------------------------------------------
   5) CONEXIONES ACTIVAS A LA BASE (aviso antes de restaurar)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_conexiones_activas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS conexiones
    FROM sys.dm_exec_sessions
    WHERE database_id = DB_ID(N'TiendaOgaDB')
      AND is_user_process = 1
      AND session_id <> @@SPID;
END;
GO

/* ---------------------------------------------------------------------
   6) RESTAURAR
      a) Verifica el archivo antes de tocar nada.
      b) Si @backup_previo = 1, genera un backup del estado actual
         (PreRestore_<fecha>_<hora>.bak) en @carpeta_previo.
      c) Restaura en modo un solo usuario y vuelve a multiusuario.
      d) Si la base quedó con un nivel de compatibilidad menor al del
         servidor, lo sube (el código usa STRING_AGG y FORMAT, que
         necesitan nivel 140 o superior).
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_tienda_backup_restaurar
    @archivo        NVARCHAR(400),
    @backup_previo  BIT           = 1,
    @carpeta_previo NVARCHAR(260) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @bd  SYSNAME       = N'TiendaOgaDB';
    DECLARE @bdq NVARCHAR(300) = QUOTENAME(@bd);
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @preventivo NVARCHAR(400) = NULL;

    IF DB_ID(@bd) IS NULL
        THROW 50110, 'La base TiendaOgaDB no existe en este servidor.', 1;

    -- a) Verificar el archivo antes de modificar nada
    RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;

    -- b) Backup preventivo
    IF @backup_previo = 1
    BEGIN
        IF @carpeta_previo IS NULL OR LTRIM(RTRIM(@carpeta_previo)) = N''
            SET @carpeta_previo = ISNULL(CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(260)), N'C:\Backups\');

        IF RIGHT(@carpeta_previo, 1) <> N'\' SET @carpeta_previo = @carpeta_previo + N'\';

        DECLARE @etiqueta NVARCHAR(128) = N'Preventivo antes de restaurar';
        SET @preventivo = @carpeta_previo + N'PreRestore_' + FORMAT(GETDATE(), 'ddMMyyyy_HHmmss') + N'.bak';

        BACKUP DATABASE @bd TO DISK = @preventivo WITH INIT, CHECKSUM, NAME = @etiqueta;
    END

    -- c) Restaurar
    BEGIN TRY
        SET @sql = N'ALTER DATABASE ' + @bdq + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;';
        EXEC (@sql);

        RESTORE DATABASE @bd FROM DISK = @archivo WITH REPLACE;

        SET @sql = N'ALTER DATABASE ' + @bdq + N' SET MULTI_USER;';
        EXEC (@sql);
    END TRY
    BEGIN CATCH
        BEGIN TRY
            SET @sql = N'ALTER DATABASE ' + @bdq + N' SET MULTI_USER;';
            EXEC (@sql);
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH;

    -- d) Compatibilidad con backups de versiones anteriores
    DECLARE @nivelServidor INT = (SELECT compatibility_level FROM sys.databases WHERE name = N'model');
    DECLARE @nivelAntes    INT = (SELECT compatibility_level FROM sys.databases WHERE name = @bd);
    DECLARE @nivelDespues  INT = @nivelAntes;

    IF @nivelAntes < @nivelServidor
    BEGIN
        SET @sql = N'ALTER DATABASE ' + @bdq + N' SET COMPATIBILITY_LEVEL = '
                   + CAST(@nivelServidor AS NVARCHAR(3)) + N';';
        EXEC (@sql);
        SET @nivelDespues = @nivelServidor;
    END

    SELECT N'Restauracion completada' AS resultado,
           @preventivo                AS backup_previo,
           @nivelAntes                AS nivel_compatibilidad_antes,
           @nivelDespues              AS nivel_compatibilidad_despues;
END;
GO

/* ---------------------------------------------------------------------
   PRUEBAS RÁPIDAS
   --------------------------------------------------------------------- */
-- EXEC master.dbo.usp_tienda_backup_crear @ruta = N'C:\TiendaOga\Backups', @nombre_archivo = N'Prueba_12102026_173800.bak';
-- EXEC master.dbo.usp_tienda_servidor_info;
-- EXEC master.dbo.usp_tienda_base_info;
-- EXEC master.dbo.usp_tienda_conexiones_activas;
-- EXEC master.dbo.usp_tienda_backup_info @archivo = N'C:\TiendaOga\Backups\Prueba_12102026_173800.bak';
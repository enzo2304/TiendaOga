-- ==============================================================
-- TiendaOga - Script de base de datos completo
-- Incluye: tablas, datos base (perfiles) y stored procedures.
-- Correr este script UNA VEZ en cada instalación nueva de SQL Server
-- (por ejemplo, en cada PC/notebook donde se clone el proyecto).
--
-- Este script es IDEMPOTENTE para las tablas (usa IF NOT EXISTS),
-- así que si ya existen no las vuelve a crear ni pisa datos.
-- Los procedimientos usan CREATE OR ALTER, así que sí se actualizan
-- cada vez que se corre el script (eso es lo esperado).
-- ==============================================================

-- ------------------------------------------------------------
-- Tabla Perfiles
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Perfiles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Perfiles (
        id_perfil     INT IDENTITY(1,1) PRIMARY KEY,
        nombre_perfil VARCHAR(50)  NOT NULL,
        descripcion   VARCHAR(255) NULL,
        Activo        BIT          NOT NULL DEFAULT 1
    );
END
GO

-- Datos base de Perfiles (solo si la tabla está vacía)
IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles)
BEGIN
    INSERT INTO dbo.Perfiles (nombre_perfil, descripcion, Activo) VALUES
    ('Administrador', 'Perfil con acceso total al sistema', 1),
    ('Gerente',        'Perfil de gestión con permisos intermedios', 1),
    ('Vendedor',       'Perfil de ventas con acceso limitado', 1);
END
GO

-- ------------------------------------------------------------
-- Tabla Usuario
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuario' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Usuario (
        id_usuario INT IDENTITY(1,1) PRIMARY KEY,
        id_perfil  INT          NOT NULL,
        nombre     VARCHAR(100) NOT NULL,
        apellido   VARCHAR(100) NOT NULL,
        usuario    VARCHAR(50)  NOT NULL UNIQUE,
        password   VARCHAR(255) NOT NULL,
        email      VARCHAR(100) NULL,
        Activo     BIT          NOT NULL DEFAULT 1,
        CONSTRAINT FK_Usuario_Perfil FOREIGN KEY (id_perfil) REFERENCES dbo.Perfiles(id_perfil)
    );
END
GO

-- ------------------------------------------------------------
-- sp_ABM_Usuario: Alta / Baja / Modificación / Reactivación
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_ABM_Usuario
    @Operacion  CHAR(1),          -- 'A' = Alta, 'B' = Baja, 'M' = Modificación, 'R' = Reactivar
    @id_usuario INT = NULL,
    @id_perfil  INT = NULL,
    @nombre     VARCHAR(100) = NULL,
    @apellido   VARCHAR(100) = NULL,
    @usuario    VARCHAR(50)  = NULL,
    @password   VARCHAR(255) = NULL,
    @email      VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Operacion = 'A'
    BEGIN
        INSERT INTO Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
        VALUES (@id_perfil, @nombre, @apellido, @usuario, @password, @email, 1);
    END
    ELSE IF @Operacion = 'B'
    BEGIN
        -- Baja lógica: no se borra la fila, se desactiva.
        UPDATE Usuario
        SET Activo = 0
        WHERE id_usuario = @id_usuario;
    END
    ELSE IF @Operacion = 'M'
    BEGIN
        UPDATE Usuario
        SET id_perfil = @id_perfil,
            nombre    = @nombre,
            apellido  = @apellido,
            usuario   = @usuario,
            password  = ISNULL(@password, password),   -- si @password es NULL, conserva la actual
            email     = @email
        WHERE id_usuario = @id_usuario;
    END
    ELSE IF @Operacion = 'R'
    BEGIN
        -- Reactivación: usuario existente que estaba dado de baja.
        UPDATE Usuario
        SET Activo = 1
        WHERE id_usuario = @id_usuario;
    END
END
GO

-- ------------------------------------------------------------
-- sp_Listar_Perfiles_Activos: para llenar el ComboBox de Perfil
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Perfiles_Activos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id_perfil, nombre_perfil
    FROM Perfiles
    WHERE Activo = 1
    ORDER BY nombre_perfil;
END
GO

-- ------------------------------------------------------------
-- sp_Listar_Usuarios: para la grilla de usuarios registrados
-- (incluye id_perfil, necesario para el ComboBox al modificar)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Usuarios
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.id_usuario, u.id_perfil, u.nombre, u.apellido, u.usuario, u.email, u.Activo, p.nombre_perfil
    FROM Usuario u
    INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil
    ORDER BY u.nombre, u.apellido;
END
GO

-- ------------------------------------------------------------
-- sp_Existe_Usuario: valida nombre de usuario duplicado
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Existe_Usuario
    @usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM Usuario
    WHERE usuario = @usuario;
END
GO

-- ==============================================================
-- Fin del script. Después de correrlo, si es una base nueva y
-- vacía, hay que crear al menos un usuario Administrador para
-- poder loguearse. Ejemplo (contraseña "1234", ya hasheada con
-- BCrypt): reemplazá el hash por uno propio si vas a usar otra
-- contraseña.
-- ==============================================================
-- IF NOT EXISTS (SELECT 1 FROM dbo.Usuario)
-- BEGIN
--     INSERT INTO dbo.Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
--     VALUES (1, 'Admin', 'Sistema', 'admin', '$2b$11$jCv0KUdYvuiIjdsGV.U8I.v8ufyBGO/UhaB82IfdWEBeMtftZGdta', 'admin@tiendaoga.com', 1);
-- END
-- GO
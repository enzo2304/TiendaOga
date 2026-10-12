USE TiendaOgaDB;

/* ---------------------------------------------------------------------
   1) TABLAS
   --------------------------------------------------------------------- */

-- TABLA CATEGORIA
CREATE TABLE categoria(
    id_categoria INT IDENTITY(1,1),
    nombre       VARCHAR(30) NOT NULL,
    descripcion  VARCHAR(50) NOT NULL,
    activo       BIT NOT NULL CONSTRAINT df_categoria_activo DEFAULT 1,
    CONSTRAINT pk_categoria PRIMARY KEY (id_categoria),
    CONSTRAINT uq_categoria_nombre UNIQUE (nombre),
    CONSTRAINT ck_categoria_nombre CHECK (LEN(LTRIM(RTRIM(nombre))) > 0)
);

-- TABLA TIPO_PAGO
CREATE TABLE tipo_pago(
    id_tipo_pago     INT IDENTITY(1,1),
    nombre_tipo_pago VARCHAR(30) NOT NULL,
    descripcion  VARCHAR(50) NULL,
    activo BIT NOT NULL CONSTRAINT df_tipo_pago_activo DEFAULT 1,
    CONSTRAINT pk_tipo_pago PRIMARY KEY (id_tipo_pago),
    CONSTRAINT uq_tipo_pago_nombre UNIQUE (nombre_tipo_pago),
    CONSTRAINT ck_tipo_pago_nombre CHECK (LEN(LTRIM(RTRIM(nombre_tipo_pago))) > 0)
);

-- TABLA CLIENTE
CREATE TABLE cliente(
    id_cliente   INT IDENTITY(1,1),
    tipo_cliente   VARCHAR(30) NOT NULL,
    nombre_razon_social VARCHAR(50) NOT NULL,
    tipo_documento  VARCHAR(30) NOT NULL,
    nro_documento  VARCHAR(20) NOT NULL,
    telefono VARCHAR(20) NULL,
    activo BIT NOT NULL CONSTRAINT df_cliente_activo DEFAULT 1,
    CONSTRAINT pk_cliente PRIMARY KEY (id_cliente),
    CONSTRAINT uq_cliente_documento UNIQUE (tipo_documento, nro_documento),
    CONSTRAINT ck_cliente_tipo CHECK (tipo_cliente IN ('PERSONA','EMPRESA')),
    CONSTRAINT ck_cliente_tipo_doc CHECK (tipo_documento IN ('DNI','CUIT','CUIL','PASAPORTE')),
    CONSTRAINT ck_cliente_nro_doc CHECK (nro_documento NOT LIKE '%[^0-9]%')  -- solo digitos
);

-- TABLA PRODUCTO
CREATE TABLE producto(
    id_producto   INT IDENTITY(1,1),
    id_categoria  INT NOT NULL,
    nombre  VARCHAR(30) NOT NULL,
    precio_costo  DECIMAL(12,2) NOT NULL,
    precio_ventas DECIMAL(12,2) NOT NULL,
    stock INT NOT NULL CONSTRAINT df_producto_stock DEFAULT 0,
    stock_bajo INT NOT NULL CONSTRAINT df_producto_stock_bajo DEFAULT 0,
    activo BIT NOT NULL CONSTRAINT df_producto_activo DEFAULT 1,
    CONSTRAINT pk_producto PRIMARY KEY (id_producto),
    CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria),
    CONSTRAINT ck_producto_costo  CHECK (precio_costo  > 0),
    CONSTRAINT ck_producto_precio CHECK (precio_ventas > 0),
    CONSTRAINT ck_producto_stock  CHECK (stock >= 0),
    CONSTRAINT ck_producto_stock_bajo CHECK (stock_bajo >= 0),
    CONSTRAINT ck_producto_nombre CHECK (LEN(LTRIM(RTRIM(nombre))) > 0)
);

-- TABLA PRODUCTO_HOGAR (extension 1 a 1 de producto)
CREATE TABLE producto_hogar(
    id_producto INT,
    material  VARCHAR(50) NULL,
    dimensiones VARCHAR(50) NULL,
    peso_kg  DECIMAL(8,2) NULL,
    ambiente VARCHAR(50) NULL,
    CONSTRAINT pk_producto_hogar PRIMARY KEY (id_producto),
    CONSTRAINT fk_hogar_producto FOREIGN KEY (id_producto)
        REFERENCES producto(id_producto) ON DELETE CASCADE,
    CONSTRAINT ck_hogar_peso CHECK (peso_kg IS NULL OR peso_kg > 0)
);

-- TABLA PRODUCTO_TECNOLOGIA (extension 1 a 1 de producto)
CREATE TABLE producto_tecnologia(
    id_producto    INT,
    marca VARCHAR(50) NOT NULL,
    modelo  VARCHAR(50) NOT NULL,
    garantia_meses INT NOT NULL CONSTRAINT df_tecno_garantia DEFAULT 0,
    voltaje  VARCHAR(20) NULL,
    CONSTRAINT pk_producto_tecnologia PRIMARY KEY (id_producto),
    CONSTRAINT fk_tecnologia_producto FOREIGN KEY (id_producto)
        REFERENCES producto(id_producto) ON DELETE CASCADE,
    CONSTRAINT ck_tecno_garantia CHECK (garantia_meses >= 0)
);

-- TABLA VENTA_CABECERA
CREATE TABLE venta_cabecera(
    id_venta INT IDENTITY(1,1),
    id_usuario INT NOT NULL,
    id_cliente INT NOT NULL,
    fecha_venta  DATETIME NOT NULL CONSTRAINT df_venta_fecha DEFAULT GETDATE(),
    total_venta  DECIMAL(12,2) NOT NULL CONSTRAINT df_venta_total DEFAULT 0,
    estado  VARCHAR(20) NOT NULL CONSTRAINT df_venta_estado DEFAULT 'PENDIENTE',
    numero_venta VARCHAR(20) NOT NULL,
    activo  BIT NOT NULL CONSTRAINT df_venta_activo DEFAULT 1,
    CONSTRAINT pk_venta_cabecera PRIMARY KEY (id_venta),
    CONSTRAINT uq_venta_numero UNIQUE (numero_venta),
    CONSTRAINT fk_venta_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    CONSTRAINT fk_venta_cliente FOREIGN KEY (id_cliente) REFERENCES cliente(id_cliente),
    CONSTRAINT ck_venta_total  CHECK (total_venta >= 0),
    CONSTRAINT ck_venta_estado CHECK (estado IN ('PENDIENTE','PAGADA','ANULADA'))
);

-- TABLA VENTA_DETALLE
CREATE TABLE venta_detalle(
    id_detalle INT IDENTITY(1,1),
    id_venta INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(12,2) NOT NULL,
    sub_total AS (cantidad * precio_unitario) PERSISTED,
    activo BIT NOT NULL CONSTRAINT df_detalle_activo DEFAULT 1,
    CONSTRAINT pk_venta_detalle PRIMARY KEY (id_detalle),
    CONSTRAINT uq_detalle_venta_producto UNIQUE (id_venta, id_producto),
    CONSTRAINT fk_detalle_venta FOREIGN KEY (id_venta) REFERENCES venta_cabecera(id_venta) ON DELETE CASCADE,
    CONSTRAINT fk_detalle_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto),
    CONSTRAINT ck_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_detalle_precio   CHECK (precio_unitario >= 0)
);

-- TABLA PAGO
CREATE TABLE pago(
    id_pago  INT IDENTITY(1,1),
    id_venta  INT NOT NULL,
    id_tipo_pago INT NOT NULL,
    monto  DECIMAL(12,2) NOT NULL,
    fecha_pago DATETIME NOT NULL CONSTRAINT df_pago_fecha DEFAULT GETDATE(),
    activo  BIT NOT NULL CONSTRAINT df_pago_activo DEFAULT 1,
    CONSTRAINT pk_pago PRIMARY KEY (id_pago),
    CONSTRAINT fk_pago_venta FOREIGN KEY (id_venta)
        REFERENCES venta_cabecera(id_venta),
    CONSTRAINT fk_pago_tipo FOREIGN KEY (id_tipo_pago)
        REFERENCES tipo_pago(id_tipo_pago),
    CONSTRAINT ck_pago_monto CHECK (monto > 0)
);
GO

/* ---------------------------------------------------------------------
   2) TRIGGERS: exclusión mutua hogar / tecnología
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_hogar_exclusivo ON producto_hogar
AFTER INSERT, UPDATE AS
BEGIN
    IF EXISTS (SELECT 1 FROM inserted i
               JOIN producto_tecnologia t ON t.id_producto = i.id_producto)
    BEGIN
        RAISERROR('El producto ya esta cargado como tecnologia.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

CREATE OR ALTER TRIGGER trg_tecnologia_exclusivo ON producto_tecnologia
AFTER INSERT, UPDATE AS
BEGIN
    IF EXISTS (SELECT 1 FROM inserted i
               JOIN producto_hogar h ON h.id_producto = i.id_producto)
    BEGIN
        RAISERROR('El producto ya esta cargado como hogar.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

/* ---------------------------------------------------------------------
   3) PROCEDIMIENTOS ALMACENADOS DE NEGOCIO
   --------------------------------------------------------------------- */

-- CRUD y gestión de cliente --------------------------------------------
CREATE OR ALTER PROCEDURE sp_cliente_insertar
    @tipo_cliente VARCHAR(30), @nombre_razon_social VARCHAR(50),
    @tipo_documento VARCHAR(30), @nro_documento VARCHAR(20), @telefono VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO cliente(tipo_cliente, nombre_razon_social, tipo_documento, nro_documento, telefono)
    VALUES (@tipo_cliente, @nombre_razon_social, @tipo_documento, @nro_documento, @telefono);
    SELECT SCOPE_IDENTITY() AS id_cliente;
END;
GO

CREATE OR ALTER PROCEDURE sp_cliente_modificar
    @id_cliente INT, @tipo_cliente VARCHAR(30), @nombre_razon_social VARCHAR(50),
    @tipo_documento VARCHAR(30), @nro_documento VARCHAR(20), @telefono VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE cliente
    SET tipo_cliente = @tipo_cliente, nombre_razon_social = @nombre_razon_social,
        tipo_documento = @tipo_documento, nro_documento = @nro_documento, telefono = @telefono
    WHERE id_cliente = @id_cliente;
END;
GO

CREATE OR ALTER PROCEDURE sp_cliente_baja @id_cliente INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE cliente SET activo = 0 WHERE id_cliente = @id_cliente;
END;
GO

CREATE OR ALTER PROCEDURE sp_cliente_reactivar @id_cliente INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE cliente SET activo = 1 WHERE id_cliente = @id_cliente;
END;
GO

CREATE OR ALTER PROCEDURE sp_cliente_listar @solo_activos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_cliente, tipo_cliente, nombre_razon_social, tipo_documento, nro_documento, telefono, activo
    FROM cliente
    WHERE (@solo_activos = 0 OR activo = 1)
    ORDER BY nombre_razon_social;
END;
GO

CREATE OR ALTER PROCEDURE sp_cliente_listar_con_compras @solo_activos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.id_cliente, c.tipo_cliente, c.nombre_razon_social,
           c.tipo_documento, c.nro_documento, c.telefono, c.activo,
           (SELECT COUNT(*) FROM venta_cabecera v
            WHERE v.id_cliente = c.id_cliente
              AND v.estado <> 'ANULADA' AND v.activo = 1) AS compras
    FROM cliente c
    WHERE (@solo_activos = 0 OR c.activo = 1)
    ORDER BY c.id_cliente;
END;
GO

-- Historial de compras de un cliente (STRING_AGG requiere SQL Server 2017+)
CREATE OR ALTER PROCEDURE sp_cliente_historial @id_cliente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.numero_venta, v.fecha_venta,
           (SELECT STRING_AGG(CONCAT(d.cantidad, ' x ', p.nombre), ', ')
            FROM venta_detalle d
            JOIN producto p ON p.id_producto = d.id_producto
            WHERE d.id_venta = v.id_venta AND d.activo = 1) AS articulos,
           (SELECT STRING_AGG(tp.nombre_tipo_pago, ', ')
            FROM pago pg
            JOIN tipo_pago tp ON tp.id_tipo_pago = pg.id_tipo_pago
            WHERE pg.id_venta = v.id_venta AND pg.activo = 1) AS metodo_pago,
           v.total_venta
    FROM venta_cabecera v
    WHERE v.id_cliente = @id_cliente AND v.activo = 1
    ORDER BY v.fecha_venta DESC;
END;
GO

-- Consultas y operaciones de venta -------------------------------------
CREATE OR ALTER PROCEDURE sp_ventas_por_fecha @desde DATE, @hasta DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.numero_venta, v.fecha_venta, c.nombre_razon_social AS cliente,
           v.total_venta, v.estado
    FROM venta_cabecera v
    JOIN cliente c ON c.id_cliente = v.id_cliente
    WHERE v.fecha_venta >= @desde
      AND v.fecha_venta <  DATEADD(DAY, 1, @hasta)
      AND v.activo = 1
    ORDER BY v.fecha_venta;
END;
GO

CREATE OR ALTER PROCEDURE sp_agregar_detalle_venta
    @id_venta INT, @id_producto INT, @cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM venta_cabecera
                       WHERE id_venta = @id_venta AND estado = 'PENDIENTE' AND activo = 1)
            THROW 50001, 'La venta no existe o no esta pendiente.', 1;

        DECLARE @precio DECIMAL(12,2);
        SELECT @precio = precio_ventas FROM producto
        WHERE id_producto = @id_producto AND activo = 1;

        IF @precio IS NULL
            THROW 50002, 'Producto inexistente o inactivo.', 1;

        UPDATE producto SET stock = stock - @cantidad
        WHERE id_producto = @id_producto AND stock >= @cantidad;

        IF @@ROWCOUNT = 0
            THROW 50003, 'Stock insuficiente.', 1;

        INSERT INTO venta_detalle(id_venta, id_producto, cantidad, precio_unitario)
        VALUES (@id_venta, @id_producto, @cantidad, @precio);

        UPDATE venta_cabecera
        SET total_venta = (SELECT SUM(sub_total) FROM venta_detalle
                           WHERE id_venta = @id_venta AND activo = 1)
        WHERE id_venta = @id_venta;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* ---------------------------------------------------------------------
   4) BACKUP (ejecutar dentro de la base de datos de la app)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_backup_bd
    @ruta NVARCHAR(260) = N'C:\Backups\'
AS
BEGIN
    SET NOCOUNT ON;

    IF RIGHT(@ruta, 1) <> N'\' SET @ruta = @ruta + N'\';

    DECLARE @archivo NVARCHAR(400) =
        @ruta + DB_NAME() + N'_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + N'.bak';

    DECLARE @sql NVARCHAR(MAX) =
        N'BACKUP DATABASE ' + QUOTENAME(DB_NAME()) +
        N' TO DISK = @a WITH INIT, CHECKSUM';

    EXEC sp_executesql @sql, N'@a NVARCHAR(400)', @a = @archivo;

    RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;

    SELECT @archivo AS archivo_generado, GETDATE() AS fecha_backup;
END;
GO

/* ---------------------------------------------------------------------
   5) RESTORE (Nota: Idealmente se ejecutan sobre master)
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_listar_backups @nombre_bd SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT bmf.physical_device_name AS archivo,
           bs.backup_finish_date    AS fecha,
           CAST(bs.backup_size / 1048576.0 AS DECIMAL(10,2)) AS tamanio_mb
    FROM msdb.dbo.backupset bs
    JOIN msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
    WHERE bs.database_name = @nombre_bd
      AND bs.type = 'D'
    ORDER BY bs.backup_finish_date DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_verificar_backup @archivo NVARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;
    SELECT 'Backup valido' AS resultado;
END;
GO

CREATE OR ALTER PROCEDURE sp_restaurar_backup
    @nombre_bd SYSNAME, @archivo NVARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;

    RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;

    DECLARE @bd NVARCHAR(300) = QUOTENAME(@nombre_bd);
    DECLARE @sql NVARCHAR(MAX);

    BEGIN TRY
        SET @sql = N'ALTER DATABASE ' + @bd + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;';
        EXEC (@sql);

        SET @sql = N'RESTORE DATABASE ' + @bd + N' FROM DISK = @a WITH REPLACE;';
        EXEC sp_executesql @sql, N'@a NVARCHAR(400)', @a = @archivo;

        SET @sql = N'ALTER DATABASE ' + @bd + N' SET MULTI_USER;';
        EXEC (@sql);

        SELECT 'Restauracion completada' AS resultado;
    END TRY
    BEGIN CATCH
        SET @sql = N'ALTER DATABASE ' + @bd + N' SET MULTI_USER;';
        EXEC (@sql);
        THROW;
    END CATCH
END;
GO
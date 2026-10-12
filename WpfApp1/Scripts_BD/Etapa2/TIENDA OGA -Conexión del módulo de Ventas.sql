/* =====================================================================
   TIENDA OGA -Conexión del módulo de Ventas
   Script T-SQL (SQL Server)
   Requisito: haber ejecutado antes Script_TiendaOGA_Etapa2_TablasYProcedimientos.sql
   El script es re-ejecutable (CREATE OR ALTER / IF NOT EXISTS).
   ===================================================================== */

USE TiendaOgaDB;
GO


-- Secuencia para numero_venta (formato V-000001).
-- Arranca después del mayor número ya existente con ese formato.
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'seq_numero_venta')
BEGIN
    DECLARE @inicio INT = ISNULL(
        (SELECT MAX(TRY_CAST(SUBSTRING(numero_venta, 3, 10) AS INT))
         FROM dbo.venta_cabecera
         WHERE numero_venta LIKE 'V-%'), 0) + 1;

    DECLARE @sqlSeq NVARCHAR(200) =
        N'CREATE SEQUENCE dbo.seq_numero_venta AS INT START WITH '
        + CAST(@inicio AS NVARCHAR(10)) + N' INCREMENT BY 1;';
    EXEC (@sqlSeq);
END;
GO

-- Tipo tabla para enviar todos los ítems de la venta en una sola llamada
IF TYPE_ID('dbo.tipo_detalle_venta') IS NULL
    CREATE TYPE dbo.tipo_detalle_venta AS TABLE (
        id_producto INT NOT NULL,
        cantidad    INT NOT NULL CHECK (cantidad > 0)
    );
GO

/* ---------------------------------------------------------------------
   2) PROCEDIMIENTOS DE LECTURA PARA LA PANTALLA DE VENTAS
   --------------------------------------------------------------------- */

-- Productos disponibles para vender
CREATE OR ALTER PROCEDURE dbo.sp_producto_listar_activos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_producto, nombre, precio_ventas, stock
    FROM dbo.producto
    WHERE activo = 1
    ORDER BY nombre;
END;
GO

-- Un producto por código (búsqueda por ID)
CREATE OR ALTER PROCEDURE dbo.sp_producto_obtener @id_producto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_producto, nombre, precio_ventas, stock
    FROM dbo.producto
    WHERE id_producto = @id_producto AND activo = 1;
END;
GO

-- Tipos de pago habilitados (combo "Tipo de Pago")
CREATE OR ALTER PROCEDURE dbo.sp_tipo_pago_listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_tipo_pago, nombre_tipo_pago
    FROM dbo.tipo_pago
    WHERE activo = 1
    ORDER BY nombre_tipo_pago;
END;
GO

/* ---------------------------------------------------------------------
   3) REGISTRO COMPLETO DE LA VENTA (una sola transacción)
      Cabecera + detalle + descuento de stock + pago + total + estado
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_venta_registrar
    @id_usuario   INT,
    @id_cliente   INT,
    @id_tipo_pago INT,
    @detalle      dbo.tipo_detalle_venta READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Validaciones previas
        IF NOT EXISTS (SELECT 1 FROM @detalle)
            THROW 50010, 'La venta no tiene productos.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.cliente WHERE id_cliente = @id_cliente AND activo = 1)
            THROW 50011, 'El cliente no existe o esta dado de baja.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.tipo_pago WHERE id_tipo_pago = @id_tipo_pago AND activo = 1)
            THROW 50012, 'El tipo de pago no existe o esta inactivo.', 1;

        -- Agrupa por producto (venta_detalle tiene UNIQUE(id_venta, id_producto))
        DECLARE @items TABLE (id_producto INT PRIMARY KEY, cantidad INT NOT NULL);
        INSERT INTO @items (id_producto, cantidad)
        SELECT id_producto, SUM(cantidad) FROM @detalle GROUP BY id_producto;

        IF EXISTS (SELECT 1
                   FROM @items i
                   LEFT JOIN dbo.producto p ON p.id_producto = i.id_producto AND p.activo = 1
                   WHERE p.id_producto IS NULL)
            THROW 50013, 'Hay productos inexistentes o inactivos.', 1;

        BEGIN TRAN;

        -- Verifica stock bloqueando las filas hasta el final de la transacción
        DECLARE @faltante VARCHAR(30), @msg NVARCHAR(200);
        SELECT TOP 1 @faltante = p.nombre
        FROM dbo.producto p WITH (UPDLOCK, HOLDLOCK)
        JOIN @items i ON i.id_producto = p.id_producto
        WHERE p.stock < i.cantidad;

        IF @faltante IS NOT NULL
        BEGIN
            SET @msg = CONCAT('Stock insuficiente para "', @faltante, '".');
            THROW 50014, @msg, 1;
        END

        -- Descuenta stock
        UPDATE p
        SET p.stock = p.stock - i.cantidad
        FROM dbo.producto p
        JOIN @items i ON i.id_producto = p.id_producto;

        -- Cabecera con número de venta correlativo
        DECLARE @n INT = NEXT VALUE FOR dbo.seq_numero_venta;
        DECLARE @numero VARCHAR(20) = CONCAT('V-', RIGHT(CONCAT('000000', @n), 6));

        INSERT INTO dbo.venta_cabecera (id_usuario, id_cliente, numero_venta)
        VALUES (@id_usuario, @id_cliente, @numero);

        DECLARE @id_venta INT = SCOPE_IDENTITY();

        -- Detalle (el precio se toma de la base, no de la aplicación)
        INSERT INTO dbo.venta_detalle (id_venta, id_producto, cantidad, precio_unitario)
        SELECT @id_venta, i.id_producto, i.cantidad, p.precio_ventas
        FROM @items i
        JOIN dbo.producto p ON p.id_producto = i.id_producto;

        DECLARE @total DECIMAL(12,2) =
            (SELECT SUM(sub_total) FROM dbo.venta_detalle WHERE id_venta = @id_venta AND activo = 1);

        -- Pago y cierre de la venta
        INSERT INTO dbo.pago (id_venta, id_tipo_pago, monto)
        VALUES (@id_venta, @id_tipo_pago, @total);

        UPDATE dbo.venta_cabecera
        SET total_venta = @total, estado = 'PAGADA'
        WHERE id_venta = @id_venta;

        COMMIT;

        SELECT @id_venta AS id_venta, @numero AS numero_venta, @total AS total_venta;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO


/* ---------------------------------------------------------------------
   PROCEDIMIENTOS PARA LA FACTURACIÓN (Agregado)
   --------------------------------------------------------------------- */

USE TiendaOgaDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_factura_cabecera @id_venta INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.id_venta, v.numero_venta, v.fecha_venta, v.total_venta, v.estado,
           v.id_usuario,
           c.tipo_cliente, c.nombre_razon_social, c.tipo_documento, c.nro_documento
    FROM dbo.venta_cabecera v
    JOIN dbo.cliente c ON c.id_cliente = v.id_cliente
    WHERE v.id_venta = @id_venta AND v.activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_factura_detalle @id_venta INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.id_producto, p.nombre, d.cantidad, d.precio_unitario, d.sub_total
    FROM dbo.venta_detalle d
    JOIN dbo.producto p ON p.id_producto = d.id_producto
    WHERE d.id_venta = @id_venta AND d.activo = 1
    ORDER BY d.id_detalle;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_factura_pagos @id_venta INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT tp.nombre_tipo_pago, pg.monto, pg.fecha_pago
    FROM dbo.pago pg
    JOIN dbo.tipo_pago tp ON tp.id_tipo_pago = pg.id_tipo_pago
    WHERE pg.id_venta = @id_venta AND pg.activo = 1;
END;
GO


-------------sp_factura_cabecera para que devuelva el nombre del vendedor, que va a salir en la factura
CREATE OR ALTER PROCEDURE dbo.sp_usuario_obtener_id_por_login @usuario VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 id_usuario
    FROM dbo.Usuario
    WHERE Activo = 1
      AND (   usuario COLLATE Latin1_General_CI_AI = @usuario
           OR CONCAT(nombre, ' ', apellido) COLLATE Latin1_General_CI_AI = @usuario)
    ORDER BY id_usuario;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_factura_cabecera @id_venta INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.id_venta, v.numero_venta, v.fecha_venta, v.total_venta, v.estado,
           CONCAT(u.nombre, ' ', u.apellido) AS vendedor,
           c.tipo_cliente, c.nombre_razon_social, c.tipo_documento, c.nro_documento
    FROM dbo.venta_cabecera v
    JOIN dbo.cliente c ON c.id_cliente = v.id_cliente
    JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario
    WHERE v.id_venta = @id_venta AND v.activo = 1;
END;
GO

-- Prueba: el login debe ser uno que exista en tu tabla
EXEC dbo.sp_usuario_obtener_id_por_login 'admin';
/* =====================================================================
   4) PRUEBAS DE VALIDACIÓN (no dejan datos guardados)
      Toman automáticamente un usuario, cliente, tipo de pago y producto
      existentes, por lo que no hace falta editar IDs.
   ===================================================================== */

-- Prueba 1: Venta exitosa (Happy Path). Se revierte al final.
BEGIN TRAN;

DECLARE @u INT  = (SELECT TOP 1 id_usuario   FROM dbo.usuario   ORDER BY id_usuario);
DECLARE @c INT  = (SELECT TOP 1 id_cliente   FROM dbo.cliente   WHERE activo = 1 ORDER BY id_cliente);
DECLARE @tp INT = (SELECT TOP 1 id_tipo_pago FROM dbo.tipo_pago WHERE activo = 1 ORDER BY id_tipo_pago);
DECLARE @pr INT = (SELECT TOP 1 id_producto  FROM dbo.producto  WHERE activo = 1 AND stock >= 2 ORDER BY id_producto);

SELECT id_producto, stock AS stock_antes FROM dbo.producto WHERE id_producto = @pr;

DECLARE @d dbo.tipo_detalle_venta;
INSERT INTO @d (id_producto, cantidad) VALUES (@pr, 2);

EXEC dbo.sp_venta_registrar @id_usuario = @u, @id_cliente = @c, @id_tipo_pago = @tp, @detalle = @d;

SELECT TOP 1 * FROM dbo.venta_cabecera ORDER BY id_venta DESC;   -- estado PAGADA, total calculado
SELECT TOP 1 * FROM dbo.venta_detalle  ORDER BY id_detalle DESC; -- precio tomado de producto
SELECT TOP 1 * FROM dbo.pago           ORDER BY id_pago DESC;    -- monto = total de la venta
SELECT id_producto, stock AS stock_despues FROM dbo.producto WHERE id_producto = @pr; -- 2 unidades menos

ROLLBACK;
GO

-- Prueba 2: Stock insuficiente. Debe fallar con error 50014 y no dejar cambios.
BEGIN TRY
    DECLARE @u2 INT  = (SELECT TOP 1 id_usuario   FROM dbo.usuario   ORDER BY id_usuario);
    DECLARE @c2 INT  = (SELECT TOP 1 id_cliente   FROM dbo.cliente   WHERE activo = 1 ORDER BY id_cliente);
    DECLARE @tp2 INT = (SELECT TOP 1 id_tipo_pago FROM dbo.tipo_pago WHERE activo = 1 ORDER BY id_tipo_pago);
    DECLARE @pr2 INT = (SELECT TOP 1 id_producto  FROM dbo.producto  WHERE activo = 1 ORDER BY id_producto);
    DECLARE @cant INT = (SELECT stock + 1000 FROM dbo.producto WHERE id_producto = @pr2);

    DECLARE @d2 dbo.tipo_detalle_venta;
    INSERT INTO @d2 (id_producto, cantidad) VALUES (@pr2, @cant);

    EXEC dbo.sp_venta_registrar @id_usuario = @u2, @id_cliente = @c2, @id_tipo_pago = @tp2, @detalle = @d2;
    PRINT 'ERROR: la venta no deberia haberse registrado.';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS error, ERROR_MESSAGE() AS mensaje;  -- 50014
END CATCH
GO

-- Prueba 3: Venta sin productos. Debe fallar con error 50010.
BEGIN TRY
    DECLARE @d3 dbo.tipo_detalle_venta;
    EXEC dbo.sp_venta_registrar @id_usuario = 1, @id_cliente = 1, @id_tipo_pago = 1, @detalle = @d3;
    PRINT 'ERROR: la venta no deberia haberse registrado.';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS error, ERROR_MESSAGE() AS mensaje;  -- 50010
END CATCH
GO

-- Prueba 4: Cliente inexistente. Debe fallar con error 50011.
BEGIN TRY
    DECLARE @pr4 INT = (SELECT TOP 1 id_producto FROM dbo.producto WHERE activo = 1 ORDER BY id_producto);
    DECLARE @d4 dbo.tipo_detalle_venta;
    INSERT INTO @d4 (id_producto, cantidad) VALUES (@pr4, 1);
    EXEC dbo.sp_venta_registrar @id_usuario = 1, @id_cliente = -1, @id_tipo_pago = 1, @detalle = @d4;
    PRINT 'ERROR: la venta no deberia haberse registrado.';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS error, ERROR_MESSAGE() AS mensaje;  -- 50011
END CATCH
GO

-- Prueba 5: Tipo de pago inexistente. Debe fallar con error 50012.
BEGIN TRY
    DECLARE @c5 INT  = (SELECT TOP 1 id_cliente  FROM dbo.cliente  WHERE activo = 1 ORDER BY id_cliente);
    DECLARE @pr5 INT = (SELECT TOP 1 id_producto FROM dbo.producto WHERE activo = 1 ORDER BY id_producto);
    DECLARE @d5 dbo.tipo_detalle_venta;
    INSERT INTO @d5 (id_producto, cantidad) VALUES (@pr5, 1);
    EXEC dbo.sp_venta_registrar @id_usuario = 1, @id_cliente = @c5, @id_tipo_pago = -1, @detalle = @d5;
    PRINT 'ERROR: la venta no deberia haberse registrado.';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS error, ERROR_MESSAGE() AS mensaje;  -- 50012
END CATCH
GO

-- Prueba 6: Producto inexistente. Debe fallar con error 50013.
BEGIN TRY
    DECLARE @c6 INT = (SELECT TOP 1 id_cliente FROM dbo.cliente WHERE activo = 1 ORDER BY id_cliente);
    DECLARE @d6 dbo.tipo_detalle_venta;
    INSERT INTO @d6 (id_producto, cantidad) VALUES (-1, 1);
    EXEC dbo.sp_venta_registrar @id_usuario = 1, @id_cliente = @c6, @id_tipo_pago = 1, @detalle = @d6;
    PRINT 'ERROR: la venta no deberia haberse registrado.';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS error, ERROR_MESSAGE() AS mensaje;  -- 50013
END CATCH
GO

-- Verificación final: las pruebas no deben haber dejado ventas nuevas ni cambios de stock
SELECT COUNT(*) AS ventas_registradas FROM dbo.venta_cabecera;
GO
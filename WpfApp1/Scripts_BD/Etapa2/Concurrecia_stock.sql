/* =====================================================================
   04_concurrencia_stock.sql
   Correcciones de concurrencia sobre ventas / inventario.
   Re-ejecutable (CREATE OR ALTER).

   1) sp_anular_venta        -> OBLIGATORIO: evita devolver el stock dos veces
   2) sp_venta_registrar     -> OPCIONAL: el original ya es seguro; esta versión
                                no depende del plan de ejecución ni del HOLDLOCK
   3) Pruebas manuales (al final, comentadas)
   ===================================================================== */
USE TiendaOgaDB;
GO

/* ---------------------------------------------------------------------
   1) ANULAR VENTA  (reemplaza a la versión de
      Scripts_BD/Etapa2/sp_anular_venta_ modificar_ sp_cliente_historial.sql)

   Problema original: el IF NOT EXISTS solo "mira" el estado y no bloquea.
   Dos anulaciones simultáneas pueden pasar ese IF y las dos devolver el stock.
   Solución: el UPDATE del estado es la verificación. Solo uno lo logra
   (@@ROWCOUNT = 1); el otro obtiene 0 filas y falla sin tocar el stock.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_anular_venta
    @id_venta         INT,
    @id_usuario_anulo INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Verificación + cambio de estado en UNA sola sentencia atómica
        UPDATE dbo.venta_cabecera
        SET estado           = 'ANULADA',
            id_usuario_anulo = @id_usuario_anulo,
            fecha_anulacion  = GETDATE()
        WHERE id_venta = @id_venta
          AND estado  <> 'ANULADA'
          AND activo   = 1;

        IF @@ROWCOUNT = 0
            THROW 50001, 'La venta no existe o ya está anulada.', 1;

        -- Solo llega acá quien ganó la anulación: devuelve el stock
        UPDATE p
        SET p.stock = p.stock + d.cantidad
        FROM dbo.producto p
        JOIN dbo.venta_detalle d ON d.id_producto = p.id_producto
        WHERE d.id_venta = @id_venta AND d.activo = 1;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* ---------------------------------------------------------------------
   2) REGISTRAR VENTA (OPCIONAL)
      Misma lógica que la original, pero el descuento de stock es un
      UPDATE condicional atómico (WHERE stock >= cantidad), igual al patrón
      que ya usás en sp_agregar_detalle_venta.
      Además evita el HOLDLOCK (serializable), que si el optimizador elige
      un scan de producto bloquea el rango completo de la tabla y
      serializa TODAS las ventas.
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
        IF NOT EXISTS (SELECT 1 FROM @detalle)
            THROW 50010, 'La venta no tiene productos.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.cliente WHERE id_cliente = @id_cliente AND activo = 1)
            THROW 50011, 'El cliente no existe o esta dado de baja.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.tipo_pago WHERE id_tipo_pago = @id_tipo_pago AND activo = 1)
            THROW 50012, 'El tipo de pago no existe o esta inactivo.', 1;

        DECLARE @items TABLE (id_producto INT PRIMARY KEY, cantidad INT NOT NULL);
        INSERT INTO @items (id_producto, cantidad)
        SELECT id_producto, SUM(cantidad) FROM @detalle GROUP BY id_producto;

        IF EXISTS (SELECT 1
                   FROM @items i
                   LEFT JOIN dbo.producto p ON p.id_producto = i.id_producto AND p.activo = 1
                   WHERE p.id_producto IS NULL)
            THROW 50013, 'Hay productos inexistentes o inactivos.', 1;

        BEGIN TRAN;

        -- Descuento atómico: solo descuenta los productos que tienen stock suficiente
        DECLARE @ok TABLE (id_producto INT PRIMARY KEY);

        UPDATE p
        SET p.stock = p.stock - i.cantidad
        OUTPUT inserted.id_producto INTO @ok
        FROM dbo.producto p
        JOIN @items i ON i.id_producto = p.id_producto
        WHERE p.stock >= i.cantidad;

        -- Si algún producto no se pudo descontar, se cancela TODA la venta (rollback)
        IF (SELECT COUNT(*) FROM @ok) <> (SELECT COUNT(*) FROM @items)
        BEGIN
            DECLARE @faltante VARCHAR(30), @msg NVARCHAR(200);

            SELECT TOP 1 @faltante = p.nombre
            FROM @items i
            JOIN dbo.producto p ON p.id_producto = i.id_producto
            WHERE i.id_producto NOT IN (SELECT id_producto FROM @ok);

            SET @msg = CONCAT('Stock insuficiente para "', @faltante, '".');
            THROW 50014, @msg, 1;
        END

        DECLARE @n INT = NEXT VALUE FOR dbo.seq_numero_venta;
        DECLARE @numero VARCHAR(20) = CONCAT('V-', RIGHT(CONCAT('000000', @n), 6));

        INSERT INTO dbo.venta_cabecera (id_usuario, id_cliente, numero_venta)
        VALUES (@id_usuario, @id_cliente, @numero);

        DECLARE @id_venta INT = SCOPE_IDENTITY();

        INSERT INTO dbo.venta_detalle (id_venta, id_producto, cantidad, precio_unitario)
        SELECT @id_venta, i.id_producto, i.cantidad, p.precio_ventas
        FROM @items i
        JOIN dbo.producto p ON p.id_producto = i.id_producto;

        DECLARE @total DECIMAL(12,2) =
            (SELECT SUM(sub_total) FROM dbo.venta_detalle WHERE id_venta = @id_venta AND activo = 1);

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


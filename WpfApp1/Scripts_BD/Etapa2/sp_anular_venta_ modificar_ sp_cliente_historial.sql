USE TiendaOgaDB;
GO
--sp_anular_ventas
IF COL_LENGTH('dbo.venta_cabecera', 'id_usuario_anulo') IS NULL
    ALTER TABLE dbo.venta_cabecera ADD id_usuario_anulo INT NULL;
GO

IF COL_LENGTH('dbo.venta_cabecera', 'fecha_anulacion') IS NULL
    ALTER TABLE dbo.venta_cabecera ADD fecha_anulacion DATETIME NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_anular_venta
    @id_venta INT,
    @id_usuario_anulo INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.venta_cabecera
                       WHERE id_venta = @id_venta AND estado <> 'ANULADA' AND activo = 1)
            THROW 50001, 'La venta no existe o ya está anulada.', 1;

        UPDATE dbo.venta_cabecera
        SET estado = 'ANULADA',
            id_usuario_anulo = @id_usuario_anulo,
            fecha_anulacion = GETDATE()
        WHERE id_venta = @id_venta;

        --aca restauramos el stock
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



---MOPDIFICAR EL SP_CLIENTE_HISTORIAL
USE [TiendaOgaDB]
GO

ALTER PROCEDURE [dbo].[sp_cliente_historial] @id_cliente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.numero_venta, v.fecha_venta,
           (SELECT STRING_AGG(CONCAT(d.cantidad, ' x ', p.nombre), ', ')
            FROM dbo.venta_detalle d
            JOIN dbo.producto p ON p.id_producto = d.id_producto
            WHERE d.id_venta = v.id_venta AND d.activo = 1) AS articulos,
           (SELECT STRING_AGG(tp.nombre_tipo_pago, ', ')
            FROM dbo.pago pg
            JOIN dbo.tipo_pago tp ON tp.id_tipo_pago = pg.id_tipo_pago
            WHERE pg.id_venta = v.id_venta AND pg.activo = 1) AS metodo_pago,
           v.total_venta,
           v.estado,
           v.id_venta
    FROM dbo.venta_cabecera v
    WHERE v.id_cliente = @id_cliente AND v.activo = 1
    ORDER BY v.fecha_venta DESC;
END;
GO
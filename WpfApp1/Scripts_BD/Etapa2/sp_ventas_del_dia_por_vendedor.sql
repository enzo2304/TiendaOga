USE TiendaOgaDB;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ventas_del_dia_por_vendedor
    @usuario VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

    SELECT 
        v.numero_venta AS NroComprobante,
        FORMAT(v.fecha_venta, 'HH:mm') AS Hora,
        CONCAT(c.nombre_razon_social, ' (', c.tipo_documento, ': ', c.nro_documento, ')') AS NombreCliente,
        ISNULL((
            SELECT STRING_AGG(CONCAT(p.nombre, ' x', d.cantidad), ', ')
            FROM dbo.venta_detalle d
            JOIN dbo.producto p ON p.id_producto = d.id_producto
            WHERE d.id_venta = v.id_venta AND d.activo = 1
        ), '-') AS Detalle,
        ISNULL(tp.nombre_tipo_pago, 'Efectivo') AS MetodoPago,
        v.total_venta AS Total
    FROM dbo.venta_cabecera v
    JOIN dbo.cliente c ON c.id_cliente = v.id_cliente
    JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario
    LEFT JOIN dbo.pago pg ON pg.id_venta = v.id_venta AND pg.activo = 1
    LEFT JOIN dbo.tipo_pago tp ON tp.id_tipo_pago = pg.id_tipo_pago
    WHERE CAST(v.fecha_venta AS DATE) = @hoy
      AND v.estado <> 'ANULADA'
      AND v.activo = 1
      AND (
          @usuario IS NULL 
          OR @usuario = ''
          OR u.usuario COLLATE Latin1_General_CI_AI = @usuario
          OR CONCAT(u.nombre, ' ', u.apellido) COLLATE Latin1_General_CI_AI = @usuario
      )
    ORDER BY v.fecha_venta DESC;
END;
GO
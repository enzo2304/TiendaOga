USE TiendaOgaDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_reporte_rendimiento_vendedor
    @id_usuario INT,
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- BLOQUE 1: KPIs Generales (Ventas, Facturación, Ticket Promedio y Anulaciones)
    DECLARE @TotalVentas INT, @TotalAnuladas INT, @TotalFacturado DECIMAL(18,2);

    SELECT 
        @TotalVentas = COUNT(id_venta),
        @TotalFacturado = ISNULL(SUM(total_venta), 0)
    FROM dbo.venta_cabecera
    WHERE id_usuario = @id_usuario 
      AND CAST(fecha_venta AS DATE) >= @fecha_desde 
      AND CAST(fecha_venta AS DATE) <= @fecha_hasta
      AND estado <> 'ANULADA' AND activo = 1;

    SELECT @TotalAnuladas = COUNT(id_venta)
    FROM dbo.venta_cabecera
    WHERE id_usuario = @id_usuario 
      AND CAST(fecha_venta AS DATE) >= @fecha_desde 
      AND CAST(fecha_venta AS DATE) <= @fecha_hasta
      AND estado = 'ANULADA' AND activo = 1;

    SELECT 
        ISNULL(@TotalVentas, 0) AS CantidadVentas,
        ISNULL(@TotalFacturado, 0) AS VolumenFacturacion,
        CASE WHEN ISNULL(@TotalVentas, 0) > 0 THEN @TotalFacturado / @TotalVentas ELSE 0 END AS TicketPromedio,
        ISNULL(@TotalAnuladas, 0) AS CantidadAnuladas,
        CASE WHEN (@TotalVentas + @TotalAnuladas) > 0 
             THEN CAST(@TotalAnuladas AS FLOAT) / CAST((@TotalVentas + @TotalAnuladas) AS FLOAT) * 100 
             ELSE 0 END AS TasaAnulacion;

    -- BLOQUE 2: Desglose por Método de Pago
    SELECT 
        ISNULL(tp.nombre_tipo_pago, 'Sin Especificar') AS MetodoPago,
        ISNULL(SUM(v.total_venta), 0) AS TotalCobrado
    FROM dbo.venta_cabecera v
    JOIN dbo.pago pg ON v.id_venta = pg.id_venta AND pg.activo = 1
    JOIN dbo.tipo_pago tp ON pg.id_tipo_pago = tp.id_tipo_pago
    WHERE v.id_usuario = @id_usuario
      AND CAST(v.fecha_venta AS DATE) >= @fecha_desde 
      AND CAST(v.fecha_venta AS DATE) <= @fecha_hasta
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY tp.nombre_tipo_pago;

    -- BLOQUE 3: Top 5 Productos del Vendedor
    SELECT TOP 5
        p.nombre AS NombreProducto,
        SUM(d.cantidad) AS CantidadVendida,
        SUM(d.cantidad * d.precio_unitario) AS SubtotalGenerado
    FROM dbo.venta_cabecera v
    JOIN dbo.venta_detalle d ON v.id_venta = d.id_venta AND d.activo = 1
    JOIN dbo.producto p ON p.id_producto = d.id_producto
    WHERE v.id_usuario = @id_usuario
      AND CAST(v.fecha_venta AS DATE) >= @fecha_desde 
      AND CAST(v.fecha_venta AS DATE) <= @fecha_hasta
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY p.id_producto, p.nombre
    ORDER BY CantidadVendida DESC;
END;
GO
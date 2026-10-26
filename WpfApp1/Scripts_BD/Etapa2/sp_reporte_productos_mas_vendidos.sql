USE TiendaOgaDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_reporte_productos_mas_vendidos
    @mes INT,
    @anio INT
AS
BEGIN
    SET NOCOUNT ON;

    -- BLOQUE 1: Ranking (Unidades y Rentabilidad)
    SELECT 
        ROW_NUMBER() OVER(ORDER BY SUM(d.cantidad) DESC) AS Posicion,
        p.id_producto,
        p.nombre AS NombreProducto,
        c.nombre AS Categoria,
        SUM(d.cantidad) AS CantidadVendida,
        SUM(d.sub_total) AS TotalIngreso,
        -- Actualizado a p.precio_costo
        SUM(d.cantidad * ISNULL(p.precio_costo, 0)) AS CostoTotal,
        (SUM(d.sub_total) - SUM(d.cantidad * ISNULL(p.precio_costo, 0))) AS GananciaNeta,
        p.stock AS StockActual
    FROM dbo.venta_cabecera v
    JOIN dbo.venta_detalle d ON v.id_venta = d.id_venta
    JOIN dbo.producto p ON p.id_producto = d.id_producto
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    WHERE MONTH(v.fecha_venta) = @mes AND YEAR(v.fecha_venta) = @anio
      AND v.estado <> 'ANULADA' AND v.activo = 1 AND d.activo = 1
    GROUP BY p.id_producto, p.nombre, c.nombre, p.stock, p.precio_costo
    ORDER BY CantidadVendida DESC;

    -- BLOQUE 2: Métodos de Pago Generales
    SELECT 
        ISNULL(tp.nombre_tipo_pago, 'Sin Especificar') AS MetodoPago,
        ISNULL(SUM(v.total_venta), 0) AS TotalCobrado
    FROM dbo.venta_cabecera v
    JOIN dbo.pago pg ON v.id_venta = pg.id_venta AND pg.activo = 1
    JOIN dbo.tipo_pago tp ON pg.id_tipo_pago = tp.id_tipo_pago
    WHERE MONTH(v.fecha_venta) = @mes AND YEAR(v.fecha_venta) = @anio
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY tp.nombre_tipo_pago;

    -- BLOQUE 3: Métodos de Pago POR PRODUCTO (Para la interactividad)
    SELECT 
        d.id_producto,
        ISNULL(tp.nombre_tipo_pago, 'Sin Especificar') AS MetodoPago,
        SUM(d.sub_total) AS TotalCobrado
    FROM dbo.venta_cabecera v
    JOIN dbo.venta_detalle d ON v.id_venta = d.id_venta AND d.activo = 1
    JOIN dbo.pago pg ON v.id_venta = pg.id_venta AND pg.activo = 1
    JOIN dbo.tipo_pago tp ON pg.id_tipo_pago = tp.id_tipo_pago
    WHERE MONTH(v.fecha_venta) = @mes AND YEAR(v.fecha_venta) = @anio
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY d.id_producto, tp.nombre_tipo_pago;
END;
GO
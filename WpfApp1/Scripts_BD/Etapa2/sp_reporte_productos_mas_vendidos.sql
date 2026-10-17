USE TiendaOgaDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_reporte_productos_mas_vendidos
    @mes INT,
    @anio INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        -- ROW_NUMBER() nos genera el ranking (1, 2, 3...) automáticamente
        ROW_NUMBER() OVER(ORDER BY SUM(d.cantidad) DESC) AS Posicion,
        p.id_producto,
        p.nombre AS NombreProducto,
        c.nombre AS Categoria,
        SUM(d.cantidad) AS CantidadVendida,
        SUM(d.cantidad) * p.precio_ventas AS TotalIngreso,
        p.stock AS StockActual
    FROM dbo.venta_cabecera v
    JOIN dbo.venta_detalle d ON v.id_venta = d.id_venta
    JOIN dbo.producto p ON p.id_producto = d.id_producto
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    WHERE MONTH(v.fecha_venta) = @mes
      AND YEAR(v.fecha_venta) = @anio
      AND v.estado <> 'ANULADA'
      AND v.activo = 1
      AND d.activo = 1
    GROUP BY 
        p.id_producto, 
        p.nombre, 
        c.nombre, 
        p.precio_ventas, 
        p.stock
    ORDER BY CantidadVendida DESC;
END;
GO
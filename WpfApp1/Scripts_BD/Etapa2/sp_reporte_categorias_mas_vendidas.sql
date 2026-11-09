USE TiendaOgaDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_reporte_categorias_mas_vendidos
    @fechaInicio DATE,
    @fechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY SUM(d.cantidad) DESC) AS Posicion,
        c.id_categoria,
        c.nombre AS NombreCategoria,
        SUM(d.cantidad) AS TotalUnidadesVendidas,
        SUM(d.sub_total) AS TotalIngresoCategoria
    FROM dbo.venta_cabecera v
    JOIN dbo.venta_detalle d ON v.id_venta = d.id_venta AND d.activo = 1
    JOIN dbo.producto p ON p.id_producto = d.id_producto
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    WHERE CAST(v.fecha_venta AS DATE) BETWEEN @fechaInicio AND @fechaFin
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY c.id_categoria, c.nombre
    ORDER BY TotalUnidadesVendidas DESC;
END;
GO
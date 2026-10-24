USE TiendaOgaDB;
GO

/* Reporte general (panel gerencial). Devuelve 5 resultados en este orden:
   1) KPIs  2) Rentabilidad 6 meses  3) Medios de pago
   4) Ranking de vendedores  5) Stock crítico
   Sin parámetros usa el mes actual. */
CREATE OR ALTER PROCEDURE dbo.sp_reporte_general
    @mes  INT = NULL,
    @anio INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    --Establece el rango de fechas (@desde y @hasta) para el primer y último día del mes evaluado.

    DECLARE @desde DATE = DATEFROMPARTS(ISNULL(@anio, YEAR(GETDATE())),
                                        ISNULL(@mes,  MONTH(GETDATE())), 1);
    DECLARE @hasta DATE = DATEADD(MONTH, 1, @desde);

    -- 1) KPIs Indicadores Clave de Rendimiento) ----------------------------------------------------------
    SELECT
        ISNULL((SELECT SUM(d.cantidad * (d.precio_unitario - p.precio_costo))
                FROM dbo.venta_cabecera v
                JOIN dbo.venta_detalle d ON d.id_venta = v.id_venta AND d.activo = 1
                JOIN dbo.producto p      ON p.id_producto = d.id_producto
                WHERE v.fecha_venta >= @desde AND v.fecha_venta < @hasta
                  AND v.estado <> 'ANULADA' AND v.activo = 1), 0)      AS GananciaNeta,
        ISNULL((SELECT SUM(CAST(stock AS DECIMAL(18,2)) * precio_costo)
                FROM dbo.producto WHERE activo = 1), 0)                AS CapitalInventario,
        ISNULL((SELECT TOP 1 CONCAT(u.nombre, ' ', u.apellido)
                FROM dbo.venta_cabecera v
                JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario
                WHERE v.fecha_venta >= @desde AND v.fecha_venta < @hasta
                  AND v.estado <> 'ANULADA' AND v.activo = 1
                GROUP BY u.id_usuario, u.nombre, u.apellido
                ORDER BY SUM(v.total_venta) DESC), '-')                AS MejorVendedor,
        (SELECT COUNT(*) FROM dbo.producto
         WHERE activo = 1 AND stock <= stock_bajo)                     AS AlertasStock;

    -- 2) Rentabilidad: últimos 6 meses (incluye meses sin ventas) -------
    ;WITH meses AS (
        SELECT DATEADD(MONTH, -n, @desde) AS periodo
        FROM (VALUES (0),(1),(2),(3),(4),(5)) t(n)
    )
    SELECT m.periodo                                                    AS Periodo,
           ISNULL(SUM(d.sub_total), 0)                                  AS IngresosBrutos,
           ISNULL(SUM(d.cantidad * (d.precio_unitario - p.precio_costo)), 0) AS GananciaNeta
    FROM meses m
    LEFT JOIN dbo.venta_cabecera v
           ON v.fecha_venta >= m.periodo AND v.fecha_venta < DATEADD(MONTH, 1, m.periodo)
          AND v.estado <> 'ANULADA' AND v.activo = 1
    LEFT JOIN dbo.venta_detalle d ON d.id_venta = v.id_venta AND d.activo = 1
    LEFT JOIN dbo.producto p      ON p.id_producto = d.id_producto
    GROUP BY m.periodo
    ORDER BY m.periodo;

    -- 3) Ingresos por medio de pago (mes) -------------------------------
    SELECT tp.nombre_tipo_pago AS MetodoPago,
           SUM(pg.monto)       AS Total
    FROM dbo.pago pg
    JOIN dbo.venta_cabecera v ON v.id_venta = pg.id_venta
    JOIN dbo.tipo_pago tp     ON tp.id_tipo_pago = pg.id_tipo_pago
    WHERE pg.activo = 1
      AND v.fecha_venta >= @desde AND v.fecha_venta < @hasta
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY tp.nombre_tipo_pago
    ORDER BY Total DESC;

    -- 4) Ranking de vendedores (top 5 del mes) --------------------------
    SELECT TOP 5
           CONCAT(u.nombre, ' ', u.apellido) AS Vendedor,
           COUNT(v.id_venta)                 AS CantVentas,
           SUM(v.total_venta)                AS Recaudado
    FROM dbo.venta_cabecera v
    JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario
    WHERE v.fecha_venta >= @desde AND v.fecha_venta < @hasta
      AND v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY u.id_usuario, u.nombre, u.apellido
    ORDER BY Recaudado DESC;

    -- 5) Stock crítico ---------------------------------------------------
    SELECT TOP 10
           p.nombre AS Producto,
           c.nombre AS Categoria,
           p.stock  AS Stock,
           p.stock_bajo AS StockMinimo
    FROM dbo.producto p
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    WHERE p.activo = 1 AND p.stock <= p.stock_bajo
    ORDER BY p.stock, p.nombre;
END;
GO

-- Prueba
EXEC dbo.sp_reporte_general;
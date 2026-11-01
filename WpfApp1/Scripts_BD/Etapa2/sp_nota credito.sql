USE TiendaOgaDB;
GO

-- Requiere haber ejecutado 02_anulacion_ventas.sql (columnas id_usuario_anulo y fecha_anulacion)
CREATE OR ALTER PROCEDURE dbo.sp_nota_credito_extra
    @id_venta INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.fecha_anulacion,
        ISNULL(CONCAT(u.nombre, ' ', u.apellido), '-') AS anulador
    FROM dbo.venta_cabecera v
    LEFT JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario_anulo
    WHERE v.id_venta = @id_venta
      AND v.estado = 'ANULADA';
END;
GO
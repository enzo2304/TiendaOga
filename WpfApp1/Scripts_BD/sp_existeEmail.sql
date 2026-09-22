USE TiendaOgaDB;
GO

CREATE PROCEDURE dbo.sp_Existe_Email
    @email VARCHAR(100),
    @id_usuario_excluir INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM dbo.Usuario
    WHERE Email = @email
      AND (@id_usuario_excluir IS NULL OR id_usuario <> @id_usuario_excluir)
END
GO
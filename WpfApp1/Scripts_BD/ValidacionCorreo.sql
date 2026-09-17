CREATE OR ALTER PROCEDURE dbo.sp_Existe_Email
    @email VARCHAR(100),
    @id_usuario_excluir INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) 
    FROM usuario 
    WHERE email = @email
      AND (@id_usuario_excluir IS NULL OR id_usuario <> @id_usuario_excluir);
END;
GO
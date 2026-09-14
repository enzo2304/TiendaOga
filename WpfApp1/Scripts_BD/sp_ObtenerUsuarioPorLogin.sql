
-- sp_ObtenerUsuarioPorLogin: trae los datos necesarios para
-- validar el login (el hash de password se verifica en C# con
-- BCrypt, acá solo se consulta el registro).
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerUsuarioPorLogin
    @usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.nombre, u.apellido, u.Activo, u.password, p.nombre_perfil
    FROM Usuario u
    INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil
    WHERE u.usuario = @usuario;
END
GO
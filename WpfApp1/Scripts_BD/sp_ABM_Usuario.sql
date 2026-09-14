CREATE OR ALTER PROCEDURE dbo.sp_ABM_Usuario
    @Operacion  CHAR(1),          -- 'A' = Alta, 'B' = Baja, 'M' = Modificación, 'R' = Reactivar
    @id_usuario INT = NULL,
    @id_perfil  INT = NULL,
    @nombre     VARCHAR(100) = NULL,
    @apellido   VARCHAR(100) = NULL,
    @usuario    VARCHAR(50)  = NULL,
    @password   VARCHAR(255) = NULL,
    @email      VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Operacion = 'A'
    BEGIN
        INSERT INTO Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
        VALUES (@id_perfil, @nombre, @apellido, @usuario, @password, @email, 1);
    END
    ELSE IF @Operacion = 'B'
    BEGIN
        -- Baja lógica: no se borra la fila, se desactiva.
        UPDATE Usuario
        SET Activo = 0
        WHERE id_usuario = @id_usuario;
    END
    ELSE IF @Operacion = 'M'
    BEGIN
        UPDATE Usuario
        SET id_perfil = @id_perfil,
            nombre    = @nombre,
            apellido  = @apellido,
            usuario   = @usuario,
            password  = @password,
            email     = @email
        WHERE id_usuario = @id_usuario;
    END
    ELSE IF @Operacion = 'R'
    BEGIN
        -- Reactivación: usuario existente que estaba dado de baja.
        UPDATE Usuario
        SET Activo = 1
        WHERE id_usuario = @id_usuario;
    END
END
GO

-- ------------------------------------------------------------
-- sp_Listar_Perfiles_Activos: para llenar el ComboBox de Perfil
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Perfiles_Activos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id_perfil, nombre_perfil
    FROM Perfiles
    WHERE Activo = 1
    ORDER BY nombre_perfil;
END
GO

-- ------------------------------------------------------------
-- sp_Listar_Usuarios: para la grilla de usuarios registrados
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Usuarios
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.id_usuario, u.nombre, u.apellido, u.usuario, u.email, u.Activo, p.nombre_perfil
    FROM Usuario u
    INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil
    ORDER BY u.nombre, u.apellido;
END
GO

-- ------------------------------------------------------------
-- sp_Existe_Usuario: valida nombre de usuario duplicado
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Existe_Usuario
    @usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM Usuario
    WHERE usuario = @usuario;
END
GO
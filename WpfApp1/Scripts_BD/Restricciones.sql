USE TiendaOgaDB;
GO

-- 1. Restricciones en tabla usuario
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_usuario_login' AND object_id = OBJECT_ID('usuario'))
BEGIN
    ALTER TABLE usuario ADD CONSTRAINT UQ_usuario_login UNIQUE (usuario);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_usuario_email' AND object_id = OBJECT_ID('usuario'))
BEGIN
    ALTER TABLE usuario ADD CONSTRAINT UQ_usuario_email UNIQUE (email);
END;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_usuario_email_formato')
BEGIN
    ALTER TABLE usuario 
    ADD CONSTRAINT CK_usuario_email_formato 
    CHECK (email LIKE '%@%._%');
END;
GO

-- 2. Stored Procedure: sp_ObtenerUsuarioPorLogin
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerUsuarioPorLogin
    @login VARCHAR(50) = NULL,
    @usuario VARCHAR(50) = NULL,
    @p_login VARCHAR(50) = NULL,
    @nombre_usuario VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @filtro VARCHAR(50) = COALESCE(@login, @usuario, @p_login, @nombre_usuario);

    SELECT 
        u.id_usuario,
        u.usuario AS login,
        u.usuario,
        u.password AS password_hash,
        u.password,
        u.nombre,
        u.apellido,
        u.email,
        u.Activo AS estado,
        u.Activo,
        u.id_perfil,
        CASE 
            WHEN u.id_perfil = 1 THEN 'Administrador'
            WHEN u.id_perfil = 2 THEN 'Vendedor'
            ELSE 'Usuario'
        END AS nombre_perfil,
        CASE 
            WHEN u.id_perfil = 1 THEN 'Administrador'
            WHEN u.id_perfil = 2 THEN 'Vendedor'
            ELSE 'Usuario'
        END AS rol
    FROM usuario u
    WHERE u.usuario = @filtro;
END;
GO

-- 3. Stored Procedure: sp_Listar_Perfiles_Activos
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Perfiles_Activos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 1 AS id_perfil, 'Administrador' AS nombre_perfil
    UNION ALL
    SELECT 2 AS id_perfil, 'Vendedor' AS nombre_perfil
    UNION ALL
    SELECT 3 AS id_perfil, 'Gerente' AS nombre_perfil;
END;
GO

-- 4. Stored Procedure: sp_Listar_Usuarios
CREATE OR ALTER PROCEDURE dbo.sp_Listar_Usuarios
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        u.id_usuario,
        u.usuario,
        u.usuario AS login,
        u.nombre,
        u.apellido,
        u.email,
        u.id_perfil,
        CASE 
            WHEN u.id_perfil = 1 THEN 'Administrador'
            WHEN u.id_perfil = 2 THEN 'Vendedor'
            WHEN u.id_perfil = 3 THEN 'Gerente'
            ELSE 'Usuario'
        END AS nombre_perfil,
        u.Activo,
        u.Activo AS estado
    FROM usuario u;
END;
GO

-- 5. Stored Procedure: sp_Existe_Usuario
CREATE OR ALTER PROCEDURE dbo.sp_Existe_Usuario
    @usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM usuario WHERE usuario = @usuario;
END;
GO

-- 6. Stored Procedure: sp_ABM_Usuario
CREATE OR ALTER PROCEDURE dbo.sp_ABM_Usuario
    @operacion CHAR(1),
    @id_usuario INT = NULL,
    @id_perfil INT = NULL,
    @nombre VARCHAR(50) = NULL,
    @apellido VARCHAR(50) = NULL,
    @usuario VARCHAR(50) = NULL,
    @login VARCHAR(50) = NULL,
    @password VARCHAR(255) = NULL,
    @email VARCHAR(100) = NULL,
    @activo BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @usuarioFinal VARCHAR(50) = COALESCE(@usuario, @login);

    IF @operacion = 'A'
    BEGIN
        INSERT INTO usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
        VALUES (@id_perfil, @nombre, @apellido, @usuarioFinal, @password, @email, 1);
    END
    ELSE IF @operacion = 'M'
    BEGIN
        UPDATE usuario
        SET id_perfil = COALESCE(@id_perfil, id_perfil),
            nombre = COALESCE(@nombre, nombre),
            apellido = COALESCE(@apellido, apellido),
            usuario = COALESCE(@usuarioFinal, usuario),
            email = COALESCE(@email, email),
            password = CASE WHEN @password IS NOT NULL AND @password <> '' THEN @password ELSE password END
        WHERE id_usuario = @id_usuario;
    END
    ELSE IF @operacion = 'B'
    BEGIN
        UPDATE usuario SET Activo = 0 WHERE id_usuario = @id_usuario;
    END
    ELSE IF @operacion IN ('R', 'H')
    BEGIN
        UPDATE usuario SET Activo = 1 WHERE id_usuario = @id_usuario;
    END
END;
GO
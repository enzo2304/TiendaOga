-- =============================================
-- 1. CREACIÓN DE LA BASE DE DATOS
-- =============================================
CREATE DATABASE TiendaOgaDB;
GO

USE TiendaOgaDB;
GO

-- =============================================
-- 2. CREACIÓN DE TABLAS
-- =============================================

-- Tabla Perfiles (Se crea primero porque no depende de nadie)
CREATE TABLE Perfiles (
    id_perfil INT IDENTITY(1,1) PRIMARY KEY,
    nombre_perfil VARCHAR(50) NOT NULL,
    descripcion VARCHAR(255) NULL,
    Activo BIT DEFAULT 1
);
GO

-- Tabla Usuario (Se crea segunda porque depende de Perfiles)
CREATE TABLE Usuario (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_perfil INT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    usuario VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100) NULL,
    Activo BIT DEFAULT 1,
    
    -- Relación (Clave Foránea) con la tabla Perfiles
    CONSTRAINT FK_Usuario_Perfil FOREIGN KEY (id_perfil) REFERENCES Perfiles(id_perfil)
);
GO

-- =============================================
-- 3. PROCEDIMIENTOS ALMACENADOS
-- =============================================

-- ABM para Perfiles
CREATE PROCEDURE dbo.sp_ABM_Perfiles (
    @Operacion CHAR(1),                -- 'A': Alta, 'B': Baja, 'M': Modificación
    @id_perfil INT = NULL,             -- Requerido para Baja y Modificación
    @nombre_perfil VARCHAR(50) = NULL, -- Requerido para Alta y Modificación
    @descripcion VARCHAR(255) = NULL
)
AS
BEGIN
    -- ALTA (Insertar nuevo perfil)
    IF @Operacion = 'A'
    BEGIN
        INSERT INTO Perfiles (nombre_perfil, descripcion, Activo)
        VALUES (@nombre_perfil, @descripcion, 1);
    END
    
    -- MODIFICACIÓN (Actualizar un perfil existente)
    ELSE IF @Operacion = 'M'
    BEGIN
        UPDATE Perfiles
        SET nombre_perfil = @nombre_perfil,
            descripcion = @descripcion
        WHERE id_perfil = @id_perfil;
    END
    
    -- BAJA (Baja lógica, lo desactivamos en lugar de borrarlo)
    ELSE IF @Operacion = 'B'
    BEGIN
        UPDATE Perfiles
        SET Activo = 0
        WHERE id_perfil = @id_perfil;
    END
END
GO

-- =============================================
-- 4. DATOS DE PRUEBA (SEED DATA)
-- =============================================
INSERT INTO Perfiles (nombre_perfil, descripcion, Activo)
VALUES ('Administrador', 'Perfil con acceso total al sistema', 1);

INSERT INTO Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
VALUES (1, 'Enzo', 'Sanchez', 'admin', '1234', 'enzo@admin.com', 1);
GO
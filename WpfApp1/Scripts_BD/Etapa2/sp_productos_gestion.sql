USE TiendaOgaDB;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* =========================================================================
   1. LISTAR TODOS LOS PRODUCTOS (Activos e Inactivos) CON SU RUBRO
   ========================================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_producto_listar_catalogo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.id_producto,
        p.nombre AS nombre_producto,
        c.nombre AS nombre_categoria,
        p.precio_costo,
        p.precio_ventas,
        p.stock,
        CASE 
            WHEN th.id_producto IS NOT NULL THEN 'Hogar'
            WHEN tt.id_producto IS NOT NULL THEN 'Tecnología'
            ELSE c.nombre 
        END AS tipo_producto,
        p.activo
    FROM dbo.producto p
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    LEFT JOIN dbo.producto_hogar th ON th.id_producto = p.id_producto
    LEFT JOIN dbo.producto_tecnologia tt ON tt.id_producto = p.id_producto
    ORDER BY p.activo DESC, p.nombre ASC;
END;
GO

/* =========================================================================
   2. OBTENER UN PRODUCTO POR ID (Incluye datos específicos de Hogar/Tecno)
   ========================================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_producto_obtener_detalle
    @id_producto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.id_producto,
        p.nombre AS nombre_producto,
        c.nombre AS nombre_categoria,
        p.precio_costo,
        p.precio_ventas,
        p.stock,
        CASE 
            WHEN th.id_producto IS NOT NULL THEN 'Hogar'
            WHEN tt.id_producto IS NOT NULL THEN 'Tecnología'
            ELSE c.nombre 
        END AS tipo_producto,
        p.activo,
        -- Datos de Hogar
        th.material,
        th.dimensiones,
        th.peso_kg AS peso,
        th.ambiente,
        -- Datos de Tecnología
        tt.marca,
        tt.modelo,
        tt.garantia_meses,
        tt.voltaje
    FROM dbo.producto p
    JOIN dbo.categoria c ON c.id_categoria = p.id_categoria
    LEFT JOIN dbo.producto_hogar th ON th.id_producto = p.id_producto
    LEFT JOIN dbo.producto_tecnologia tt ON tt.id_producto = p.id_producto
    WHERE p.id_producto = @id_producto;
END;
GO

/* =========================================================================
   3. GUARDAR O MODIFICAR PRODUCTO CON TRANSACCIÓN Y VALIDACIÓN ATÓMICA
   ========================================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_producto_guardar_o_modificar
    @id_producto        INT = NULL, -- NULL si es Nuevo, número si es Modificar
    @nombre   VARCHAR(30),
    @precio_costo       DECIMAL(12,2),
    @precio_ventas      DECIMAL(12,2),
    @stock              INT,
    @tipo_producto      VARCHAR(50), -- 'Hogar' o 'Tecnología'
    -- Atributos Hogar
    @material           VARCHAR(50) = NULL,
    @dimensiones        VARCHAR(50) = NULL,
    @peso               DECIMAL(8,2) = NULL,
    @ambiente           VARCHAR(50) = NULL,
    -- Atributos Tecnología
    @marca              VARCHAR(50) = NULL,
    @modelo             VARCHAR(50) = NULL,
    @garantia_meses     INT = NULL,
    @voltaje  VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Validaciones de integridad en el motor SQL
        IF @precio_costo <= 0
            THROW 51001, 'El precio de costo debe ser mayor a cero.', 1;

        IF @precio_ventas < @precio_costo
            THROW 51002, 'El precio de venta no puede ser inferior al precio de costo.', 1;

        IF @stock < 0
            THROW 51003, 'El stock no puede ser negativo.', 1;

        -- Determinar categoría ID según el tipo
        DECLARE @id_cat INT;
        SELECT TOP 1 @id_cat = id_categoria 
        FROM dbo.categoria 
        WHERE nombre LIKE '%' + @tipo_producto + '%';

        IF @id_cat IS NULL
            SELECT TOP 1 @id_cat = id_categoria FROM dbo.categoria ORDER BY id_categoria;

        BEGIN TRAN;

        IF @id_producto IS NULL OR @id_producto = 0
        BEGIN
            -- INSERT en producto base
            INSERT INTO dbo.producto (id_categoria, nombre, precio_costo, precio_ventas, stock, activo)
            VALUES (@id_cat, @nombre, @precio_costo, @precio_ventas, @stock, 1);

            SET @id_producto = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- UPDATE en producto base
            UPDATE dbo.producto
            SET id_categoria  = @id_cat,
                nombre        = @nombre,
                precio_costo  = @precio_costo,
                precio_ventas = @precio_ventas,
                stock         = @stock
            WHERE id_producto = @id_producto;
        END

        -- Gestionar subtipos (Hogar / Tecnología)
        IF @tipo_producto = 'Hogar'
        BEGIN
            -- Si existía en tecnología, lo quitamos
            DELETE FROM dbo.producto_tecnologia WHERE id_producto = @id_producto;

            IF EXISTS (SELECT 1 FROM dbo.producto_hogar WHERE id_producto = @id_producto)
            BEGIN
                UPDATE dbo.producto_hogar
                    SET material    = @material,
                        dimensiones = @dimensiones,
                        peso_kg     = @peso,
                        ambiente    = @ambiente
                    WHERE id_producto = @id_producto;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.producto_hogar (id_producto, material, dimensiones, peso_kg, ambiente)
                VALUES (@id_producto, @material, @dimensiones, @peso, @ambiente);
            END
        END
        ELSE IF @tipo_producto = 'Tecnología'
        BEGIN
            -- Si existía en hogar, lo quitamos
            DELETE FROM dbo.producto_hogar WHERE id_producto = @id_producto;

            IF EXISTS (SELECT 1 FROM dbo.producto_tecnologia WHERE id_producto = @id_producto)
            BEGIN
                UPDATE dbo.producto_tecnologia
                SET marca          = @marca,
                    modelo         = @modelo,
                    garantia_meses = ISNULL(@garantia_meses, 0),
                    voltaje        = @voltaje
                WHERE id_producto  = @id_producto;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.producto_tecnologia (id_producto, marca, modelo, garantia_meses, voltaje)
                VALUES (@id_producto, @marca, @modelo, ISNULL(@garantia_meses, 0), @voltaje);
            END
        END

        COMMIT;
        SELECT @id_producto AS id_producto;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* =========================================================================
   4. BAJA LÓGICA / REACTIVACIÓN DE PRODUCTO
   ========================================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_producto_cambiar_activo
    @id_producto INT,
    @activo      BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.producto
    SET activo = @activo
    WHERE id_producto = @id_producto;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO
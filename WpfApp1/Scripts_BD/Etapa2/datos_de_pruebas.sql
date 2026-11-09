/* =====================================================================
   06_datos_de_prueba_dashboards.sql
   Datos de prueba (mock) para los dashboards estadisticos de TiendaOgaDB.

   - Se PUEDE ejecutar mas de una vez: no duplica usuarios, clientes ni ventas.
   - Respeta el orden: parametricas -> entidades -> transaccionales.
   - Reutiliza lo que YA tenes cargado (2 clientes, los productos de tu tabla,
     usuarios 1 y 2). Solo agrega lo que falta.
   - Carga 26 ventas (14 en octubre y 12 en noviembre de 2026), 2 anuladas.
   - Todo corre en una transaccion: si algo falla, no queda nada a medias.

   IMPORTANTE: las ventas se insertan directo en las tablas, sin pasar por
   sp_venta_registrar, asi que NO descuentan stock. El stock final de los
   productos clave se fija a mano en el paso 6 (para las alertas).
   ===================================================================== */
USE TiendaOgaDB;
GO

/* ---- Paso 0: columnas de anulacion (las crea 02_anulacion_ventas.sql) ---- */
IF COL_LENGTH('dbo.venta_cabecera', 'id_usuario_anulo') IS NULL
    ALTER TABLE dbo.venta_cabecera ADD id_usuario_anulo INT NULL;
GO
IF COL_LENGTH('dbo.venta_cabecera', 'fecha_anulacion') IS NULL
    ALTER TABLE dbo.venta_cabecera ADD fecha_anulacion DATETIME NULL;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    /* =================================================================
       1) PARAMETRICAS: Perfiles, tipo_pago, categoria
       ================================================================= */
    INSERT INTO dbo.Perfiles (nombre_perfil, descripcion, Activo)
    SELECT v.nombre, v.descripcion, 1
    FROM (VALUES
        ('Administrador', 'Perfil con acceso total al sistema'),
        ('Gerente',       'Perfil de gestión con permisos intermedios'),
        ('Vendedor',      'Perfil de ventas con acceso limitado')
    ) AS v(nombre, descripcion)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Perfiles p WHERE p.nombre_perfil COLLATE Latin1_General_CI_AI = v.nombre COLLATE Latin1_General_CI_AI);

    INSERT INTO dbo.tipo_pago (nombre_tipo_pago, descripcion)
    SELECT v.nombre, v.descripcion
    FROM (VALUES
        ('Efectivo',      'Pago al contado con dinero físico'),
        ('Transferencia', 'Transferencia bancaria o billetera virtual')
    ) AS v(nombre, descripcion)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tipo_pago t WHERE t.nombre_tipo_pago COLLATE Latin1_General_CI_AI = v.nombre COLLATE Latin1_General_CI_AI);

    INSERT INTO dbo.categoria (nombre, descripcion)
    SELECT v.nombre, v.descripcion
    FROM (VALUES
        ('Herramientas', 'Herramientas eléctricas y manuales'),
        ('Bazar',        'Artículos de cocina y hogar'),
        ('Limpieza',     'Productos y accesorios de limpieza'),
        ('Tecnología',   'Iluminación inteligente y seguridad')
    ) AS v(nombre, descripcion)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.categoria c WHERE c.nombre COLLATE Latin1_General_CI_AI = v.nombre COLLATE Latin1_General_CI_AI);

    /* =================================================================
       2) ENTIDADES: vendedores (Usuario)
          Si Juan Perez o Lucas Martinez Quarta ya existen, se reutilizan.
          Si no existen, se crean con la clave 'Vendedor123' (hash BCrypt).
       ================================================================= */
    DECLARE @id_perfil_vendedor INT =
        (SELECT TOP 1 id_perfil FROM dbo.Perfiles WHERE nombre_perfil COLLATE Latin1_General_CI_AI = 'Vendedor' ORDER BY id_perfil);
    DECLARE @hash VARCHAR(255) = '$2a$11$JiOyUP1NkclhT9EHd6mFReDqeT0bjBF5raztYNaPmQjWO0IO9XdY.';   -- BCrypt de 'Vendedor123'

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE CONCAT(nombre, ' ', apellido) LIKE 'Juan%P_rez%')
       AND NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE usuario = 'jperez')
        INSERT INTO dbo.Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
        VALUES (@id_perfil_vendedor, 'Juan', 'Perez', 'jperez', @hash, 'juan.perez@tiendaoga.com', 1);

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE CONCAT(nombre, ' ', apellido) LIKE 'Lucas%Mart_nez%Quarta%')
       AND NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE usuario = 'lquarta')
        INSERT INTO dbo.Usuario (id_perfil, nombre, apellido, usuario, password, email, Activo)
        VALUES (@id_perfil_vendedor, 'Lucas', 'Martínez Quarta', 'lquarta', @hash, 'lucas.quarta@tiendaoga.com', 1);

    DECLARE @id_juan  INT = (SELECT TOP 1 id_usuario FROM dbo.Usuario
                             WHERE CONCAT(nombre, ' ', apellido) LIKE 'Juan%P_rez%' ORDER BY id_usuario);
    DECLARE @id_lucas INT = (SELECT TOP 1 id_usuario FROM dbo.Usuario
                             WHERE CONCAT(nombre, ' ', apellido) LIKE 'Lucas%Mart_nez%Quarta%' ORDER BY id_usuario);

    IF @id_juan IS NULL OR @id_lucas IS NULL
        THROW 50100, 'No se pudieron resolver los vendedores Juan Perez / Lucas Martinez Quarta.', 1;

    /* =================================================================
       3) ENTIDADES: clientes (ya tenias 2; se agregan 9 mas)
       ================================================================= */
    INSERT INTO dbo.cliente (tipo_cliente, nombre_razon_social, tipo_documento, nro_documento, telefono)
    SELECT v.tipo_cliente, v.nombre, v.tipo_doc, v.nro_doc, v.telefono
    FROM (VALUES
        ('PERSONA', 'María Fernández',               'DNI',  '30124567',    '3794-112233'),
        ('PERSONA', 'Carlos Benítez',                'DNI',  '28456789',    '3794-223344'),
        ('PERSONA', 'Lucía Gómez',                   'DNI',  '35678912',    '3794-334455'),
        ('PERSONA', 'Martín Acosta',                 'DNI',  '33219876',    '3794-445566'),
        ('PERSONA', 'Valeria Romero',                'DNI',  '38765432',    '3794-556677'),
        ('PERSONA', 'Diego Sánchez',                 'DNI',  '31987654',    '3794-667788'),
        ('PERSONA', 'Sofía Duarte',                  'DNI',  '40213345',    '3794-778899'),
        ('EMPRESA', 'Ferretería El Tornillo S.R.L.', 'CUIT', '30712345678', '3794-889900'),
        ('EMPRESA', 'Hogar y Confort S.A.',          'CUIT', '30698765432', '3794-990011')
    ) AS v(tipo_cliente, nombre, tipo_doc, nro_doc, telefono)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.cliente c
                      WHERE c.tipo_documento = v.tipo_doc AND c.nro_documento = v.nro_doc);

    /* =================================================================
       4) ENTIDADES: productos
          Tus productos ya existen (y hay copias repetidas por haber corrido
          el script de poblado mas de una vez). Se usa UNA copia por nombre:
          la de menor id_producto que este activa.
       ================================================================= */
    DECLARE @prod TABLE (
        clave       CHAR(3)       NOT NULL PRIMARY KEY,
        nombre      VARCHAR(30)   NOT NULL,
        id_producto INT           NULL,
        precio      DECIMAL(12,2) NULL
    );

    INSERT INTO @prod (clave, nombre) VALUES
        ('TAL', 'Taladro Percutor 650W'),
        ('OLL', 'Juego de Ollas 5 piezas'),
        ('ESC', 'Escoba de cerdas duras'),
        ('FOC', 'Foco Inteligente LED Wi-Fi'),
        ('CAM', 'Cámara de Seguridad Interior'),
        ('CAL', 'calculador casio fx-570 PLUS'),
        ('WAV', 'WAVE100');

    UPDATE x
    SET x.id_producto = p.id_producto,
        x.precio      = p.precio_ventas
    FROM @prod x
    CROSS APPLY (SELECT TOP 1 id_producto, precio_ventas
                 FROM dbo.producto
                 WHERE nombre COLLATE Latin1_General_CI_AI = x.nombre COLLATE Latin1_General_CI_AI AND activo = 1
                 ORDER BY id_producto) AS p;

    -- Productos obligatorios del pedido: si falta alguno, se crea.
    INSERT INTO dbo.producto (id_categoria, nombre, precio_costo, precio_ventas, stock)
    SELECT (SELECT TOP 1 id_categoria FROM dbo.categoria WHERE nombre COLLATE Latin1_General_CI_AI = v.categoria COLLATE Latin1_General_CI_AI ORDER BY id_categoria),
           v.nombre, v.costo, v.precio, v.stock
    FROM (VALUES
        ('Herramientas', 'Taladro Percutor 650W',        45000, 65000, 10),
        ('Bazar',        'Juego de Ollas 5 piezas',      30000, 45000,  5),
        ('Limpieza',     'Escoba de cerdas duras',        1800,  3000, 30),
        ('Tecnología',   'Foco Inteligente LED Wi-Fi',    9000, 14000, 40),
        ('Tecnología',   'Cámara de Seguridad Interior', 35000, 52000, 12)
    ) AS v(categoria, nombre, costo, precio, stock)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.producto p WHERE p.nombre COLLATE Latin1_General_CI_AI = v.nombre COLLATE Latin1_General_CI_AI AND p.activo = 1);

    -- Se vuelve a resolver por si se creo alguno recien
    UPDATE x
    SET x.id_producto = p.id_producto,
        x.precio      = p.precio_ventas
    FROM @prod x
    CROSS APPLY (SELECT TOP 1 id_producto, precio_ventas
                 FROM dbo.producto
                 WHERE nombre COLLATE Latin1_General_CI_AI = x.nombre COLLATE Latin1_General_CI_AI AND activo = 1
                 ORDER BY id_producto) AS p;

    -- Extension HOGAR (solo si el producto no esta ya en hogar ni en tecnologia)
    INSERT INTO dbo.producto_hogar (id_producto, material, dimensiones, peso_kg, ambiente)
    SELECT x.id_producto, v.material, v.dimensiones, v.peso, v.ambiente
    FROM (VALUES
        ('OLL', 'Acero inoxidable',  'Set de 5 piezas', 4.20, 'Cocina'),
        ('ESC', 'Plástico y fibra',  '120 x 30 cm',     0.60, 'Limpieza')
    ) AS v(clave, material, dimensiones, peso, ambiente)
    JOIN @prod x ON x.clave = v.clave
    WHERE x.id_producto IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.producto_hogar h       WHERE h.id_producto = x.id_producto)
      AND NOT EXISTS (SELECT 1 FROM dbo.producto_tecnologia t  WHERE t.id_producto = x.id_producto);

    -- Extension TECNOLOGIA
    INSERT INTO dbo.producto_tecnologia (id_producto, marca, modelo, garantia_meses, voltaje)
    SELECT x.id_producto, v.marca, v.modelo, v.garantia, v.voltaje
    FROM (VALUES
        ('FOC', 'Philips',      'Wiz A60 9W',   12, '220V'),
        ('CAM', 'TP-Link',      'Tapo C200',    24, '5V DC'),
        ('TAL', 'Black+Decker', 'TP650',        12, '220V'),
        ('CAL', 'Casio',        'fx-570 PLUS',  12, 'Solar')
    ) AS v(clave, marca, modelo, garantia, voltaje)
    JOIN @prod x ON x.clave = v.clave
    WHERE x.id_producto IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.producto_tecnologia t WHERE t.id_producto = x.id_producto)
      AND NOT EXISTS (SELECT 1 FROM dbo.producto_hogar h      WHERE h.id_producto = x.id_producto);

    /* =================================================================
       5) TRANSACCIONALES: ventas, detalle y pagos
          V = cabeceras (J = Juan, L = Lucas; el cliente se busca por documento)
          D = detalle (nro de venta, producto, cantidad)
          Lucas vende mas cantidad y de mayor valor => 'Mejor vendedor'.
       ================================================================= */
    DECLARE @V TABLE (
        nro      INT          NOT NULL PRIMARY KEY,
        fecha    DATETIME     NOT NULL,
        vendedor CHAR(1)      NOT NULL,
        dni      VARCHAR(20)  NOT NULL,
        pago     VARCHAR(30)  NOT NULL,
        anulada  BIT          NOT NULL
    );

    INSERT INTO @V (nro, fecha, vendedor, dni, pago, anulada) VALUES
    (1, '20261002 10:20', 'J', '41777777', 'Efectivo', 0),
    (2, '20261003 11:45', 'L', '30124567', 'Transferencia', 0),
    (3, '20261005 16:05', 'L', '28456789', 'Efectivo', 0),
    (4, '20261007 09:40', 'J', '22222222', 'Efectivo', 0),
    (5, '20261008 18:30', 'L', '35678912', 'Transferencia', 0),
    (6, '20261010 12:15', 'J', '33219876', 'Efectivo', 0),
    (7, '20261013 17:50', 'L', '38765432', 'Efectivo', 0),
    (8, '20261015 10:05', 'L', '30712345678', 'Transferencia', 0),
    (9, '20261017 19:10', 'J', '31987654', 'Efectivo', 0),
    (10, '20261020 13:25', 'L', '40213345', 'Efectivo', 0),
    (11, '20261022 15:40', 'J', '41777777', 'Transferencia', 1),
    (12, '20261024 11:00', 'L', '22222222', 'Efectivo', 0),
    (13, '20261027 16:30', 'L', '30124567', 'Transferencia', 0),
    (14, '20261029 10:55', 'J', '28456789', 'Efectivo', 0),
    (15, '20261102 10:10', 'L', '35678912', 'Efectivo', 0),
    (16, '20261102 17:20', 'J', '33219876', 'Transferencia', 0),
    (17, '20261103 12:35', 'L', '30698765432', 'Transferencia', 0),
    (18, '20261104 09:50', 'L', '38765432', 'Efectivo', 0),
    (19, '20261104 18:15', 'J', '40213345', 'Efectivo', 0),
    (20, '20261105 11:30', 'L', '41777777', 'Transferencia', 0),
    (21, '20261106 14:45', 'J', '31987654', 'Efectivo', 0),
    (22, '20261106 19:00', 'L', '22222222', 'Efectivo', 1),
    (23, '20261107 10:25', 'L', '30712345678', 'Efectivo', 0),
    (24, '20261107 18:10', 'J', '28456789', 'Transferencia', 0),
    (25, '20261108 16:40', 'L', '30124567', 'Efectivo', 0),
    (26, '20261108 19:20', 'J', '35678912', 'Transferencia', 0);

    DECLARE @D TABLE (
        nro      INT      NOT NULL,
        clave    CHAR(3)  NOT NULL,
        cantidad INT      NOT NULL,
        PRIMARY KEY (nro, clave)
    );

    INSERT INTO @D (nro, clave, cantidad) VALUES
    (1, 'ESC', 2),
    (1, 'FOC', 1),
    (2, 'CAM', 1),
    (2, 'FOC', 2),
    (3, 'TAL', 1),
    (4, 'OLL', 1),
    (4, 'ESC', 1),
    (5, 'CAL', 1),
    (6, 'FOC', 3),
    (7, 'TAL', 1),
    (7, 'ESC', 2),
    (8, 'TAL', 2),
    (9, 'WAV', 2),
    (10, 'CAM', 1),
    (11, 'OLL', 1),
    (11, 'FOC', 1),
    (12, 'OLL', 2),
    (13, 'CAM', 2),
    (13, 'FOC', 2),
    (14, 'ESC', 4),
    (15, 'TAL', 1),
    (15, 'FOC', 1),
    (16, 'OLL', 1),
    (17, 'OLL', 3),
    (17, 'ESC', 3),
    (18, 'CAL', 1),
    (19, 'FOC', 2),
    (19, 'WAV', 1),
    (20, 'CAM', 1),
    (20, 'TAL', 1),
    (21, 'ESC', 2),
    (22, 'CAM', 1),
    (23, 'TAL', 2),
    (23, 'ESC', 5),
    (24, 'FOC', 2),
    (25, 'OLL', 1),
    (25, 'FOC', 1),
    (26, 'ESC', 3);

    DECLARE @creadas TABLE (id_venta INT PRIMARY KEY);

    DECLARE @i INT = 1,
            @max INT = (SELECT MAX(nro) FROM @V),
            @f DATETIME, @u INT, @c INT, @tp INT, @anul BIT,
            @total DECIMAL(12,2), @n INT, @numero VARCHAR(20), @id_venta INT,
            @insertadas INT = 0, @omitidas INT = 0;

    WHILE @i <= @max
    BEGIN
        SELECT @f    = v.fecha,
               @u    = CASE v.vendedor WHEN 'L' THEN @id_lucas ELSE @id_juan END,
               @c    = (SELECT TOP 1 c.id_cliente FROM dbo.cliente c
                        WHERE c.nro_documento = v.dni ORDER BY c.id_cliente),
               @tp   = (SELECT TOP 1 t.id_tipo_pago FROM dbo.tipo_pago t
                        WHERE t.nombre_tipo_pago = v.pago ORDER BY t.id_tipo_pago),
               @anul = v.anulada
        FROM @V v
        WHERE v.nro = @i;

        SET @total = (SELECT SUM(d.cantidad * p.precio)
                      FROM @D d
                      JOIN @prod p ON p.clave = d.clave
                      WHERE d.nro = @i AND p.id_producto IS NOT NULL);

        -- Se omite la venta si falta algun dato o si ya fue cargada antes (re-ejecucion)
        IF @c IS NULL OR @u IS NULL OR @tp IS NULL OR @total IS NULL OR @total <= 0
           OR EXISTS (SELECT 1 FROM dbo.venta_cabecera WHERE id_usuario = @u AND fecha_venta = @f)
        BEGIN
            SET @omitidas += 1;
        END
        ELSE
        BEGIN
            SET @n      = NEXT VALUE FOR dbo.seq_numero_venta;
            SET @numero = CONCAT('V-', RIGHT(CONCAT('000000', @n), 6));

            INSERT INTO dbo.venta_cabecera
                (id_usuario, id_cliente, fecha_venta, total_venta, estado, numero_venta,
                 id_usuario_anulo, fecha_anulacion)
            VALUES
                (@u, @c, @f, @total,
                 CASE WHEN @anul = 1 THEN 'ANULADA' ELSE 'PAGADA' END,
                 @numero,
                 CASE WHEN @anul = 1 THEN @u END,
                 CASE WHEN @anul = 1 THEN DATEADD(HOUR, 2, @f) END);

            SET @id_venta = SCOPE_IDENTITY();

            INSERT INTO dbo.venta_detalle (id_venta, id_producto, cantidad, precio_unitario)
            SELECT @id_venta, p.id_producto, d.cantidad, p.precio
            FROM @D d
            JOIN @prod p ON p.clave = d.clave
            WHERE d.nro = @i AND p.id_producto IS NOT NULL;

            INSERT INTO dbo.pago (id_venta, id_tipo_pago, monto, fecha_pago)
            VALUES (@id_venta, @tp, @total, DATEADD(SECOND, 25, @f));

            INSERT INTO @creadas (id_venta) VALUES (@id_venta);
            SET @insertadas += 1;
        END

        SET @i += 1;
    END

    /* =================================================================
       6) STOCK PARA LAS ALERTAS
          Taladro = 1 (critico), Camara = 0 (critico), Ollas = 2 (bajo).
          Umbral stock_bajo = 3 en los productos que lo tienen en 0.
       ================================================================= */
    UPDATE p SET p.stock = 1
    FROM dbo.producto p JOIN @prod x ON x.id_producto = p.id_producto WHERE x.clave = 'TAL';

    UPDATE p SET p.stock = 0
    FROM dbo.producto p JOIN @prod x ON x.id_producto = p.id_producto WHERE x.clave = 'CAM';

    UPDATE p SET p.stock = 2
    FROM dbo.producto p JOIN @prod x ON x.id_producto = p.id_producto WHERE x.clave = 'OLL';

    UPDATE p SET p.stock_bajo = 3
    FROM dbo.producto p JOIN @prod x ON x.id_producto = p.id_producto WHERE p.stock_bajo = 0;

    COMMIT;

    PRINT CONCAT('Ventas insertadas: ', @insertadas, ' | omitidas (ya existian o faltaban datos): ', @omitidas);

    /* =================================================================
       7) VERIFICACION (solo lectura)
       ================================================================= */
    -- 7.1 Forma de pago de las ventas cargadas por ESTE script (objetivo ~60% / 40%)
    SELECT tp.nombre_tipo_pago AS forma_pago,
           COUNT(*) AS ventas,
           CAST(100.0 * COUNT(*) / SUM(COUNT(*)) OVER () AS DECIMAL(5,1)) AS porcentaje
    FROM @creadas c
    JOIN dbo.pago pg ON pg.id_venta = c.id_venta
    JOIN dbo.tipo_pago tp ON tp.id_tipo_pago = pg.id_tipo_pago
    GROUP BY tp.nombre_tipo_pago;

    -- 7.2 Facturacion por vendedor (todas las ventas no anuladas): Lucas debe quedar primero
    SELECT CONCAT(u.nombre, ' ', u.apellido) AS vendedor,
           COUNT(*) AS ventas,
           SUM(v.total_venta) AS facturado
    FROM dbo.venta_cabecera v
    JOIN dbo.Usuario u ON u.id_usuario = v.id_usuario
    WHERE v.estado <> 'ANULADA' AND v.activo = 1
    GROUP BY CONCAT(u.nombre, ' ', u.apellido)
    ORDER BY facturado DESC;

    -- 7.3 Evolucion mensual
    SELECT YEAR(fecha_venta) AS anio, MONTH(fecha_venta) AS mes,
           SUM(CASE WHEN estado <> 'ANULADA' THEN 1 ELSE 0 END) AS ventas,
           SUM(CASE WHEN estado <> 'ANULADA' THEN total_venta ELSE 0 END) AS facturado,
           SUM(CASE WHEN estado =  'ANULADA' THEN 1 ELSE 0 END) AS anuladas
    FROM dbo.venta_cabecera
    WHERE activo = 1
    GROUP BY YEAR(fecha_venta), MONTH(fecha_venta)
    ORDER BY anio, mes;

    -- 7.4 Alertas de stock
    SELECT id_producto, nombre, stock, stock_bajo
    FROM dbo.producto
    WHERE activo = 1 AND stock <= stock_bajo
    ORDER BY stock, nombre;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT CONCAT('ERROR ', ERROR_NUMBER(), ' (linea ', ERROR_LINE(), '): ', ERROR_MESSAGE());
    THROW;
END CATCH;
GO

/* ---------------------------------------------------------------------
   OPCIONAL (NO se ejecuta solo): productos repetidos
   Tu tabla tiene copias del mismo producto (por haber corrido el script de
   poblado varias veces). En los reportes aparecen como barras separadas.
   Para dejar activa SOLO la primera copia de cada nombre, descomenta y corre:

   UPDATE p SET p.activo = 0
   FROM dbo.producto p
   WHERE p.id_producto > (SELECT MIN(x.id_producto) FROM dbo.producto x WHERE x.nombre COLLATE Latin1_General_CI_AI = p.nombre COLLATE Latin1_General_CI_AI);

   (No borra nada: las ventas viejas siguen apuntando a esos id.)
   --------------------------------------------------------------------- */

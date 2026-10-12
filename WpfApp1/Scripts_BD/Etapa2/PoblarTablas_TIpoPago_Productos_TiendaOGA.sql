-- 1. Primero insertamos algunas categorías acordes a tus productos
INSERT INTO dbo.categoria (nombre, descripcion) VALUES 
('Herramientas', 'Herramientas eléctricas y manuales'),
('Bazar', 'Artículos de cocina y hogar'),
('Limpieza', 'Productos y accesorios de limpieza'),
('Tecnología', 'Iluminación inteligente y seguridad');

-- 2. Insertamos los productos de la imagen relacionándolos con sus categorías
-- (Asumiendo que los IDs de categoría generados arriba son 1, 2, 3 y 4 respectivamente)
INSERT INTO dbo.producto (id_categoria, nombre, precio_costo, precio_ventas, stock) 
VALUES 
-- [1] Taladro Percutor 650W (Stock: 10)
(1, 'Taladro Percutor 650W', 45000, 65000, 10),

-- [2] Juego de Ollas 5 piezas (Stock: 5)
(2, 'Juego de Ollas 5 piezas', 30000, 45000, 5),

-- [3] Escoba de cerdas duras (Stock: 30)
(3, 'Escoba de cerdas duras', 1800, 3000, 30),

-- [4] Foco Inteligente LED Wi-Fi (Stock: 40)
(4, 'Foco Inteligente LED Wi-Fi', 9000, 14000, 40),

-- [5] Cámara de Seguridad Interior (Stock: 12)
(4, 'Cámara de Seguridad Interior', 35000, 52000, 12);


---AGREGAMOS EL TIPO DE PAGO TAMBIEN QUE NO TENGO
-- 1. Insertamos las categorías para los productos
INSERT INTO dbo.categoria (nombre, descripcion) VALUES 
('Herramientas', 'Herramientas eléctricas y manuales'),
('Bazar', 'Artículos de cocina y hogar'),
('Limpieza', 'Productos y accesorios de limpieza'),
('Tecnología', 'Iluminación inteligente y seguridad');

-- 2. Insertamos los productos relacionándolos con las categorías
INSERT INTO dbo.producto (id_categoria, nombre, precio_costo, precio_ventas, stock) VALUES 
(1, 'Taladro Percutor 650W', 45000, 65000, 10),
(2, 'Juego de Ollas 5 piezas', 30000, 45000, 5),
(3, 'Escoba de cerdas duras', 1800, 3000, 30),
(4, 'Foco Inteligente LED Wi-Fi', 9000, 14000, 40),
(4, 'Cámara de Seguridad Interior', 35000, 52000, 12);

-- 3. Insertamos los Tipos de Pago junto con su descripción directamente (Sin necesidad de UPDATE)
INSERT INTO dbo.tipo_pago (nombre_tipo_pago, descripcion) VALUES 
('Efectivo', 'Pago al contado con dinero físico'),
('Tarjeta', 'Pago mediante tarjeta de débito o crédito'),
('Transferencia', 'Transferencia bancaria o billetera virtual');



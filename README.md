# 🛒 TiendaOga

![Cátedra](https://img.shields.io/badge/C%C3%A1tedra-Taller_de_Programaci%C3%B3n_II-555555?style=flat-square)
![Lenguaje](https://img.shields.io/badge/C%23-.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Interfaz](https://img.shields.io/badge/UI-WPF-0EA5E9?style=flat-square)
![Motor](https://img.shields.io/badge/SQL_Server-T--SQL-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)

**TiendaOga** es un sistema de gestión y ventas para una tienda. Es una aplicación de escritorio desarrollada en **C# y .NET (WPF)** que permite administrar productos, registrar ventas y controlar el acceso de los usuarios según su rol. Este repositorio contiene el código fuente del proyecto desarrollado para la cátedra Taller de Programación II.

<p align="center">
  <img alt="Login" src="https://img.shields.io/badge/M%C3%93DULO-LOGIN_Y_ROLES-3B82F6?style=for-the-badge">
  <img alt="Catálogo" src="https://img.shields.io/badge/M%C3%93DULO-CAT%C3%81LOGO-8B5CF6?style=for-the-badge">
  <img alt="Ventas" src="https://img.shields.io/badge/M%C3%93DULO-VENTAS-22C55E?style=for-the-badge">
</p>

| Módulo | Descripción |
| :--- | :--- |
| Login y roles | Autenticación de usuarios y permisos según el rol. |
| Catálogo | Alta, consulta, modificación y baja de productos y stock. |
| Ventas | Registro de transacciones y compras de los clientes. |

---

## ![01](https://img.shields.io/badge/01-555555?style=flat-square)![PRESENTACI%C3%93N](https://img.shields.io/badge/PRESENTACI%C3%93N_Y_CONTEXTO-3B82F6?style=flat-square) Presentación y contexto

**TiendaOga** centraliza las operaciones diarias de una tienda en una sola aplicación: el personal inicia sesión, gestiona el catálogo de productos y registra las ventas, que quedan almacenadas en una base de datos relacional en **SQL Server**.

---

## ![02](https://img.shields.io/badge/02-555555?style=flat-square)![CARACTER%C3%8DSTICAS](https://img.shields.io/badge/CARACTER%C3%8DSTICAS-22C55E?style=flat-square) Características principales

**Gestión de usuarios y roles**

- Sistema de autenticación (login).
- Roles con distintos permisos: **Administrador**, **Gerente** y **Vendedor**.

**Gestión de inventario (catálogo)**

- Interfaz para administrar el catálogo de productos.
- Operaciones CRUD (crear, leer, actualizar y borrar) para mantener el stock actualizado.

**Ventas y facturación**

- Módulo para registrar las transacciones y compras de los clientes.
- Registro detallado de cada operación en la base de datos.

**Persistencia de datos**

- Base de datos relacional en SQL Server.
- Consultas y procedimientos almacenados para el manejo de la información.

---

## ![03](https://img.shields.io/badge/03-555555?style=flat-square)![ROLES](https://img.shields.io/badge/ROLES_Y_PERMISOS-F59E0B?style=flat-square) Roles y permisos

| Rol | Permisos |
| :--- | :--- |
| `Administrador` | _Completar: ej. acceso total, gestión de usuarios y catálogo._ |
| `Gerente` | _Completar: ej. gestión de catálogo y consulta de ventas._ |
| `Vendedor` | _Completar: ej. registro de ventas._ |

---

## ![04](https://img.shields.io/badge/04-555555?style=flat-square)![TECNOLOG%C3%8DAS](https://img.shields.io/badge/TECNOLOG%C3%8DAS-8B5CF6?style=flat-square) Tecnologías utilizadas

| Componente | Tecnología |
| :--- | :--- |
| Lenguaje principal | C# |
| Framework | .NET (WPF) |
| Lenguaje de base de datos | T-SQL |
| Motor de base de datos | SQL Server |
| Gestor de base de datos | SQL Server Management Studio (SSMS) |
| Entorno de desarrollo | Visual Studio |

---

## ![05](https://img.shields.io/badge/05-555555?style=flat-square)![ESTRUCTURA](https://img.shields.io/badge/ESTRUCTURA_DEL_REPOSITORIO-6B7280?style=flat-square) Estructura del repositorio

```text
TiendaOga/
│
├── WpfApp1/          # Proyecto WPF: interfaz y lógica de la aplicación
├── TiendaOga.slnx    # Solución de Visual Studio
├── .gitattributes
├── .gitignore
└── README.md
```

---

## ![06](https://img.shields.io/badge/06-555555?style=flat-square)![EJECUCI%C3%93N](https://img.shields.io/badge/EJECUCI%C3%93N-14B8A6?style=flat-square) Cómo ejecutar el proyecto

> **Importante:** es una aplicación de escritorio (WPF), por lo que **solo se ejecuta en Windows**. Necesita una base de datos SQL Server creada; sin ella el login y los módulos no funcionan.

### Requisitos previos

| Requisito | Detalle |
| :--- | :--- |
| Sistema operativo | Windows 10 u 11. |
| Visual Studio | **2022 versión 17.13 o superior** (o 2026), con la carga de trabajo **Desarrollo de escritorio con .NET**. Las versiones anteriores no abren la solución `.slnx`. |
| SQL Server | Express, Developer o LocalDB. |
| SSMS | SQL Server Management Studio. |
| Git | Para clonar el repositorio (opcional: también se puede descargar el ZIP). |

### Paso a paso

**Paso 1. Descargar el proyecto**

```bash
git clone https://github.com/enzo2304/TiendaOga.git
```

También se puede usar el botón verde **Code → Download ZIP** en GitHub y descomprimir la carpeta.

**Paso 2. Crear la base de datos**

1. Abrir **SSMS** y conectarse al servidor. Nombres habituales: `localhost\SQLEXPRESS` (Express) o `(localdb)\MSSQLLocalDB` (LocalDB).
2. Ir a **Archivo → Abrir → ScriptDB** y seleccionar el script de la base de datos.
3. Presionar **Ejecutar** (`F5`). El script crea la base `[TiendaOgaDB]` con sus tablas, procedimientos almacenados y datos iniciales.
4. Verificar en el Explorador de objetos (clic derecho en **Bases de datos → Actualizar**) que la base aparece.

**Paso 3. Configurar la conexión**
1. Abrir el archivo `App.config` dentro de `WpfApp1`.
2. Reemplazar el servidor por el de tu equipo (el mismo que usaste en SSMS):

```text
Server=TU_SERVIDOR;Database=TiendaOgaDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Si el servidor usa usuario y contraseña de SQL en lugar de la cuenta de Windows, reemplazar `Trusted_Connection=True` por `User Id=USUARIO;Password=CONTRASEÑA`.

**Paso 4. Abrir la solución**

1. Hacer doble clic en `TiendaOga.slnx` (se abre con Visual Studio).
2. Esperar a que se restauren los paquetes NuGet (se ve en la barra inferior). Si hay errores por paquetes faltantes: clic derecho sobre la solución → **Restaurar paquetes NuGet**.

**Paso 5. Elegir el proyecto de inicio**

En el Explorador de soluciones, clic derecho sobre `WpfApp1` → **Establecer como proyecto de inicio** (debe verse en negrita).

**Paso 6. Compilar y ejecutar**

1. Menú **Compilar → Compilar solución** (`Ctrl + Shift + B`) y comprobar que no haya errores.
2. Presionar **F5** (o el botón **Iniciar**). Se abre la ventana de login.

**Paso 7. Iniciar sesión**

Ingresar con un usuario de prueba cargado por el script:

| Rol | Usuario | Contraseña |
| :--- | :--- | :--- |
| Administrador | `[COMPLETAR]` | `[COMPLETAR]` |
| Gerente | `[COMPLETAR]` | `[COMPLETAR]` |
| Vendedor | `[COMPLETAR]` | `[COMPLETAR]` |

### Problemas frecuentes

| Problema | Solución |
| :--- | :--- |
| Visual Studio no abre `TiendaOga.slnx`. | Actualizar a Visual Studio 2022 17.13 o superior. |
| Error de conexión al iniciar sesión. | Revisar el nombre del servidor en la cadena de conexión y que el servicio de SQL Server esté iniciado. |
| `Cannot open database ... requested by the login`. | La base no existe: repetir el Paso 2. |
| Error de certificado SSL. | Agregar `TrustServerCertificate=True` a la cadena de conexión. |
| La aplicación no inicia o abre otro proyecto. | Repetir el Paso 5. |

---

## ![07](https://img.shields.io/badge/07-555555?style=flat-square)![DESARROLLADORES](https://img.shields.io/badge/DESARROLLADORES-EC4899?style=flat-square) Desarrolladores

- [Christian Gabriel Jesus Gomez](https://github.com/Chris10TS)
- [Sanchez Rodriguez Enzo Nahuel](https://github.com/enzo2304)

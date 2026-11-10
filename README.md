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

**Requisitos**

- Visual Studio con la carga de trabajo de escritorio de .NET.
- SQL Server y SQL Server Management Studio (SSMS).

**Pasos**

| Orden | Acción | Nota |
| :---: | :--- | :--- |
| 1 | Clonar el repositorio. | `git clone https://github.com/enzo2304/TiendaOga.git` |
| 2 | Crear la base de datos en SQL Server. | _Completar: indicar el script, si se incluye en el repositorio._ |
| 3 | Configurar la cadena de conexión. | _Completar: indicar en qué archivo se configura._ |
| 4 | Abrir `TiendaOga.slnx` en Visual Studio y ejecutar. | Usar `WpfApp1` como proyecto de inicio. |

---

## ![07](https://img.shields.io/badge/07-555555?style=flat-square)![CAPTURAS](https://img.shields.io/badge/CAPTURAS-EC4899?style=flat-square) Capturas

_Completar: agregar capturas del login, el catálogo y el módulo de ventas, por ejemplo en una carpeta `capturas/`._

---

## ![08](https://img.shields.io/badge/08-555555?style=flat-square)![DESARROLLADORES](https://img.shields.io/badge/DESARROLLADORES-EC4899?style=flat-square) Desarrolladores

- [Christian Gabriel Jesus Gomez](https://github.com/Chris10TS)
- [Sanchez Rodriguez Enzo Nahuel](https://github.com/enzo2304)

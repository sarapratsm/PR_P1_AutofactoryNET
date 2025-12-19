# 🏭 AutoFactory - Sistema de Gestión de Producción

![Status](https://img.shields.io/badge/Status-Completed-success)
![Technology](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/Platform-WPF-blue)
![Database](https://img.shields.io/badge/Database-Oracle-red)

**AutoFactory** es una aplicación de escritorio completa desarrollada en **WPF (Windows Presentation Foundation)** para la gestión integral de una fábrica de automoción. Permite administrar productos, componentes, proveedores y generar informes de fabricación (BOM).

Este proyecto implementa una arquitectura por capas (MVVM + DAO), integración con informes externos y una lógica de base de datos robusta mediante PL/SQL.

---

## 🚀 Funcionalidades Clave

### 📦 Gestión de Productos y BOM (Bill of Materials)
- **Estructura Jerárquica:** Creación de productos compuestos por múltiples items (componentes o sub-productos).
- **CRUD Completo:** Alta, baja, modificación y consulta de productos.
- **Validación de Stock:** Control de cantidades y tipos de unidad (kg, u, m).
- **Lógica Recursiva:** Gestión inteligente de inserción/actualización del árbol de componentes.

### ⚙️ Gestión de Componentes y Proveedores
- **Asignación Múltiple:** Un componente puede tener múltiples proveedores con precios distintos.
- **Cálculo Automático de Costes:** El precio medio (`Preu Mig`) se calcula automáticamente en la base de datos mediante **Triggers y Funciones PL/SQL** cada vez que cambia un proveedor.
- **Imágenes:** Carga y visualización de imágenes de componentes (almacenadas como `BLOB` en Oracle).

### 📄 Informes y Reportes
- **Integración con JasperReports:** Generación de informes PDF ("Hoja de Ruta" / BOM) consumiendo la API REST de Jasper Server.
- **Visor Integrado:** Visualización de los reportes dentro de la app usando **WebView2**, sin necesidad de abrir el navegador.

### 🛡️ Seguridad y Arquitectura
- **Login Seguro:** Hash de contraseñas utilizando **BCrypt**.
- **Configuración Externa:** Gestión de cadenas de conexión e IPs mediante `appsettings.json`.
- **Patrón MVVM:** Separación clara entre la interfaz (View), la lógica (ViewModel) y los datos (Model).
- **Transacciones:** Uso de transacciones ACID para asegurar la integridad de los datos al guardar estructuras complejas.

---

## 🛠️ Stack Tecnológico

* **Frontend:** C# .NET 8.0, WPF (XAML).
* **Base de Datos:** Oracle Database.
* **Acceso a Datos:** ADO.NET con patrón DAO y Unit of Work.
* **Reportes:** JasperReports Server.
* **Librerías:**
    * `BCrypt.Net` (Seguridad).
    * `Oracle.ManagedDataAccess`.
    * `Microsoft.Web.WebView2` (Visor PDF).
    * `Newtonsoft.Json`.
    * `DependencyPropertyGenerator`.

---

## 💾 Base de Datos (PL/SQL)

La lógica de negocio crítica reside en la base de datos para garantizar la integridad.
* **Tablas Principales:** `PRODUCTE`, `COMPONENTE`, `PROVEIDOR`, `PROD_ITEM` (Tabla intermedia del BOM).
* **Triggers:**
    * `T_PROV_COMP_AFTER`: Recalcula el precio medio del componente al asignar proveedores.
    * `PKG_FLAGS`: Paquete para evitar errores de "Tabla Mutante" en Oracle.

---

## 🔧 Instalación y Despliegue

### Requisitos
* Visual Studio 2022.
* .NET 6.0 SDK.
* Oracle Database.
* (Opcional) JasperReports Server para los informes.

### Pasos
1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/tu-usuario/AutoFactory.git](https://github.com/tu-usuario/AutoFactory.git)
    ```
2.  **Base de Datos:**
    * Ejecuta el script `script_bbdd_autofactory.sql` (ubicado en la carpeta raíz) en tu instancia de Oracle.
    * Esto creará las tablas, triggers, funciones y datos de prueba.
3.  **Configuración:**
    * Abre el archivo `appsettings.json` en la raíz del proyecto.
    * Ajusta la `ConnectionStrings` y la `JasperConfig` con tus IPs y credenciales.
4.  **Compilar y Ejecutar:**
    * Abre la solución en Visual Studio y ejecuta en modo `Release` o `Debug`.

---

## ✒️ Autor

**Sara Prats Morales**
* [LinkedIn]([https://linkedin.com/in/tu-perfil](https://www.linkedin.com/in/sara-prats-morales/))
* [GitHub](https://github.com/kuro-spm)

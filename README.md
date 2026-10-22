# 🚗 DriveControl - Sistema de Gestión de Alquiler de Vehículos

> **Sistema Web de Uso Interno Exclusivo** para la administración integral de los alquileres de una agencia de vehículos. Permite gestionar clientes, flota, categorías, reservas, cobros y auditoría interna.

---

## 🔑 Credenciales de Acceso (Usuarios Seed)

Al ejecutar las migraciones e iniciar la aplicación por primera vez, el *seeder* (`Data/DatosIniciales.cs`) crea de forma automática e idempotente los siguientes usuarios de prueba:

|        Rol        |    Correo Electrónico       |   Contraseña   | Permisos Principales |
| :---------------- | :-------------------------- | :------------- | :------------------- |
| **Administrador** | `admin@drivecontrol.com`    |   `Admin123!`  | Acceso total: Usuarios, Vehículos, Categorías y Auditoría interna.|
|    **Empleado**   | `empleado@drivecontrol.com` | `Empleado123!` | Operaciones del día a día: Gestión de clientes, alquileres y pagos.|

*Nota: Los clientes no tienen acceso al sistema; toda la gestión es operada por el personal de la agencia.*

---

## 👥 Integrantes del Grupo (Grupo 11)

* **Evelyn Cetera** — [evelyncetera@gmail.com](mailto:evelyncetera@gmail.com)
* **Matias Correa** — [matigc90@gmail.com](mailto:matigc90@gmail.com)
* **Christian Villegas** — [villegaschristian16@gmail.com](mailto:villegaschristian16@gmail.com)
* **Mauricio Barca** — [mauriciobarca1989@gmail.com](mailto:mauriciobarca1989@gmail.com)

🔗 **Repositorio oficial:** [https://github.com/mgc90/alquiler-vehiculos-labo2-g11](https://github.com/mgc90/alquiler-vehiculos-labo2-g11)

---

## 🚀 Puesta en Marcha (Entorno Local)

### Requisitos Previos
* **.NET 10 SDK**
* **MySQL / MariaDB** ejecutándose en `localhost:3306` (por defecto: usuario `root` sin contraseña).

### Pasos para Ejecutar

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/mgc90/alquiler-vehiculos-labo2-g11.git
   cd alquiler-vehiculos-labo2-g11
   ```

2. **Configuración de la Base de Datos:**
   La cadena de conexión apunta a la base `alquiler_vehiculos` en `appsettings.json`. Si necesitas modificar credenciales locales, puedes crear o editar el archivo `appsettings.Development.json`.

3. **Crear la Base de Datos y Tablas:**
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar la Aplicación:**
   ```bash
   dotnet run --launch-profile http
   ```
   Accede desde tu navegador a: [http://localhost:5119](http://localhost:5119)

---

## 📌 Descripción de la Problemática y Solución

### Problemática
Las agencias que operan manualmente o mediante planillas descentralizadas sufren de:
* **Solapamiento de reservas:** Alquiler duplicado de unidades o asignación de vehículos en taller/mantenimiento.
* **Falta de inmutabilidad en cobros:** Dificultad para auditar dinero efectivamente ingresado contra deudas pendientes (multas, recargos o daños).
* **Deficiencia en auditoría:** Imposibilidad de saber qué empleado creó, finalizó o anuló un registro.
* **Descontrol de licencias:** Falta de validación sobre el estado y vencimiento de la licencia de conducir del cliente.

### Propuesta de Solución
Aplicación web desarrollada en **ASP.NET Core MVC** con:
* Control de disponibilidad en tiempo real.
* Autenticación basada en Roles mediante Cookies.
* Registro inmutable de pagos y trazabilidad de saldos.
* Auditoría integrada para Administradores.
* Documentación fotográfica (portadas de flota y registro de daños).

---

## 🛠️ Reglas de Negocio y Flujo Operativo

### 1. Usuarios y Roles
* **Empleado:** Crea y gestiona clientes, consulta disponibilidad, realiza reservas, procesa entregas/devoluciones y cobra pagos. Solo edita su propio perfil.
* **Administrador:** Además de las funciones del empleado, administra usuarios, categorías, catálogo de vehículos y consulta la auditoría del sistema.
* **Clientes:** Son registrados por los empleados (requiere DNI, datos de contacto y fecha de vencimiento de licencia vigente durante el periodo de alquiler).

### 2. Ciclo de Vida del Alquiler
1. **Consulta de Disponibilidad:** Verificación en tiempo real de vehículos habilitados sin solapamiento de fechas.
2. **Reserva (`Reservado`):** El precio diario de la categoría se congela al momento de reservar.
3. **Entrega (`EnCurso`):** Se registra la fecha y hora real de entrega del vehículo.
4. **Devolución (`Finalizado`):** Se registra la entrega real. Si hay retrasos o daños, los recargos se suman al saldo del alquiler. La unidad queda libre inmediatamente.
5. **Cancelación (`Cancelado`):** Se libera el vehículo. La seña abonada se pierde como penalización y se calcula la multa correspondiente.

### 3. Pagos e Inmutabilidad
* La tabla `Pago` representa estrictamente dinero ingresado.
* **Inmutabilidad:** Un pago registrado no se modifica. Si hubo un error, se marca como `Anulado` y se registra un nuevo pago.
* **Tipos de Pago:** Seña, Parcial, Saldo, Recargo y Multa.

---

## 🗄️ Modelo de Datos (Entidades)

* **Usuario:** `Id`, `Email`, `PasswordHash`, `Nombre`, `Apellido`, `Avatar`, `Rol`, `Activo`
* **Cliente:** `Id`, `Dni`, `Nombre`, `Apellido`, `FechaNacimiento`, `Telefono`, `Email`, `NumeroLicencia`, `VencimientoLicencia`, `Activo`
* **CategoriaVehiculo:** `Id`, `Nombre`, `Descripcion`, `PrecioDiario`, `DepositoGarantia`, `Activa`
* **Vehiculo:** `Id`, `CategoriaVehiculoId`, `Patente`, `Marca`, `Modelo`, `KilometrajeActual`, `EstadoOperativo` *(Habilitado / Mantenimiento / Baja)*
* **Alquiler:** `Id`, `ClienteId`, `VehiculoId`, `CreadoPorUsuarioId`, `CerradoPorUsuarioId`, `EntregaPrevista`, `DevolucionPrevista`, `EntregaReal`, `DevolucionReal`, `PrecioAcordado`, `Observaciones`, `Multa`, `ImporteTotal`, `Estado` *(Reservado / EnCurso / Finalizado / Cancelado)*
* **Pago:** `Id`, `AlquilerId`, `CreadoPorUsuarioId`, `AnuladoPorUsuarioId`, `Fecha`, `FechaAnulacion`, `Importe`, `Tipo`, `MedioPago`, `Estado` *(Vigente / Anulado)*
* **Imagen:** `Id`, `idVehiculo`, `titulo`, `url`, `descripcion`, `idAlquiler` *(Opcional para registro de daños)*

---

## 📊 Informes y Requerimientos Técnicos

### Informes Prioritarios
1. Listado de vehículos filtrado por estado operativo.
2. Listado de alquileres vigentes según rango de fechas.
3. Historial de pagos por alquiler con opción a registrar nuevos cobros.
4. Consulta de disponibilidad de vehículos por rango de fechas y categoría.

### Requerimientos Técnicos Adicionales
* **Servidor:** Paginado, filtrados y búsquedas dinámicas en desplegables resueltos 100% en el servidor.
* **AJAX / Vue.js:** Implementación de al menos un módulo/ABM completo (ej. Vehículos) utilizando Vue.js mediante peticiones AJAX.
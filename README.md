Sistema de Gestión de Alquiler de Vehículos
(Uso Interno Exclusivo)
El proyecto consiste en un sistema web de uso estrictamente interno para la administración integral de los alquileres de una agencia de vehículos. El sistema será utilizado por empleados y administradores para gestionar clientes, vehículos, categorías, alquileres, pagos y la información asociada a cada operación.
Los clientes no acceden directamente al sistema, sino que toda la gestión se realiza desde la agencia.

Datos del Grupo
Evelyn Cetera - evelyncetera@gmail.com 
Matias Correa - matigc90@gmail.com 
Christian Villegas - villegaschristian16@gmail.com
Mauricio Barca - [ingresar Mail]
URL Repositorio : [Ingresar Link]

Puesta en Marcha (Desarrollo Local)
Requisitos: .NET 10 SDK y MySQL/MariaDB en localhost (por defecto: usuario root sin contraseña, puerto 3306).
La cadena de conexión vive en appsettings.json (base de datos: alquiler_vehiculos). Cada desarrollador puede sobrescribirla en appsettings.Development.json.
Para crear las tablas por primera vez: dotnet ef database update
Para ejecutar la aplicación: dotnet run --launch-profile http (http://localhost:5119)

Credenciales de acceso (usuarios seed)
Al iniciar la aplicación, el seeder (Data/DatosIniciales.cs) crea los roles y un usuario por rol. Solo se crean si no existen (idempotente):
Administrador: admin@drivecontrol.com / Admin123!
Empleado: empleado@drivecontrol.com / Empleado123!

Descripción de la Problemática: 
Las agencias de alquiler de vehículos que operan de forma manual o mediante planillas descentralizadas enfrentan dificultades críticas de gestión:
Solapamiento de reservas: Riesgo de alquilar el mismo vehículo a dos clientes en rangos de fechas superpuestos o asignar unidades en mantenimiento.
Falta de inmutabilidad en cobros y deudas: Dificultad para diferenciar entre el dinero efectivamente cobrado (señas, saldos) y los cargos pendientes (multas por cancelación, recargos por mora o costos de daños).
Deficiencia en la auditoría interna: Imposibilidad de identificar qué empleado creó una reserva, quién procesó una devolución o quién anuló un pago en el sistema.
Descontrol de vencimientos: Falta de validación sobre el estado de la licencia de conducir del cliente durante el período de alquiler.
Propuesta de Solución: 
Se desarrollará una aplicación web ASP.NET Core MVC de uso interno que centralizará la administración del negocio mediante las siguientes funcionalidades:
Control de disponibilidad en tiempo real: Verificación automática de vehículos habilitados y libres de reservas en el rango de fechas solicitado.
Autenticación y Seguridad basada en Roles: Control de acceso mediante cookies para los roles Empleado (operaciones de alquiler y pagos) y Administrador (gestión de usuarios, vehículos, categorías y auditoría).
Gestión inmutable de pagos y saldos: La entidad de pagos registrará únicamente dinero ingresado de forma irreversible, manejando las anulaciones mediante cambios de estado y registrando deudas pendientes dentro del propio alquiler.
Auditoría integrada: Registro automático del usuario interno que crea o cierra un alquiler y crea o anula un pago, visible únicamente para administradores.
Documentación fotográfica: Módulo de carga de imágenes para portadas de vehículos y registro opcional de daños vinculados a un alquiler.

1. Acceso, Usuarios y Roles
Acceso: Se realiza mediante correo electrónico y contraseña.
Roles:
Empleado: Administra clientes, consulta disponibilidad, crea y gestiona alquileres y procesa pagos. Solo puede modificar su propio perfil (datos personales, contraseña y avatar).
Administrador: Además de las funciones de empleado, gestiona usuarios, categorías de vehículos, vehículos y tiene acceso exclusivo a las vistas de auditoría.
Baja de Usuarios: Se aplican bajas lógicas (desactivación).
Clientes: No inician sesión ni tienen cuenta en el sistema. Son registrados por un empleado ingresando DNI, nombre completo, fecha de nacimiento, datos de contacto, número de licencia y fecha de vencimiento de la misma.
Requisitos para alquilar: Ser mayor de edad, estar activo y contar con la licencia vigente durante todo el período previsto del alquiler.

2. Flujo Operativo del Alquiler
Consulta de Disponibilidad: El empleado selecciona la categoría deseada y las fechas de retiro y devolución previstas. El sistema verifica qué vehículos están habilitados y no poseen alquileres Reservados o EnCurso que se superpongan en ese rango de fechas.
Creación de la Reserva ( Reservado ): Al seleccionar un vehículo, el precio diario se copia desde su CategoriaVehiculo y queda fijo para ese alquiler, sin verse afectado por futuros cambios de tarifa.
Entrega de la Unidad ( EnCurso ): Se registra la fecha y hora real de entrega, el alquiler cambia su estado a EnCurso.
Devolución de la Unidad ( Finalizado ):
Se registra la fecha, hora real de devolución y observaciones de daños/costos.
La fecha de devolución prevista nunca se sobrescribe; la fecha real se almacena aparte para conservar la información original y permitir recálculos.
Si la devolución se realiza con retraso, los recargos por demora y los costos por daños se incorporan al saldo total del alquiler.
Finalización con saldo pendiente: El alquiler se marca como Finalizado y el vehículo vuelve a estar disponible de inmediato, evitando que la unidad quede bloqueada o en estado EnCurso indefinidamente. La deuda generada permanece registrada como saldo pendiente del alquiler.
Cancelación del Alquiler ( Cancelado ):
Si el cliente cancela antes del retiro, la seña abonada no es reembolsable (se pierde como penalización).
Se calcula la multa por cancelación y se registra en el campo correspondiente del alquiler.
El alquiler pasa a estado Cancelado y el vehículo se libera inmediatamente para nuevas reservas. La deuda no cubierta queda registrada como saldo pendiente.
Extensión de Alquiler: No se modifica el alquiler original. Se crea un alquiler nuevo con el mismo cliente y vehículo para el nuevo período (verificando disponibilidad previa).

3. Registro de Pagos y Auditoría
Regla de Pagos: La entidad Pago representa únicamente dinero efectivamente ingresado/recibido. Los cargos, multas o deudas no se registran dentro de la tabla Pago hasta que sean abonados por el cliente.
Inmutabilidad y Anulación: Un pago realizado no se edita en su importe, fecha, tipo ni medio de pago. En caso de error, el pago se marca con estado Anulado y se registra un nuevo pago.
Tipos de Pago: Seña, Parcial, Saldo, Recargo y Multa.
Auditoría:
En la entidad Alquiler se registra el usuario creador y el usuario que lo cerró (finalizó o canceló).
En la entidad Pago se registra el usuario que creó el pago y el usuario que lo anuló.
Esta información de auditoría sólo es visible para el rol Administrador.

4. Entidades Definitivas del Sistema
Usuario: Id, Email, PasswordHash, Nombre, Apellido, Avatar, Rol (Admin / Empleado), Activo.
Cliente: Id, Dni, Nombre, Apellido, FechaNacimiento, Telefono, Email, NumeroLicencia, VencimientoLicencia, Activo.
CategoriaVehiculo: Id, Nombre, Descripcion, PrecioDiario, DepositoGarantia, Activa.
Vehiculo: Id, CategoriaVehiculoId, Patente, Marca, Modelo, KilometrajeActual, EstadoOperativo (Habilitado / Mantenimiento / Baja).
Alquiler: Id, ClienteId, VehiculoId, CreadoPorUsuarioId, CerradoPorUsuarioId, EntregaPrevista, DevolucionPrevista, EntregaReal, DevolucionReal, PrecioAcordado, Observaciones, Multa, ImporteTotal, Estado (Reservado / EnCurso / Finalizado / Cancelado).
Pago: Id, AlquilerId, CreadoPorUsuarioId, AnuladoPorUsuarioId, Fecha, FechaAnulacion, Importe, Tipo (Seña / Parcial / Total / Recargo / Multa), MedioPago, Estado (Vigente / Anulado).
Imagen: Id, idVehiculo, titulo, url, descripción, idAlquiler(nullable por daños)

5. Descripción de Relaciones
CategoriaVehiculo a Vehiculo (1 a 0..*): Una categoría clasifica a varios vehículos, pero cada vehículo pertenece strictly a una categoría.
Vehiculo a Alquiler (1 a 0..*): Un vehículo puede participar en múltiples alquileres a lo largo del tiempo, pero cada alquiler corresponde a una sola unidad.
Cliente a Alquiler (1 a 0..*):  Un cliente puede realizar varios alquileres, pero cada contrato tiene un único cliente responsable.
Alquiler a Pago (1 a 0..*): Un alquiler puede recibir múltiples pagos (señas, saldos, multas). Cada pago está vinculado a un único alquiler.
Usuario a Alquiler (1 a 0..*)  Relación de Auditoría: Un usuario (empleado o admin) registra la creación del alquiler (CreadoPorUsuarioId), y un usuario registra el cierre o cancelación (CerradoPorUsuarioId).
Usuario a Pago (1 a 0..*) Relación de Auditoría: Un usuario registra la recepción de un pago (CreadoPorUsuarioId), y un usuario registra su eventual anulación (AnuladoPorUsuarioId).
Vehiculo e Imagen / Alquiler e Imagen (1 a 0..* y 0..1 a 0..*): Una imagen pertenece obligatoriamente a un vehículo (foto de catálogo o portada). Opcionalmente (idAlquiler anulable), la imagen puede asociarse a un alquiler específico para documentar el estado o daños en la entrega o devolución.

6. Informes Prioritarios
Listar todos los vehículos con su categoría, permitiendo filtrar por estado operativo (Habilitado, Mantenimiento, Baja).
Listar todos los alquileres vigentes según rango de fechas (retiro y devolución).
Listar los pagos realizados para un alquiler específico, con opción de cargar un nuevo pago desde esa vista.
Dadas dos fechas (retiro y devolución) y opcionalmente una categoría, listar todos los vehículos no ocupados en ese período.

No prioritarios: 
Listar todos los vehículos de una categoría específica. 
Listar los vehículos más alquilados en los últimos 365 días. 
Listar los vehículos sin alquileres en los últimos X días (30, 60, etc.). 
Listar todos los alquileres que terminen en X días (permitir elegir o especificar plazo). 
Listar los clientes cuya licencia vence en los próximos X días. 
Otros requerimientos 
Los listados deben contar con paginado por servidor. 
Las búsquedas deben ser resueltas en el servidor. 
La selección de valores tipo desplegable (cliente, vehículo, categoría) debe hacerse con búsqueda en el servidor, para evitar traer todos los valores disponibles. 
Al menos un ABM (por ejemplo, vehículos) debe estar desarrollado con Vue.js y funcionar completamente vía AJAX.

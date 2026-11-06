CREATE DATABASE IF NOT EXISTS `alquiler_vehiculos`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `alquiler_vehiculos`;

SET NAMES utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetRoles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`),
    UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUsers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Apellido` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Avatar` longtext CHARACTER SET utf8mb4 NULL,
    `Activo` tinyint(1) NOT NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`),
    UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
    KEY `EmailIndex` (`NormalizedEmail`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` int NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId`
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    KEY `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` int NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    KEY `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
    `UserId` int NOT NULL,
    `RoleId` int NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId`
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
    `UserId` int NOT NULL,
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `CategoriasVehiculo` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Descripcion` varchar(250) CHARACTER SET utf8mb4 NULL,
    `PrecioDiario` decimal(18,2) NOT NULL,
    `DepositoGarantia` decimal(18,2) NOT NULL,
    `Activa` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriasVehiculo` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Clientes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Dni` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Nombre` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Apellido` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FechaNacimiento` datetime(6) NOT NULL,
    `Telefono` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `NumeroLicencia` longtext CHARACTER SET utf8mb4 NOT NULL,
    `VencimientoLicencia` datetime(6) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Vehiculos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CategoriaVehiculoId` int NOT NULL,
    `Patente` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Marca` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Modelo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `KilometrajeActual` int NOT NULL,
    `EstadoOperativo` int NOT NULL,
    CONSTRAINT `PK_Vehiculos` PRIMARY KEY (`Id`),
    KEY `IX_Vehiculos_CategoriaVehiculoId` (`CategoriaVehiculoId`),
    CONSTRAINT `FK_Vehiculos_CategoriasVehiculo_CategoriaVehiculoId`
        FOREIGN KEY (`CategoriaVehiculoId`) REFERENCES `CategoriasVehiculo` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Alquileres` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClienteId` int NOT NULL,
    `VehiculoId` int NOT NULL,
    `CreadoPorUsuarioId` int NOT NULL,
    `CerradoPorUsuarioId` int NULL,
    `EntregaPrevista` datetime(6) NOT NULL,
    `DevolucionPrevista` datetime(6) NOT NULL,
    `EntregaReal` datetime(6) NULL,
    `DevolucionReal` datetime(6) NULL,
    `PrecioAcordado` decimal(18,2) NOT NULL,
    `Observaciones` longtext CHARACTER SET utf8mb4 NULL,
    `Multa` decimal(18,2) NOT NULL,
    `ImporteTotal` decimal(18,2) NOT NULL,
    `Estado` int NOT NULL,
    CONSTRAINT `PK_Alquileres` PRIMARY KEY (`Id`),
    KEY `IX_Alquileres_CerradoPorUsuarioId` (`CerradoPorUsuarioId`),
    KEY `IX_Alquileres_ClienteId` (`ClienteId`),
    KEY `IX_Alquileres_CreadoPorUsuarioId` (`CreadoPorUsuarioId`),
    KEY `IX_Alquileres_VehiculoId` (`VehiculoId`),
    CONSTRAINT `FK_Alquileres_AspNetUsers_CerradoPorUsuarioId`
        FOREIGN KEY (`CerradoPorUsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Alquileres_AspNetUsers_CreadoPorUsuarioId`
        FOREIGN KEY (`CreadoPorUsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Alquileres_Clientes_ClienteId`
        FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Alquileres_Vehiculos_VehiculoId`
        FOREIGN KEY (`VehiculoId`) REFERENCES `Vehiculos` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Imagenes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VehiculoId` int NOT NULL,
    `Titulo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Url` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Descripcion` longtext CHARACTER SET utf8mb4 NULL,
    `AlquilerId` int NULL,
    CONSTRAINT `PK_Imagenes` PRIMARY KEY (`Id`),
    KEY `IX_Imagenes_AlquilerId` (`AlquilerId`),
    KEY `IX_Imagenes_VehiculoId` (`VehiculoId`),
    CONSTRAINT `FK_Imagenes_Alquileres_AlquilerId`
        FOREIGN KEY (`AlquilerId`) REFERENCES `Alquileres` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Imagenes_Vehiculos_VehiculoId`
        FOREIGN KEY (`VehiculoId`) REFERENCES `Vehiculos` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Pagos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AlquilerId` int NOT NULL,
    `CreadoPorUsuarioId` int NOT NULL,
    `AnuladoPorUsuarioId` int NULL,
    `Fecha` datetime(6) NOT NULL,
    `FechaAnulacion` datetime(6) NULL,
    `Importe` decimal(18,2) NOT NULL,
    `Tipo` int NOT NULL,
    `MedioPago` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Estado` int NOT NULL,
    CONSTRAINT `PK_Pagos` PRIMARY KEY (`Id`),
    KEY `IX_Pagos_AlquilerId` (`AlquilerId`),
    KEY `IX_Pagos_AnuladoPorUsuarioId` (`AnuladoPorUsuarioId`),
    KEY `IX_Pagos_CreadoPorUsuarioId` (`CreadoPorUsuarioId`),
    CONSTRAINT `FK_Pagos_Alquileres_AlquilerId`
        FOREIGN KEY (`AlquilerId`) REFERENCES `Alquileres` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Pagos_AspNetUsers_AnuladoPorUsuarioId`
        FOREIGN KEY (`AnuladoPorUsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Pagos_AspNetUsers_CreadoPorUsuarioId`
        FOREIGN KEY (`CreadoPorUsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
    ('20261019140348_InitialCreate', '9.0.21'),
    ('20261023214658_MigracionEtapa1', '9.0.21'),
    ('20261028001905_AgregarAlquileresPagosEImagenes', '9.0.21');

INSERT INTO `CategoriasVehiculo`
    (`Nombre`, `Descripcion`, `PrecioDiario`, `DepositoGarantia`, `Activa`)
SELECT semilla.`Nombre`, semilla.`Descripcion`, semilla.`PrecioDiario`, semilla.`DepositoGarantia`, 1
FROM (
    SELECT 'Compacto' AS `Nombre`, 'Categoría compacto para pruebas del sistema.' AS `Descripcion`,
           15000.00 AS `PrecioDiario`, 50000.00 AS `DepositoGarantia`
    UNION ALL SELECT 'SUV', 'Categoría suv para pruebas del sistema.', 35000.00, 80000.00
    UNION ALL SELECT 'Sedán', 'Categoría sedán para pruebas del sistema.', 22000.00, 60000.00
    UNION ALL SELECT 'Pickup', 'Categoría pickup para pruebas del sistema.', 40000.00, 90000.00
) AS semilla
WHERE NOT EXISTS (
    SELECT 1 FROM `CategoriasVehiculo` existente WHERE existente.`Nombre` = semilla.`Nombre`
);

INSERT INTO `Clientes`
    (`Dni`, `Nombre`, `Apellido`, `FechaNacimiento`, `Telefono`, `Email`,
     `NumeroLicencia`, `VencimientoLicencia`, `Activo`)
SELECT semilla.`Dni`, semilla.`Nombre`, semilla.`Apellido`, semilla.`FechaNacimiento`,
       semilla.`Telefono`, semilla.`Email`, CONCAT('LIC-', semilla.`Dni`),
       DATE_ADD(CURDATE(), INTERVAL semilla.`AnosLicencia` YEAR), 1
FROM (
    SELECT '28999111' AS `Dni`, 'Carlos' AS `Nombre`, 'Gómez' AS `Apellido`,
           '1982-01-01' AS `FechaNacimiento`, '2664112233' AS `Telefono`,
           'carlos.gomez@email.com' AS `Email`, 2 AS `AnosLicencia`
    UNION ALL SELECT '35444555', 'Ana', 'Martínez', '1990-02-04', '2664556677', 'ana.martinez@email.com', 1
    UNION ALL SELECT '31222333', 'Juan', 'Pérez', '1985-03-07', '2665223344', 'juan.perez@email.com', 3
    UNION ALL SELECT '30999877', 'María', 'González', '1992-04-10', '2665334455', 'maria.gonzalez@email.com', 2
    UNION ALL SELECT '28555444', 'Lucía', 'Fernández', '1988-05-13', '2665445566', 'lucia.fernandez@email.com', 4
    UNION ALL SELECT '33555666', 'Diego', 'Rodríguez', '1995-06-16', '2665556677', 'diego.rodriguez@email.com', 1
    UNION ALL SELECT '29888777', 'Sofía', 'López', '1991-07-19', '2665667788', 'sofia.lopez@email.com', 2
    UNION ALL SELECT '31999123', 'Mateo', 'Díaz', '1998-08-22', '2665778899', 'mateo.diaz@email.com', 5
    UNION ALL SELECT '30444455', 'Valentina', 'Torres', '1993-09-25', '2665889900', 'valentina.torres@email.com', 2
    UNION ALL SELECT '27777555', 'Joaquín', 'Méndez', '1986-10-01', '2665990011', 'joaquin.mendez@email.com', 3
    UNION ALL SELECT '32666888', 'Martina', 'Suárez', '1994-11-04', '2665001122', 'martina.suarez@email.com', 1
    UNION ALL SELECT '30000111', 'Thiago', 'Aguirre', '2000-12-07', '2666112233', 'thiago.aguirre@email.com', 4
) AS semilla
WHERE NOT EXISTS (SELECT 1 FROM `Clientes`);

INSERT INTO `Vehiculos`
    (`CategoriaVehiculoId`, `Patente`, `Marca`, `Modelo`, `KilometrajeActual`, `EstadoOperativo`)
SELECT categoria.`Id`, semilla.`Patente`, semilla.`Marca`, semilla.`Modelo`,
       semilla.`KilometrajeActual`, semilla.`EstadoOperativo`
FROM (
    SELECT 'Compacto' AS `Categoria`, 'AF123JK' AS `Patente`, 'Toyota' AS `Marca`,
           'Etios' AS `Modelo`, 25000 AS `KilometrajeActual`, 0 AS `EstadoOperativo`
    UNION ALL SELECT 'Compacto', 'AB345CD', 'Chevrolet', 'Onix', 18000, 0
    UNION ALL SELECT 'Compacto', 'AC001EF', 'Renault', 'Sandero', 31000, 1
    UNION ALL SELECT 'SUV', 'AG987LM', 'Ford', 'Territory', 12000, 0
    UNION ALL SELECT 'SUV', 'AD222GH', 'Volkswagen', 'Taos', 20000, 0
    UNION ALL SELECT 'SUV', 'AE777IJ', 'Jeep', 'Compass', 28000, 0
    UNION ALL SELECT 'Sedán', 'AF555KL', 'Honda', 'City', 15000, 0
    UNION ALL SELECT 'Sedán', 'AG666MN', 'Nissan', 'Versa', 22000, 0
    UNION ALL SELECT 'Sedán', 'AH888OP', 'Toyota', 'Corolla', 9000, 0
    UNION ALL SELECT 'Pickup', 'AI111QR', 'Toyota', 'Hilux', 35000, 0
    UNION ALL SELECT 'Pickup', 'AJ222ST', 'Ford', 'Ranger', 40000, 0
    UNION ALL SELECT 'Pickup', 'AK333UV', 'Chevrolet', 'S10', 27000, 2
) AS semilla
INNER JOIN `CategoriasVehiculo` categoria ON categoria.`Nombre` = semilla.`Categoria`
WHERE NOT EXISTS (SELECT 1 FROM `Vehiculos`);

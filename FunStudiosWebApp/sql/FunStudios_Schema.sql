USE master;
GO

-- =============================================
-- FunStudios - Script de creación de base de datos
-- Generado desde los modelos EF Core y OnModelCreating
-- Todos los PKs usan IDENTITY(1,1)
-- Los catálogos se insertan con SET IDENTITY_INSERT ON
-- =============================================

IF DB_ID('FunStudios') IS NOT NULL
BEGIN
    ALTER DATABASE FunStudios SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FunStudios;
END
GO

CREATE DATABASE FunStudios;
GO

USE FunStudios;
GO

-- =============================================
-- 1. ROLES
-- =============================================
CREATE TABLE Roles (
    RolId       INT           NOT NULL IDENTITY(1,1),
    NombreRol   NVARCHAR(50)  NOT NULL,

    CONSTRAINT PK_Roles PRIMARY KEY (RolId)
);
GO

-- =============================================
-- 2. USUARIOS
-- =============================================
CREATE TABLE Usuarios (
    UsuarioId      INT            NOT NULL IDENTITY(1,1),
    RolId          INT            NOT NULL,
    Nombres        NVARCHAR(100)  NOT NULL,
    Apellidos      NVARCHAR(100)  NOT NULL,
    NombreUsuario  NVARCHAR(50)   NOT NULL,
    PasswordHash   NVARCHAR(500)  NOT NULL,
    Activo         BIT            NOT NULL DEFAULT 1,

    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioId),
    CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId)
        REFERENCES Roles(RolId) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX IX_Usuarios_RolId
    ON Usuarios(RolId);
GO

-- =============================================
-- 3. FAMILIAS
-- =============================================
CREATE TABLE Familias (
    FamiliaId        INT            NOT NULL IDENTITY(1,1),
    ApellidosFamilia NVARCHAR(150)  NOT NULL,
    NombreTutor1     NVARCHAR(100)  NULL,
    TelefonoTutor1   NVARCHAR(20)   NOT NULL,
    NombreTutor2     NVARCHAR(100)  NULL,
    TelefonoTutor2   NVARCHAR(20)   NULL,
    HuellaTutorTemplate VARBINARY(MAX) NULL,

    CONSTRAINT PK_Familias PRIMARY KEY (FamiliaId)
);
GO

-- =============================================
-- 4. CLIENTES
-- =============================================
CREATE TABLE Clientes (
    ClienteId          INT            NOT NULL IDENTITY(1,1),
    FamiliaId          INT            NOT NULL,
    Nombre             NVARCHAR(100)  NOT NULL,
    FechaNacimiento    DATE           NOT NULL,
    NotasMedicas       NVARCHAR(500)  NULL,
    HuellaNinoTemplate VARBINARY(MAX) NULL,

    CONSTRAINT PK_Clientes PRIMARY KEY (ClienteId),
    CONSTRAINT FK_Clientes_Familias FOREIGN KEY (FamiliaId)
        REFERENCES Familias(FamiliaId) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX IX_Clientes_FamiliaId
    ON Clientes(FamiliaId);
GO

-- =============================================
-- 5. TIPO_ACCESO
-- =============================================
CREATE TABLE TiposAcceso (
    TipoAccesoId  INT           NOT NULL IDENTITY(1,1),
    NombreAcceso  NVARCHAR(50)  NOT NULL,

    CONSTRAINT PK_TiposAcceso PRIMARY KEY (TipoAccesoId)
);
GO

-- =============================================
-- 6. METODOS_PAGO
-- =============================================
CREATE TABLE MetodosPago (
    MetodoPagoId  INT           NOT NULL IDENTITY(1,1),
    NombreMetodo  NVARCHAR(50)  NOT NULL,

    CONSTRAINT PK_MetodosPago PRIMARY KEY (MetodoPagoId)
);
GO

-- =============================================
-- 7. MEMBRESIAS
-- =============================================
CREATE TABLE Membresias (
    MembresiaId      INT            NOT NULL IDENTITY(1,1),
    ClienteId        INT            NOT NULL,
    NombreMembresia  NVARCHAR(100)  NOT NULL,
    Descripcion      NVARCHAR(300)  NULL,
    FechaInicio      DATE           NOT NULL,
    FechaFin         DATE           NOT NULL,
    Precio           DECIMAL(18,2)  NOT NULL,
    Estado           NVARCHAR(20)   NOT NULL DEFAULT 'Activa',
    FechaCreacion    DATETIME2      NOT NULL,

    CONSTRAINT PK_Membresias PRIMARY KEY (MembresiaId),
    CONSTRAINT FK_Membresias_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes(ClienteId) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX IX_Membresias_ClienteId
    ON Membresias(ClienteId);
GO

-- =============================================
-- 8. VENTAS
-- =============================================
CREATE TABLE Ventas (
    VentaId       INT            NOT NULL IDENTITY(1,1),
    ClienteId     INT            NOT NULL,
    MetodoPagoId  INT            NOT NULL,
    Monto         DECIMAL(18,2)  NOT NULL,
    Descripcion   NVARCHAR(300)  NOT NULL,
    FechaVenta    DATETIME2      NOT NULL,

    CONSTRAINT PK_Ventas PRIMARY KEY (VentaId),
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes(ClienteId) ON DELETE NO ACTION,
    CONSTRAINT FK_Ventas_MetodosPago FOREIGN KEY (MetodoPagoId)
        REFERENCES MetodosPago(MetodoPagoId) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX IX_Ventas_ClienteId
    ON Ventas(ClienteId);
GO

CREATE NONCLUSTERED INDEX IX_Ventas_MetodoPagoId
    ON Ventas(MetodoPagoId);
GO

-- =============================================
-- 9. FIDELIDAD_CUENTAS (1 a 1 con Cliente)
-- =============================================
CREATE TABLE FidelidadCuentas (
    FidelidadId              INT       NOT NULL IDENTITY(1,1),
    ClienteId                INT       NOT NULL,
    ConteoChecks             TINYINT   NOT NULL DEFAULT 0,
    EntradasGratisGanadas    INT       NOT NULL DEFAULT 0,
    FechaUltimaActualizacion DATETIME2 NULL,

    CONSTRAINT PK_FidelidadCuentas PRIMARY KEY (FidelidadId),
    CONSTRAINT UQ_FidelidadCuentas_ClienteId UNIQUE (ClienteId),
    CONSTRAINT FK_FidelidadCuentas_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes(ClienteId) ON DELETE CASCADE
);
GO

-- =============================================
-- 10. ENTRADAS_HISTORIAL
-- =============================================
CREATE TABLE EntradasHistorial (
    EntradaId         INT        NOT NULL IDENTITY(1,1),
    FamiliaId         INT        NOT NULL,
    ClienteId         INT        NOT NULL,
    FechaHoraEntrada  DATETIME2  NOT NULL,
    FechaHoraSalida   DATETIME2  NULL,
    TipoAccesoId      INT        NOT NULL,
    MembresiaId       INT        NULL,
    VentaId           INT        NULL,
    TutorNombre       NVARCHAR(150) NULL,
    TutorTelefono     NVARCHAR(20)  NULL,

    CONSTRAINT PK_EntradasHistorial PRIMARY KEY (EntradaId),
    CONSTRAINT FK_EntradasHistorial_Familias FOREIGN KEY (FamiliaId)
        REFERENCES Familias(FamiliaId) ON DELETE NO ACTION,
    CONSTRAINT FK_EntradasHistorial_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes(ClienteId) ON DELETE NO ACTION,
    CONSTRAINT FK_EntradasHistorial_TiposAcceso FOREIGN KEY (TipoAccesoId)
        REFERENCES TiposAcceso(TipoAccesoId) ON DELETE NO ACTION,
    CONSTRAINT FK_EntradasHistorial_Membresias FOREIGN KEY (MembresiaId)
        REFERENCES Membresias(MembresiaId) ON DELETE NO ACTION,
    CONSTRAINT FK_EntradasHistorial_Ventas FOREIGN KEY (VentaId)
        REFERENCES Ventas(VentaId) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX IX_EntradasHistorial_ClienteId
    ON EntradasHistorial(ClienteId);
GO

CREATE NONCLUSTERED INDEX IX_EntradasHistorial_FamiliaId
    ON EntradasHistorial(FamiliaId);
GO

CREATE NONCLUSTERED INDEX IX_EntradasHistorial_TipoAccesoId
    ON EntradasHistorial(TipoAccesoId);
GO

CREATE NONCLUSTERED INDEX IX_EntradasHistorial_VentaId
    ON EntradasHistorial(VentaId);
GO

CREATE NONCLUSTERED INDEX IX_EntradasHistorial_MembresiaId
    ON EntradasHistorial(MembresiaId);
GO

-- =============================================
-- 11. LOGS_BIOMETRICO
-- =============================================
CREATE TABLE LogsBiometrico (
    LogId              INT            NOT NULL IDENTITY(1,1),
    FechaHoraEvento    DATETIME2      NOT NULL,
    Gravedad           NVARCHAR(20)   NULL,
    Operacion          NVARCHAR(50)   NULL,
    MensajeDetalle     NVARCHAR(500)  NOT NULL,
    IPEstacionTrabajo  NVARCHAR(45)   NOT NULL,
    UsuarioId          INT            NULL,
    ClienteId          INT            NULL,

    CONSTRAINT PK_LogsBiometrico PRIMARY KEY (LogId),
    CONSTRAINT FK_LogsBiometrico_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE SET NULL,
    CONSTRAINT FK_LogsBiometrico_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes(ClienteId) ON DELETE SET NULL
);
GO

CREATE NONCLUSTERED INDEX IX_LogsBiometrico_UsuarioId
    ON LogsBiometrico(UsuarioId);
GO

CREATE NONCLUSTERED INDEX IX_LogsBiometrico_ClienteId
    ON LogsBiometrico(ClienteId);
GO

-- =============================================
-- INSERTAR CATÁLOGOS CON IDs FIJOS
-- (el código usa constantes: Rol.Administrador=1, TiposAcceso.DayPass=1, etc.)
-- =============================================

SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (RolId, NombreRol) VALUES
(1, N'Administrador'),
(2, N'Recepción');
SET IDENTITY_INSERT Roles OFF;
GO

SET IDENTITY_INSERT TiposAcceso ON;
INSERT INTO TiposAcceso (TipoAccesoId, NombreAcceso) VALUES
(1, N'Day Pass'),
(2, N'Fidelidad Gratis'),
(3, N'Membresía');
SET IDENTITY_INSERT TiposAcceso OFF;
GO

SET IDENTITY_INSERT MetodosPago ON;
INSERT INTO MetodosPago (MetodoPagoId, NombreMetodo) VALUES
(1, N'Efectivo'),
(2, N'Tarjeta'),
(3, N'Transferencia');
SET IDENTITY_INSERT MetodosPago OFF;
GO

-- =============================================
-- USUARIOS DEL SISTEMA
-- Contraseñas hasheadas con PBKDF2 (SHA256, 100000 iteraciones)
-- Formato: {iteraciones}.{saltBase64}.{hashBase64}
-- Generadas con PasswordHasher.Hash() de la aplicación
-- =============================================
SET IDENTITY_INSERT Usuarios ON;
INSERT INTO Usuarios (UsuarioId, RolId, Nombres, Apellidos, NombreUsuario, PasswordHash, Activo) VALUES
(1, 1, N'Ana',    N'Martínez',  N'admin',     N'100000.MkVbcaMcAmvA+8mYHABcOA==.GPjnrhd1g8Hid8fwDozxD2Ja7ANIIm/JlTm8Y5yMpj0=', 1),
(2, 2, N'Carla',  N'Reyes',     N'recepcion', N'100000.2Hwno3N2Xn28HzoUC7Y0zw==.istILaLS4gWyxXUFz6y/5Hwhqdh5x4CUTaq9La3rOkw=', 1),
(3, 2, N'Luis',   N'Ortega',    N'lortega',   N'100000.2Hwno3N2Xn28HzoUC7Y0zw==.istILaLS4gWyxXUFz6y/5Hwhqdh5x4CUTaq9La3rOkw=', 0);
SET IDENTITY_INSERT Usuarios OFF;
GO

PRINT '==============================================';
PRINT '  Base de datos FunStudios creada exitosamente';
PRINT '  11 tablas + 3 catálogos + 3 usuarios';
PRINT '==============================================';
GO

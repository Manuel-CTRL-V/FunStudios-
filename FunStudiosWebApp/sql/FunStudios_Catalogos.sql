-- =====================================================
-- FunStudios - Datos de catálogos del sistema
-- Ejecutar DESPUÉS de FunStudios_Esquema.sql, en la base destino.
-- Idempotente: se puede ejecutar más de una vez sin duplicar filas.
-- (El usuario administrador lo crea la aplicación en el primer arranque.)
--
-- Seleccione la base destino antes de ejecutar, por ejemplo:
--   USE [FunStudios];
-- =====================================================

-- ---------- Roles ----------
SET IDENTITY_INSERT dbo.Roles ON;
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RolId = 1)
    INSERT INTO dbo.Roles (RolId, NombreRol) VALUES (1, N'Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RolId = 2)
    INSERT INTO dbo.Roles (RolId, NombreRol) VALUES (2, N'Recepción');
SET IDENTITY_INSERT dbo.Roles OFF;
GO

-- ---------- Tipos de acceso ----------
SET IDENTITY_INSERT dbo.TiposAcceso ON;
IF NOT EXISTS (SELECT 1 FROM dbo.TiposAcceso WHERE TipoAccesoId = 1)
    INSERT INTO dbo.TiposAcceso (TipoAccesoId, NombreAcceso, Precio, Descripcion, Activo)
    VALUES (1, N'Day Pass', 400.00, N'Entrada de un día (pago de $400 RD$).', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.TiposAcceso WHERE TipoAccesoId = 2)
    INSERT INTO dbo.TiposAcceso (TipoAccesoId, NombreAcceso, Precio, Descripcion, Activo)
    VALUES (2, N'Fidelidad Gratis', 0.00, N'Entrada gratis acumulada por fidelidad (sin pago).', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.TiposAcceso WHERE TipoAccesoId = 3)
    INSERT INTO dbo.TiposAcceso (TipoAccesoId, NombreAcceso, Precio, Descripcion, Activo)
    VALUES (3, N'Membresía', 0.00, N'Entrada incluida en una membresía vigente (sin pago).', 1);
SET IDENTITY_INSERT dbo.TiposAcceso OFF;
GO

-- ---------- Métodos de pago ----------
SET IDENTITY_INSERT dbo.MetodosPago ON;
IF NOT EXISTS (SELECT 1 FROM dbo.MetodosPago WHERE MetodoPagoId = 1)
    INSERT INTO dbo.MetodosPago (MetodoPagoId, NombreMetodo) VALUES (1, N'Efectivo');
IF NOT EXISTS (SELECT 1 FROM dbo.MetodosPago WHERE MetodoPagoId = 2)
    INSERT INTO dbo.MetodosPago (MetodoPagoId, NombreMetodo) VALUES (2, N'Tarjeta');
IF NOT EXISTS (SELECT 1 FROM dbo.MetodosPago WHERE MetodoPagoId = 3)
    INSERT INTO dbo.MetodosPago (MetodoPagoId, NombreMetodo) VALUES (3, N'Transferencia');
SET IDENTITY_INSERT dbo.MetodosPago OFF;
GO

-- ---------- Parámetros del sistema ----------
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Clave = N'Fidelidad.ChecksParaPremio')
    INSERT INTO dbo.Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible)
    VALUES (N'Fidelidad.ChecksParaPremio', N'8',
            N'Cantidad de visitas (Day Pass) necesarias para ganar una entrada gratis.',
            N'numero', N'Checks para premio de fidelidad');
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Clave = N'Fidelidad.ChecksPorDia')
    INSERT INTO dbo.Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible)
    VALUES (N'Fidelidad.ChecksPorDia', N'1',
            N'Cantidad máxima de checks que un niño puede sumar por entradas Day Pass dentro de un mismo día.',
            N'numero', N'Checks máximos por día');
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Clave = N'Moneda.Simbolo')
    INSERT INTO dbo.Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible)
    VALUES (N'Moneda.Simbolo', N'RD$',
            N'Símbolo de la moneda que se muestra en toda la aplicación.',
            N'texto', N'Símbolo de moneda');
IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Clave = N'Parque.Nombre')
    INSERT INTO dbo.Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible)
    VALUES (N'Parque.Nombre', N'FunStudios',
            N'Nombre del parque que se muestra en reportes y encabezados.',
            N'texto', N'Nombre del parque');
GO

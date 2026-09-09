/* =====================================================================
   FunStudios - Migración v2: Configuración y escalabilidad
   Fecha: 2026-09-04
   Base: FunStudios

   Cambios:
     1. Nacionalidad del niño (Clientes.Nacionalidad)
     2. Código único de familia (Familias.CodigoFamilia) -> formato "FAM-001"
     3. Tabla de parámetros del sistema (clave-valor) para parametrizar
        checks de fidelidad, moneda, nombre del parque, etc.
     4. TiposAcceso: precio, descripción y activo (parametrización de
        tipos de acceso disponibles y sus precios)

   -------- CÓMO EJECUTAR --------
   En SQL Server Management Studio (SSMS):
     - Conectarse con la base FunStudios (o con cualquier BD de ese servidor)
     - Ejecutar el script completo (F5)
   El script es IDEMPOTENTE: se puede ejecutar varias veces sin error.
   ===================================================================== */

USE FunStudios;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* =====================================================================
   1) NACIONALIDAD EN CLIENTES (niño)
      - NULL permitido (es un campo opcional con default en la UI)
   ===================================================================== */
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Nacionalidad'
)
BEGIN
    ALTER TABLE Clientes ADD Nacionalidad NVARCHAR(50) NULL;
END
GO

/* =====================================================================
   2) CÓDIGO ÚNICO DE FAMILIA (FAM-001, FAM-002, ...)
      - Se agrega columna, se puebla para las familias existentes
        (en orden por FamiliaId) y se le pone índice único.
      - Para las familias nuevas el código lo genera la aplicación
        (siguiente número secuencial). El índice único evita duplicados.
   ===================================================================== */
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Familias' AND COLUMN_NAME = 'CodigoFamilia'
)
BEGIN
    ALTER TABLE Familias ADD CodigoFamilia NVARCHAR(20) NULL;
END
GO

-- Poblar códigos para las familias que aún no tienen uno (solo para datos existentes)
UPDATE f
SET f.CodigoFamilia = 'FAM-' + RIGHT(REPLICATE('0', 10) + CAST(n.Rn AS VARCHAR(10)), 3)
FROM Familias f
INNER JOIN (
    SELECT FamiliaId, ROW_NUMBER() OVER (ORDER BY FamiliaId) AS Rn
    FROM Familias
    WHERE CodigoFamilia IS NULL
) n ON f.FamiliaId = n.FamiliaId;
GO

-- Índice único sobre el código de familia
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_Familias_CodigoFamilia' AND object_id = OBJECT_ID('Familias')
)
BEGIN
    CREATE UNIQUE INDEX UQ_Familias_CodigoFamilia ON Familias (CodigoFamilia);
END
GO

/* =====================================================================
   3) TABLA PARAMETROS (clave-valor) - configuración general del sistema
      Permite parametrizar sin cambiar código: checks de fidelidad,
      moneda, nombre del parque, mensajes, etc.
   ===================================================================== */
IF OBJECT_ID('Parametros', 'U') IS NULL
BEGIN
    CREATE TABLE Parametros (
        ParametroId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Parametros PRIMARY KEY,
        Clave NVARCHAR(100) NOT NULL,
        Valor NVARCHAR(500) NULL,
        Descripcion NVARCHAR(300) NULL,
        Tipo NVARCHAR(20) NOT NULL CONSTRAINT DF_Parametros_Tipo DEFAULT N'texto',
        NombreVisible NVARCHAR(150) NULL
    );

    CREATE UNIQUE INDEX UQ_Parametros_Clave ON Parametros (Clave);
END
GO

-- Siembra de parámetros iniciales (no duplica si ya existen)
IF NOT EXISTS (SELECT 1 FROM Parametros WHERE Clave = N'Fidelidad.ChecksParaPremio')
    INSERT INTO Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible) VALUES
    (N'Fidelidad.ChecksParaPremio', N'8',
     N'Cantidad de visitas (Day Pass) necesarias para ganar una entrada gratis. Actualmente 8.',
     N'numero', N'Checks para premio de fidelidad');
GO

IF NOT EXISTS (SELECT 1 FROM Parametros WHERE Clave = N'Moneda.Simbolo')
    INSERT INTO Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible) VALUES
    (N'Moneda.Simbolo', N'RD$',
     N'Símbolo de la moneda que se muestra en toda la aplicación.',
     N'texto', N'Símbolo de moneda');
GO

IF NOT EXISTS (SELECT 1 FROM Parametros WHERE Clave = N'Parque.Nombre')
    INSERT INTO Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible) VALUES
    (N'Parque.Nombre', N'FunStudios',
     N'Nombre del parque que se muestra en reportes y encabezados.',
     N'texto', N'Nombre del parque');
GO

IF NOT EXISTS (SELECT 1 FROM Parametros WHERE Clave = N'Fidelidad.ChecksPorDia')
    INSERT INTO Parametros (Clave, Valor, Descripcion, Tipo, NombreVisible) VALUES
    (N'Fidelidad.ChecksPorDia', N'1',
     N'Cantidad máxima de checks que un niño puede sumar por entradas Day Pass dentro de un mismo día. Si entra y sale varias veces el mismo día, suma solo el límite aquí configurado.',
     N'numero', N'Checks máximos por día');
GO

/* =====================================================================
   4) TIPOS DE ACCESO: precio, descripción y activo
      Con esto el panel de Configuración puede editar el precio del
      Day Pass y desactivar/activar tipos sin tocar código.
   ===================================================================== */
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TiposAcceso' AND COLUMN_NAME = 'Precio'
)
BEGIN
    ALTER TABLE TiposAcceso ADD Precio DECIMAL(10,2) NOT NULL CONSTRAINT DF_TiposAcceso_Precio DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TiposAcceso' AND COLUMN_NAME = 'Descripcion'
)
BEGIN
    ALTER TABLE TiposAcceso ADD Descripcion NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TiposAcceso' AND COLUMN_NAME = 'Activo'
)
BEGIN
    ALTER TABLE TiposAcceso ADD Activo BIT NOT NULL CONSTRAINT DF_TiposAcceso_Activo DEFAULT 1;
END
GO

-- Valores iniciales coherentes con el funcionamiento actual
UPDATE TiposAcceso SET Precio = 400.00, Descripcion = N'Entrada de un día (pago de $400 RD$).', Activo = 1 WHERE TipoAccesoId = 1;
UPDATE TiposAcceso SET Precio = 0.00,   Descripcion = N'Entrada gratis acumulada por fidelidad (sin pago).', Activo = 1 WHERE TipoAccesoId = 2;
UPDATE TiposAcceso SET Precio = 0.00,   Descripcion = N'Entrada incluida en una membresía vigente (sin pago).', Activo = 1 WHERE TipoAccesoId = 3;
GO

/* =====================================================================
   Verificación final: consulta de lo aplicado
   ===================================================================== */
SELECT 'Clientes.Nacionalidad' AS Cambio,
       CAST(COUNT(*) AS VARCHAR) AS ColumnasEncontradas
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Nacionalidad'
UNION ALL
SELECT 'Familias.CodigoFamilia',
       CAST(COUNT(*) AS VARCHAR)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Familias' AND COLUMN_NAME = 'CodigoFamilia'
UNION ALL
SELECT 'Parametros (tabla)',
       CAST(COUNT(*) AS VARCHAR)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Parametros'
UNION ALL
SELECT 'TiposAcceso.Precio',
       CAST(COUNT(*) AS VARCHAR)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TiposAcceso' AND COLUMN_NAME = 'Precio';

SELECT FamiliaId, ApellidosFamilia, CodigoFamilia FROM Familias ORDER BY FamiliaId;
GO
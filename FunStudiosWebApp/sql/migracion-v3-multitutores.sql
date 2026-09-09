/*
 * Migración v3: múltiples tutores por familia + acompañantes en el checkpoint.
 *
 * 1) Crea la tabla Tutores (N tutores por familia, uno EsPrincipal).
 * 2) Migra los datos existentes de Familias.NombreTutor1/TelefonoTutor1
 *    (queda como principal) y NombreTutor2/TelefonoTutor2.
 * 3) Agrega EntradasHistorial.Acompanantes (cantidad de acompañantes en la entrada).
 *
 * Es IDEMPOTENTE: se puede ejecutar más de una vez sin error.
 * Aplicar: SQL Server Management Studio sobre la base de la aplicación.
 */
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Tutores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tutores (
        TutorId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tutores PRIMARY KEY,
        FamiliaId   INT NOT NULL,
        Nombre      NVARCHAR(100) NOT NULL,
        Telefono    NVARCHAR(20) NOT NULL,
        EsPrincipal BIT NOT NULL CONSTRAINT DF_Tutores_EsPrincipal DEFAULT 0,
        CONSTRAINT FK_Tutores_Familias FOREIGN KEY (FamiliaId)
            REFERENCES dbo.Familias(FamiliaId) ON DELETE CASCADE
    );

    CREATE INDEX IX_Tutores_FamiliaId ON dbo.Tutores(FamiliaId);
END
GO

-- Migra los tutores registrados en la tabla Familias (solo la primera vez)
IF NOT EXISTS (SELECT 1 FROM dbo.Tutores)
BEGIN
    INSERT INTO dbo.Tutores (FamiliaId, Nombre, Telefono, EsPrincipal)
    SELECT f.FamiliaId, f.NombreTutor1, ISNULL(NULLIF(LTRIM(RTRIM(f.TelefonoTutor1)), ''), ''),
           1
    FROM dbo.Familias f
    WHERE f.NombreTutor1 IS NOT NULL AND LTRIM(RTRIM(f.NombreTutor1)) <> '';

    INSERT INTO dbo.Tutores (FamiliaId, Nombre, Telefono, EsPrincipal)
    SELECT f.FamiliaId, f.NombreTutor2, ISNULL(NULLIF(LTRIM(RTRIM(f.TelefonoTutor2)), ''), ''),
           0
    FROM dbo.Familias f
    WHERE f.NombreTutor2 IS NOT NULL AND LTRIM(RTRIM(f.NombreTutor2)) <> '';
END
GO

-- Acompañantes: cantidad de acompañantes registrada en cada entrada (por defecto 1 = el tutor)
IF COL_LENGTH('dbo.EntradasHistorial', 'Acompanantes') IS NULL
BEGIN
    ALTER TABLE dbo.EntradasHistorial
        ADD Acompanantes INT NOT NULL
            CONSTRAINT DF_EntradasHistorial_Acompanantes DEFAULT 1;
END
GO

/*
 * OPCIONAL: una vez verificada la migración en producción, se pueden eliminar
 * las columnas de tutores fijos de la tabla Familias (la app ya no las usa).
 *
 * ALTER TABLE dbo.Familias DROP CONSTRAINT DF_Tutores_EsPrincipal;  -- no existe; sólo referencia
 * ALTER TABLE dbo.Familias DROP COLUMN NombreTutor1, TelefonoTutor1, NombreTutor2, TelefonoTutor2;
 */
GO
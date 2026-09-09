/*
 * Migración v4: la huella biométrica del tutor (RF4) pasa de la familia
 * al tutor específico que la registró (Tutor.HuellaTemplate).
 *
 * 1) Agrega Tutores.HuellaTemplate (VARBINARY(MAX)).
 * 2) Si aún existe Familias.HuellaTutorTemplate, mueve su contenido
 *    al tutor principal de cada familia.
 * 3) Elimina Familias.HuellaTutorTemplate.
 *
 * NOTA: se ejecuta en batches separados (GO) para que el ALTER (DDL) se
 * compile antes de referenciar la nueva columna. Es idempotente.
 */
SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.Tutores', 'HuellaTemplate') IS NULL
    ALTER TABLE dbo.Tutores ADD HuellaTemplate VARBINARY(MAX) NULL;
GO

-- Mueve la huella de la familia al tutor principal (si la columna antigua aún existe)
-- Se usa SQL dinámico para que la referencia no rompa la compilación del batch
-- cuando la columna ya no exista (vuelve idempotente).
IF COL_LENGTH('dbo.Familias', 'HuellaTutorTemplate') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Tutores', 'HuellaTemplate') IS NOT NULL
    BEGIN
        EXEC(N'UPDATE t
               SET t.HuellaTemplate = f.HuellaTutorTemplate
               FROM dbo.Tutores t
               JOIN dbo.Familias f ON f.FamiliaId = t.FamiliaId
               WHERE t.EsPrincipal = 1 AND f.HuellaTutorTemplate IS NOT NULL;');
    END;
END;
GO

IF COL_LENGTH('dbo.Familias', 'HuellaTutorTemplate') IS NOT NULL
    ALTER TABLE dbo.Familias DROP COLUMN HuellaTutorTemplate;
GO
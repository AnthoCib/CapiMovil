/*
    Script opcional de normalización de códigos de negocio.
    Fecha: 2026-04-23
    Objetivo: normalizar códigos legacy en PadreFamilia, Conductor y Estudiante
              al formato oficial PREFIX-<FRAGMENTO><NNNN>.

    Recomendación:
      1) Ejecutar primero en QA.
      2) Revisar SELECT previos y posteriores.
      3) Respaldar datos antes de aplicar UPDATE.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @InicioPadre INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoPadre, 4) AS INT)), 0)
        FROM dbo.PadreFamilia
        WHERE CodigoPadre LIKE 'PAD-%[0-9][0-9][0-9][0-9]'
    );

    DECLARE @InicioConductor INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoConductor, 4) AS INT)), 0)
        FROM dbo.Conductor
        WHERE CodigoConductor LIKE 'CON-%[0-9][0-9][0-9][0-9]'
    );

    DECLARE @InicioEstudiante INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoEstudiante, 4) AS INT)), 0)
        FROM dbo.Estudiante
        WHERE CodigoEstudiante LIKE 'EST-%[0-9][0-9][0-9][0-9]'
    );
    /* ========================
       PREVIEW (sin cambios)
       ======================== */
    SELECT 'PADRE_LEGACY' AS Tipo, p.IdPadre AS IdEntidad, p.CodigoPadre AS CodigoActual
    FROM dbo.PadreFamilia p
    WHERE p.CodigoPadre IS NULL
       OR p.CodigoPadre NOT LIKE 'PAD-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]';

    SELECT 'CONDUCTOR_LEGACY' AS Tipo, c.IdConductor AS IdEntidad, c.CodigoConductor AS CodigoActual
    FROM dbo.Conductor c
    WHERE c.CodigoConductor IS NULL
       OR c.CodigoConductor NOT LIKE 'CON-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]';

    SELECT 'ESTUDIANTE_LEGACY' AS Tipo, e.IdEstudiante AS IdEntidad, e.CodigoEstudiante AS CodigoActual
    FROM dbo.Estudiante e
    WHERE e.CodigoEstudiante IS NULL
       OR e.CodigoEstudiante NOT LIKE 'EST-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]';

    /* ========================
       PADRE
       ======================== */
    ;WITH Base AS
    (
        SELECT
            p.IdPadre,
            ROW_NUMBER() OVER (ORDER BY p.FechaCreacion, p.IdPadre) AS Nro,
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(p.Nombres, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(p.ApellidoPaterno, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(p.ApellidoMaterno, 'X'))), 2)) AS Fragmento
        FROM dbo.PadreFamilia p
        WHERE p.CodigoPadre IS NULL
           OR p.CodigoPadre NOT LIKE 'PAD-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE p
       SET p.CodigoPadre = CONCAT('PAD-', b.Fragmento, RIGHT(CONCAT('0000', @InicioPadre + b.Nro), 4))
    FROM dbo.PadreFamilia p
    INNER JOIN Base b ON b.IdPadre = p.IdPadre;

    /* ========================
       CONDUCTOR
       ======================== */
    ;WITH Base AS
    (
        SELECT
            c.IdConductor,
            ROW_NUMBER() OVER (ORDER BY c.FechaCreacion, c.IdConductor) AS Nro,
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(c.Nombres, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(c.ApellidoPaterno, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(c.ApellidoMaterno, 'X'))), 2)) AS Fragmento
        FROM dbo.Conductor c
        WHERE c.CodigoConductor IS NULL
           OR c.CodigoConductor NOT LIKE 'CON-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE c
       SET c.CodigoConductor = CONCAT('CON-', b.Fragmento, RIGHT(CONCAT('0000', @InicioConductor + b.Nro), 4))
    FROM dbo.Conductor c
    INNER JOIN Base b ON b.IdConductor = c.IdConductor;

    /* ========================
       ESTUDIANTE
       ======================== */
    ;WITH Base AS
    (
        SELECT
            e.IdEstudiante,
            ROW_NUMBER() OVER (ORDER BY e.FechaCreacion, e.IdEstudiante) AS Nro,
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(e.Nombres, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(e.ApellidoPaterno, 'X'))), 2)) +
            UPPER(LEFT(LTRIM(RTRIM(ISNULL(e.ApellidoMaterno, 'X'))), 2)) AS Fragmento
        FROM dbo.Estudiante e
        WHERE e.CodigoEstudiante IS NULL
           OR e.CodigoEstudiante NOT LIKE 'EST-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE e
       SET e.CodigoEstudiante = CONCAT('EST-', b.Fragmento, RIGHT(CONCAT('0000', @InicioEstudiante + b.Nro), 4))
    FROM dbo.Estudiante e
    INNER JOIN Base b ON b.IdEstudiante = e.IdEstudiante;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

/*
   Normalización opcional de códigos legacy del módulo transporte.
   Entidades: Bus, Ruta, Paradero, RutaEstudiante.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @InicioBus INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoBus, 4) AS INT)), 0)
        FROM dbo.Bus
        WHERE CodigoBus LIKE 'BUS-%[0-9][0-9][0-9][0-9]'
    );

    DECLARE @InicioRuta INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoRuta, 4) AS INT)), 0)
        FROM dbo.Ruta
        WHERE CodigoRuta LIKE 'RUT-%[0-9][0-9][0-9][0-9]'
    );

    DECLARE @InicioParadero INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoParadero, 4) AS INT)), 0)
        FROM dbo.Paradero
        WHERE CodigoParadero LIKE 'PAR-%[0-9][0-9][0-9][0-9]'
    );

    DECLARE @InicioRutaEstudiante INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoRutaEstudiante, 4) AS INT)), 0)
        FROM dbo.RutaEstudiante
        WHERE CodigoRutaEstudiante LIKE 'RAS-%[0-9][0-9][0-9][0-9]'
    );

    ;WITH BaseBus AS
    (
        SELECT
            b.IdBus,
            ROW_NUMBER() OVER (ORDER BY b.FechaCreacion, b.IdBus) AS Nro,
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(b.Placa, 'XXX'), '-', ''), ' ', ''), 3)) +
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(b.Marca, 'XXX'), '-', ''), ' ', ''), 3)) AS Fragmento
        FROM dbo.Bus b
        WHERE b.CodigoBus IS NULL
           OR b.CodigoBus NOT LIKE 'BUS-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE b
       SET b.CodigoBus = CONCAT('BUS-', bb.Fragmento, RIGHT(CONCAT('0000', @InicioBus + bb.Nro), 4))
    FROM dbo.Bus b
    INNER JOIN BaseBus bb ON bb.IdBus = b.IdBus;

    ;WITH BaseRuta AS
    (
        SELECT
            r.IdRuta,
            ROW_NUMBER() OVER (ORDER BY r.FechaCreacion, r.IdRuta) AS Nro,
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(r.Nombre, 'RUTAXX'), ' ', ''), '-', ''), 6)) AS Fragmento
        FROM dbo.Ruta r
        WHERE r.CodigoRuta IS NULL
           OR r.CodigoRuta NOT LIKE 'RUT-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE r
       SET r.CodigoRuta = CONCAT('RUT-', RIGHT(CONCAT(br.Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @InicioRuta + br.Nro), 4))
    FROM dbo.Ruta r
    INNER JOIN BaseRuta br ON br.IdRuta = r.IdRuta;

    ;WITH BaseParadero AS
    (
        SELECT
            p.IdParadero,
            ROW_NUMBER() OVER (ORDER BY p.FechaCreacion, p.IdParadero) AS Nro,
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(r.Nombre, 'RU'), ' ', ''), '-', ''), 2)) +
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(p.Nombre, 'PARA'), ' ', ''), '-', ''), 4)) AS Fragmento
        FROM dbo.Paradero p
        INNER JOIN dbo.Ruta r ON r.IdRuta = p.IdRuta
        WHERE p.CodigoParadero IS NULL
           OR p.CodigoParadero NOT LIKE 'PAR-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE p
       SET p.CodigoParadero = CONCAT('PAR-', bp.Fragmento, RIGHT(CONCAT('0000', @InicioParadero + bp.Nro), 4))
    FROM dbo.Paradero p
    INNER JOIN BaseParadero bp ON bp.IdParadero = p.IdParadero;

    ;WITH BaseRutaEstudiante AS
    (
        SELECT
            re.IdRutaEstudiante,
            ROW_NUMBER() OVER (ORDER BY re.FechaCreacion, re.IdRutaEstudiante) AS Nro,
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(r.Nombre, 'RU'), ' ', ''), '-', ''), 2)) +
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(e.Nombres, 'NO'), ' ', ''), '-', ''), 2)) +
            UPPER(LEFT(REPLACE(REPLACE(ISNULL(e.ApellidoPaterno, 'AP'), ' ', ''), '-', ''), 2)) AS Fragmento
        FROM dbo.RutaEstudiante re
        INNER JOIN dbo.Ruta r ON r.IdRuta = re.IdRuta
        INNER JOIN dbo.Estudiante e ON e.IdEstudiante = re.IdEstudiante
        WHERE re.CodigoRutaEstudiante IS NULL
           OR re.CodigoRutaEstudiante NOT LIKE 'RAS-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE re
       SET re.CodigoRutaEstudiante = CONCAT('RAS-', bre.Fragmento, RIGHT(CONCAT('0000', @InicioRutaEstudiante + bre.Nro), 4))
    FROM dbo.RutaEstudiante re
    INNER JOIN BaseRutaEstudiante bre ON bre.IdRutaEstudiante = re.IdRutaEstudiante;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

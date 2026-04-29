/* ============================================================
   CapiMovil - Orden automático de paraderos por ruta
   Fecha: 2026-04-29
   ============================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* 1) Diagnóstico de duplicados actuales por ruta + orden */
SELECT
    p.IdRuta,
    r.Nombre AS NombreRuta,
    p.OrdenParada,
    COUNT(1) AS TotalDuplicados
FROM dbo.Paradero p
LEFT JOIN dbo.Ruta r ON r.IdRuta = p.IdRuta
WHERE p.FechaEliminacion IS NULL
GROUP BY p.IdRuta, r.Nombre, p.OrdenParada
HAVING COUNT(1) > 1
ORDER BY r.Nombre, p.OrdenParada;
GO

/* 2) Normalización opcional: recalcular orden por ruta */
;WITH ParaderosOrdenados AS
(
    SELECT
        p.IdParadero,
        p.IdRuta,
        NuevoOrden = ROW_NUMBER() OVER
        (
            PARTITION BY p.IdRuta
            ORDER BY p.FechaCreacion, p.IdParadero
        )
    FROM dbo.Paradero p
    WHERE p.FechaEliminacion IS NULL
)
UPDATE p
SET p.OrdenParada = po.NuevoOrden
FROM dbo.Paradero p
INNER JOIN ParaderosOrdenados po ON po.IdParadero = p.IdParadero;
GO

/* 3) UNIQUE para proteger duplicados de orden por ruta */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Paradero')
      AND name = 'UX_Paradero_IdRuta_OrdenParada'
)
BEGIN
    CREATE UNIQUE INDEX UX_Paradero_IdRuta_OrdenParada
        ON dbo.Paradero (IdRuta, OrdenParada)
        WHERE FechaEliminacion IS NULL;
END
GO

/* 4) SP registrar: calcular orden automático por ruta */
CREATE OR ALTER PROCEDURE dbo.sp_Paradero_Registrar
    @IdRuta UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(120),
    @Direccion NVARCHAR(250),
    @Latitud DECIMAL(10, 7) = NULL,
    @Longitud DECIMAL(10, 7) = NULL,
    @HoraEstimada TIME = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @CodigoGenerado NVARCHAR(20);
    DECLARE @Correlativo INT;
    DECLARE @OrdenParadaGenerado INT;

    BEGIN TRY
        BEGIN TRAN;

        SELECT @OrdenParadaGenerado = ISNULL(MAX(p.OrdenParada), 0) + 1
        FROM dbo.Paradero p WITH (UPDLOCK, HOLDLOCK)
        WHERE p.IdRuta = @IdRuta
          AND p.FechaEliminacion IS NULL;

        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'PAR')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('PAR', 0);

            UPDATE dbo.CorrelativoDocumento
            SET UltimoNumero = UltimoNumero + 1
            WHERE TipoCodigo = 'PAR';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'PAR';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoParadero, 4) AS INT)), 0) + 1
            FROM dbo.Paradero
            WHERE CodigoParadero LIKE 'PAR-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('PAR-', RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Paradero
        (
            IdParadero, IdRuta, CodigoParadero, Nombre, Direccion, Latitud, Longitud,
            OrdenParada, HoraEstimada, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRuta, @CodigoGenerado, @Nombre, @Direccion, @Latitud, @Longitud,
            @OrdenParadaGenerado, @HoraEstimada, @Estado, SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRAN;

        SELECT
            FilasAfectadas = 1,
            CodigoGenerado = @CodigoGenerado,
            OrdenParadaGenerado = @OrdenParadaGenerado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;

        SELECT
            FilasAfectadas = 0,
            CodigoGenerado = '',
            OrdenParadaGenerado = 0,
            Mensaje = ERROR_MESSAGE();
    END CATCH
END
GO

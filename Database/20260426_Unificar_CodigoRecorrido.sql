/*
    CapiMovil - Unificación de CódigoRecorrido
    Regla única:
      REC- + LEFT(UPPER(REPLACE(REPLACE(Ruta.CodigoRuta,' ',''),'-','')) + 'XXXX', 4) + correlativo global de 4 dígitos

    Ejemplos:
      CodigoRuta = 'RUTA-01' => REC-RUTA0001
      CodigoRuta = 'RT-07'   => REC-RT070002
*/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_Recorrido_Registrar
    @IdRuta UNIQUEIDENTIFIER,
    @IdBus UNIQUEIDENTIFIER,
    @IdConductor UNIQUEIDENTIFIER,
    @Fecha DATE,
    @HoraInicioProgramada TIME = NULL,
    @HoraFinProgramada TIME = NULL,
    @EstadoRecorrido VARCHAR(20),
    @Observaciones VARCHAR(300) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @CodigoRuta VARCHAR(20);
    DECLARE @Fragmento VARCHAR(4);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @CodigoRuta = r.CodigoRuta
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SET @Fragmento = LEFT(
        UPPER(REPLACE(REPLACE(ISNULL(@CodigoRuta, 'RUTA'), ' ', ''), '-', '')) + 'XXXX',
        4
    );

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'REC')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('REC', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'REC';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'REC';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRecorrido, 4) AS INT)), 0) + 1
            FROM dbo.Recorrido
            WHERE CodigoRecorrido LIKE 'REC-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('REC-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Recorrido
        (
            IdRecorrido, IdRuta, IdBus, IdConductor, CodigoRecorrido, Fecha,
            HoraInicioProgramada, HoraFinProgramada, HoraInicioReal, HoraFinReal,
            EstadoRecorrido, Observaciones, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRuta, @IdBus, @IdConductor, @CodigoGenerado, @Fecha,
            @HoraInicioProgramada, @HoraFinProgramada, NULL, NULL,
            @EstadoRecorrido, NULLIF(@Observaciones, ''), @Estado,
            SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT CAST(1 AS INT) AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT CAST(0 AS INT) AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

/*
-- OPCIONAL: normalización histórica (solo si decides estandarizar legado)
;WITH Base AS (
    SELECT r.IdRecorrido,
           ROW_NUMBER() OVER (ORDER BY r.FechaCreacion, r.IdRecorrido) AS Nro,
           LEFT(UPPER(REPLACE(REPLACE(ISNULL(ru.CodigoRuta, 'RUTA'), ' ', ''), '-', '')) + 'XXXX', 4) AS Fragmento
    FROM dbo.Recorrido r
    INNER JOIN dbo.Ruta ru ON ru.IdRuta = r.IdRuta
)
UPDATE r
   SET r.CodigoRecorrido = CONCAT('REC-', b.Fragmento, RIGHT(CONCAT('0000', b.Nro), 4))
FROM dbo.Recorrido r
INNER JOIN Base b ON b.IdRecorrido = r.IdRecorrido;
*/

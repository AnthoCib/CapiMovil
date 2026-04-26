/*
    CapiMovil - MASTER limpieza + estandarización de códigos (datos de prueba)
    Objetivo:
      1) Corregir generación de CodigoRecorrido (correlativo seguro + anti-duplicados)
      2) Estandarizar nomenclatura de códigos por entidad
      3) Limpiar datos de prueba (opcional, recomendado en sandbox)
      4) Reiniciar/sincronizar CorrelativoDocumento

    IMPORTANTE:
      - No crea tablas nuevas
      - Usa solo T-SQL / ADO.NET compatible
      - Diseñado para ejecutarse una sola vez en entorno de pruebas
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================
   0) PARÁMETROS DE EJECUCIÓN
   ========================= */
DECLARE @LimpiarDatosPrueba BIT = 1;   -- 1 = borra datos de prueba (recomendado)
DECLARE @NormalizarSinBorrar BIT = 0;  -- 1 = normaliza códigos históricos sin borrar

/* =========================
   1) FUNCIÓN AUXILIAR
   ========================= */
CREATE OR ALTER FUNCTION dbo.fn_CodeFragment
(
    @Texto NVARCHAR(250),
    @Longitud INT,
    @Fallback VARCHAR(20)
)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @Limpio VARCHAR(250) = UPPER(REPLACE(REPLACE(REPLACE(ISNULL(@Texto, ''), ' ', ''), '-', ''), '.', ''));
    IF LEN(@Limpio) = 0 SET @Limpio = UPPER(REPLACE(REPLACE(ISNULL(@Fallback, 'XXXX'), ' ', ''), '-', ''));
    IF @Longitud IS NULL OR @Longitud <= 0 SET @Longitud = 4;
    RETURN LEFT(@Limpio + REPLICATE('X', @Longitud), @Longitud);
END
GO

/* =========================
   2) SP PRIORITARIO: RECORRIDO
   ========================= */
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
    DECLARE @CorrelativoActual INT = 0;
    DECLARE @MaxTabla INT = 0;
    DECLARE @Intentos INT = 0;
    DECLARE @CodigoRuta VARCHAR(20);
    DECLARE @Fragmento VARCHAR(4);
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @TmpCorrelativo TABLE (Valor INT);

    SELECT @CodigoRuta = r.CodigoRuta
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SET @Fragmento = dbo.fn_CodeFragment(@CodigoRuta, 4, 'RUTA');

    SELECT @MaxTabla = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRecorrido, 4) AS INT)), 0)
    FROM dbo.Recorrido
    WHERE CodigoRecorrido LIKE 'REC-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]';

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'REC')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('REC', @MaxTabla);
            ELSE
            BEGIN
                SELECT @CorrelativoActual = UltimoNumero
                FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
                WHERE TipoCodigo = 'REC';

                IF ISNULL(@CorrelativoActual, 0) < @MaxTabla
                    UPDATE dbo.CorrelativoDocumento
                       SET UltimoNumero = @MaxTabla
                     WHERE TipoCodigo = 'REC';
            END
        END
        ELSE
        BEGIN
            SET @CorrelativoActual = @MaxTabla;
        END

        WHILE (1 = 1)
        BEGIN
            IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
            BEGIN
                DELETE FROM @TmpCorrelativo;
                UPDATE dbo.CorrelativoDocumento
                   SET UltimoNumero = UltimoNumero + 1
                OUTPUT inserted.UltimoNumero INTO @TmpCorrelativo(Valor)
                 WHERE TipoCodigo = 'REC';

                SELECT TOP 1 @Correlativo = Valor FROM @TmpCorrelativo;
            END
            ELSE
            BEGIN
                SET @CorrelativoActual = @CorrelativoActual + 1;
                SET @Correlativo = @CorrelativoActual;
            END

            SET @CodigoGenerado = CONCAT('REC-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

            IF NOT EXISTS (
                SELECT 1
                FROM dbo.Recorrido WITH (UPDLOCK, HOLDLOCK)
                WHERE CodigoRecorrido = @CodigoGenerado
            )
                BREAK;

            SET @Intentos += 1;
            IF @Intentos >= 50
                THROW 50001, 'No se pudo generar un CodigoRecorrido único luego de 50 intentos.', 1;
        END

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

/* =========================
   3) LIMPIEZA CONTROLADA DE DATOS DE PRUEBA
   ========================= */
DECLARE @LimpiarDatosPrueba BIT = 1;   -- 1 = borra datos de prueba (recomendado)
DECLARE @NormalizarSinBorrar BIT = 0;  -- 1 = normaliza códigos históricos sin borrar

BEGIN TRY
    BEGIN TRANSACTION;

    IF @LimpiarDatosPrueba = 1
    BEGIN
        DELETE FROM dbo.UbicacionBus;
        DELETE FROM dbo.EventoAbordaje;
        DELETE FROM dbo.Notificacion;
        DELETE FROM dbo.Incidencia;
        DELETE FROM dbo.IA_Prediccion;
        DELETE FROM dbo.IA_Consulta;
        DELETE FROM dbo.Recorrido;
        DELETE FROM dbo.RutaEstudiante;
        DELETE FROM dbo.Paradero;
        DELETE FROM dbo.Estudiante;
        DELETE FROM dbo.Conductor;
        DELETE FROM dbo.PadreFamilia;
        DELETE FROM dbo.Bus;
        DELETE FROM dbo.Ruta;
        DELETE FROM dbo.Auditoria;
    END

    IF @NormalizarSinBorrar = 1 AND @LimpiarDatosPrueba = 0
    BEGIN
        ;WITH R AS (
            SELECT IdRuta, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdRuta) AS Nro,
                   dbo.fn_CodeFragment(Nombre, 6, 'RUTA') AS Fr
            FROM dbo.Ruta
        )
        UPDATE t SET CodigoRuta = CONCAT('RUT-', r.Fr, RIGHT(CONCAT('0000', r.Nro), 4))
        FROM dbo.Ruta t INNER JOIN R r ON r.IdRuta = t.IdRuta;

        ;WITH B AS (
            SELECT IdBus, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdBus) AS Nro,
                   dbo.fn_CodeFragment(Placa, 6, 'BUS') AS Fr
            FROM dbo.Bus
        )
        UPDATE t SET CodigoBus = CONCAT('BUS-', b.Fr, RIGHT(CONCAT('0000', b.Nro), 4))
        FROM dbo.Bus t INNER JOIN B b ON b.IdBus = t.IdBus;

        ;WITH P AS (
            SELECT IdParadero, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdParadero) AS Nro,
                   dbo.fn_CodeFragment(Nombre, 6, 'PARADA') AS Fr
            FROM dbo.Paradero
        )
        UPDATE t SET CodigoParadero = CONCAT('PAR-', p.Fr, RIGHT(CONCAT('0000', p.Nro), 4))
        FROM dbo.Paradero t INNER JOIN P p ON p.IdParadero = t.IdParadero;

        ;WITH E AS (
            SELECT IdEstudiante, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdEstudiante) AS Nro,
                   dbo.fn_CodeFragment(DNI, 6, 'ESTUD') AS Fr
            FROM dbo.Estudiante
        )
        UPDATE t SET CodigoEstudiante = CONCAT('EST-', e.Fr, RIGHT(CONCAT('0000', e.Nro), 4))
        FROM dbo.Estudiante t INNER JOIN E e ON e.IdEstudiante = t.IdEstudiante;

        ;WITH PF AS (
            SELECT IdPadre, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdPadre) AS Nro,
                   dbo.fn_CodeFragment(DNI, 6, 'PADRE') AS Fr
            FROM dbo.PadreFamilia
        )
        UPDATE t SET CodigoPadre = CONCAT('PAD-', pf.Fr, RIGHT(CONCAT('0000', pf.Nro), 4))
        FROM dbo.PadreFamilia t INNER JOIN PF pf ON pf.IdPadre = t.IdPadre;

        ;WITH C AS (
            SELECT IdConductor, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdConductor) AS Nro,
                   dbo.fn_CodeFragment(DNI, 6, 'CONDUC') AS Fr
            FROM dbo.Conductor
        )
        UPDATE t SET CodigoConductor = CONCAT('CON-', c.Fr, RIGHT(CONCAT('0000', c.Nro), 4))
        FROM dbo.Conductor t INNER JOIN C c ON c.IdConductor = t.IdConductor;

        ;WITH RE AS (
            SELECT IdRutaEstudiante, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdRutaEstudiante) AS Nro,
                   dbo.fn_CodeFragment(COALESCE(CodigoAsignacion, 'ASIG'), 6, 'ASIG') AS Fr
            FROM dbo.RutaEstudiante
        )
        UPDATE t SET CodigoAsignacion = CONCAT('RAS-', re.Fr, RIGHT(CONCAT('0000', re.Nro), 4))
        FROM dbo.RutaEstudiante t INNER JOIN RE re ON re.IdRutaEstudiante = t.IdRutaEstudiante;

        ;WITH RC AS (
            SELECT r.IdRecorrido, ROW_NUMBER() OVER (ORDER BY r.FechaCreacion, r.IdRecorrido) AS Nro,
                   dbo.fn_CodeFragment(ru.CodigoRuta, 4, 'RUTA') AS Fr
            FROM dbo.Recorrido r
            INNER JOIN dbo.Ruta ru ON ru.IdRuta = r.IdRuta
        )
        UPDATE t SET CodigoRecorrido = CONCAT('REC-', rc.Fr, RIGHT(CONCAT('0000', rc.Nro), 4))
        FROM dbo.Recorrido t INNER JOIN RC rc ON rc.IdRecorrido = t.IdRecorrido;

        ;WITH I AS (
            SELECT IdIncidencia, ROW_NUMBER() OVER (ORDER BY FechaCreacion, IdIncidencia) AS Nro,
                   dbo.fn_CodeFragment(TipoIncidencia, 6, 'INCIDE') AS Fr
            FROM dbo.Incidencia
        )
        UPDATE t SET CodigoIncidencia = CONCAT('INC-', i.Fr, RIGHT(CONCAT('0000', i.Nro), 4))
        FROM dbo.Incidencia t INNER JOIN I i ON i.IdIncidencia = t.IdIncidencia;

        ;WITH A AS (
            SELECT IdAuditoria, ROW_NUMBER() OVER (ORDER BY Fecha, IdAuditoria) AS Nro,
                   dbo.fn_CodeFragment(Tabla, 3, 'AUD') AS Fr
            FROM dbo.Auditoria
        )
        UPDATE t SET CodigoAuditoria = CONCAT('AUD-', a.Fr, RIGHT(CONCAT('0000', a.Nro), 4))
        FROM dbo.Auditoria t INNER JOIN A a ON a.IdAuditoria = t.IdAuditoria;
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* =========================
   4) REINICIO / SINCRONIZACIÓN CORRELATIVOS
   ========================= */
IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
BEGIN
    MERGE dbo.CorrelativoDocumento AS tgt
    USING (
        SELECT 'RUT' AS TipoCodigo, ISNULL(MAX(TRY_CAST(RIGHT(CodigoRuta, 4) AS INT)), 0) AS UltimoNumero FROM dbo.Ruta
        UNION ALL SELECT 'BUS', ISNULL(MAX(TRY_CAST(RIGHT(CodigoBus, 4) AS INT)), 0) FROM dbo.Bus
        UNION ALL SELECT 'PAR', ISNULL(MAX(TRY_CAST(RIGHT(CodigoParadero, 4) AS INT)), 0) FROM dbo.Paradero
        UNION ALL SELECT 'RAS', ISNULL(MAX(TRY_CAST(RIGHT(CodigoAsignacion, 4) AS INT)), 0) FROM dbo.RutaEstudiante
        UNION ALL SELECT 'REC', ISNULL(MAX(TRY_CAST(RIGHT(CodigoRecorrido, 4) AS INT)), 0) FROM dbo.Recorrido
        UNION ALL SELECT 'INC', ISNULL(MAX(TRY_CAST(RIGHT(CodigoIncidencia, 4) AS INT)), 0) FROM dbo.Incidencia
        UNION ALL SELECT 'PAD', ISNULL(MAX(TRY_CAST(RIGHT(CodigoPadre, 4) AS INT)), 0) FROM dbo.PadreFamilia
        UNION ALL SELECT 'CON', ISNULL(MAX(TRY_CAST(RIGHT(CodigoConductor, 4) AS INT)), 0) FROM dbo.Conductor
        UNION ALL SELECT 'EST', ISNULL(MAX(TRY_CAST(RIGHT(CodigoEstudiante, 4) AS INT)), 0) FROM dbo.Estudiante
        UNION ALL SELECT 'AUD', ISNULL(MAX(TRY_CAST(RIGHT(CodigoAuditoria, 4) AS INT)), 0) FROM dbo.Auditoria
    ) AS src
    ON tgt.TipoCodigo = src.TipoCodigo
    WHEN MATCHED THEN
        UPDATE SET tgt.UltimoNumero = src.UltimoNumero
    WHEN NOT MATCHED THEN
        INSERT (TipoCodigo, UltimoNumero) VALUES (src.TipoCodigo, src.UltimoNumero);
END
GO

/* =========================
   5) DIAGNÓSTICO FINAL RÁPIDO
   ========================= */
SELECT 'Ruta' AS Entidad, COUNT(*) AS Total, SUM(CASE WHEN CodigoRuta LIKE 'RUT-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) AS Inconsistentes FROM dbo.Ruta
UNION ALL
SELECT 'Bus', COUNT(*), SUM(CASE WHEN CodigoBus LIKE 'BUS-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Bus
UNION ALL
SELECT 'Paradero', COUNT(*), SUM(CASE WHEN CodigoParadero LIKE 'PAR-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Paradero
UNION ALL
SELECT 'Recorrido', COUNT(*), SUM(CASE WHEN CodigoRecorrido LIKE 'REC-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Recorrido
UNION ALL
SELECT 'RutaEstudiante', COUNT(*), SUM(CASE WHEN CodigoAsignacion LIKE 'RAS-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.RutaEstudiante
UNION ALL
SELECT 'Incidencia', COUNT(*), SUM(CASE WHEN CodigoIncidencia LIKE 'INC-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Incidencia
UNION ALL
SELECT 'PadreFamilia', COUNT(*), SUM(CASE WHEN CodigoPadre LIKE 'PAD-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.PadreFamilia
UNION ALL
SELECT 'Conductor', COUNT(*), SUM(CASE WHEN CodigoConductor LIKE 'CON-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Conductor
UNION ALL
SELECT 'Estudiante', COUNT(*), SUM(CASE WHEN CodigoEstudiante LIKE 'EST-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Estudiante
UNION ALL
SELECT 'Auditoria', COUNT(*), SUM(CASE WHEN CodigoAuditoria LIKE 'AUD-[A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]' THEN 0 ELSE 1 END) FROM dbo.Auditoria;
GO

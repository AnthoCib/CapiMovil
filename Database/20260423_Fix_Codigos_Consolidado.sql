/*
    Script consolidado - Corrección de generación de códigos de negocio
    Fecha: 2026-04-23
    Objetivo:
      - eliminar generación con placeholders (XXXXXX)
      - centralizar sanitización/fragmentos en funciones reutilizables
      - unificar patrón de retorno para ADO.NET
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================================================
   FUNCIONES AUXILIARES
   ========================================================= */
CREATE OR ALTER FUNCTION dbo.fn_Codigo_Sanitizar(@Texto NVARCHAR(4000))
RETURNS NVARCHAR(4000)
AS
BEGIN
    DECLARE @s NVARCHAR(4000) = UPPER(LTRIM(RTRIM(ISNULL(@Texto, N''))));
    DECLARE @r NVARCHAR(4000) = N'';
    DECLARE @i INT = 1;
    DECLARE @c NCHAR(1);

    SET @s = REPLACE(@s, N'Á', N'A');
    SET @s = REPLACE(@s, N'É', N'E');
    SET @s = REPLACE(@s, N'Í', N'I');
    SET @s = REPLACE(@s, N'Ó', N'O');
    SET @s = REPLACE(@s, N'Ú', N'U');
    SET @s = REPLACE(@s, N'Ü', N'U');
    SET @s = REPLACE(@s, N'Ñ', N'N');

    WHILE @i <= LEN(@s)
    BEGIN
        SET @c = SUBSTRING(@s, @i, 1);
        IF @c LIKE N'[A-Z0-9]'
            SET @r += @c;
        SET @i += 1;
    END

    RETURN @r;
END
GO

CREATE OR ALTER FUNCTION dbo.fn_Codigo_Fragmento
(
    @Texto NVARCHAR(4000),
    @Longitud INT,
    @Fallback NVARCHAR(20)
)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @base NVARCHAR(4000) = dbo.fn_Codigo_Sanitizar(@Texto);
    DECLARE @fb NVARCHAR(4000) = dbo.fn_Codigo_Sanitizar(@Fallback);

    IF @Longitud IS NULL OR @Longitud <= 0 SET @Longitud = 1;
    IF LEN(@fb) = 0 SET @fb = N'GEN';

    IF LEN(@base) = 0
        SET @base = @fb;

    WHILE LEN(@base) < @Longitud
        SET @base = @base + LEFT(@fb, 1);

    RETURN CONVERT(VARCHAR(20), LEFT(@base, @Longitud));
END
GO

/* =========================================================
   TRANSPORTE
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Ruta_Registrar
    @Nombre VARCHAR(120),
    @Descripcion VARCHAR(250) = NULL,
    @Turno VARCHAR(20),
    @HoraInicio TIME,
    @HoraFin TIME,
    @PuntoInicio VARCHAR(200) = NULL,
    @PuntoFin VARCHAR(200) = NULL,
    @EstadoRuta VARCHAR(20),
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@Nombre, 3, 'RUT') +
        dbo.fn_Codigo_Fragmento(@Turno, 3, 'TUR');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'RUT')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('RUT', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'RUT';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'RUT';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRuta, 4) AS INT)), 0) + 1
            FROM dbo.Ruta
            WHERE CodigoRuta LIKE 'RUT-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('RUT-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Ruta
        (
            IdRuta, CodigoRuta, Nombre, Descripcion, Turno, HoraInicio, HoraFin,
            PuntoInicio, PuntoFin, EstadoRuta, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @CodigoGenerado, @Nombre, NULLIF(@Descripcion, ''), @Turno, @HoraInicio, @HoraFin,
            NULLIF(@PuntoInicio, ''), NULLIF(@PuntoFin, ''), @EstadoRuta, @Estado,
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

CREATE OR ALTER PROCEDURE dbo.sp_Bus_Registrar
    @Placa VARCHAR(20),
    @Marca VARCHAR(60) = NULL,
    @Modelo VARCHAR(60) = NULL,
    @Color VARCHAR(30) = NULL,
    @Anio INT = NULL,
    @Capacidad INT,
    @EstadoOperacion VARCHAR(20),
    @SeguroVigente BIT,
    @FechaVencimientoSOAT DATE = NULL,
    @FechaRevisionTecnica DATE = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@Placa, 3, 'PLA') +
        dbo.fn_Codigo_Fragmento(@Marca, 3, 'MAR');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'BUS')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('BUS', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'BUS';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'BUS';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoBus, 4) AS INT)), 0) + 1
            FROM dbo.Bus
            WHERE CodigoBus LIKE 'BUS-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('BUS-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Bus
        (
            IdBus, CodigoBus, Placa, Marca, Modelo, Color, Anio, Capacidad,
            EstadoOperacion, SeguroVigente, FechaVencimientoSOAT, FechaRevisionTecnica,
            Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @CodigoGenerado, @Placa, NULLIF(@Marca, ''), NULLIF(@Modelo, ''), NULLIF(@Color, ''),
            @Anio, @Capacidad, @EstadoOperacion, @SeguroVigente, @FechaVencimientoSOAT, @FechaRevisionTecnica,
            @Estado, SYSUTCDATETIME(), NULL, NULL
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

CREATE OR ALTER PROCEDURE dbo.sp_Paradero_Registrar
    @IdRuta UNIQUEIDENTIFIER,
    @Nombre VARCHAR(120),
    @Direccion VARCHAR(250),
    @Latitud DECIMAL(10,7) = NULL,
    @Longitud DECIMAL(10,7) = NULL,
    @OrdenParada INT,
    @HoraEstimada TIME = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @RutaNombre VARCHAR(120);
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @RutaNombre = r.Nombre
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@Nombre, 3, 'PAR') +
        dbo.fn_Codigo_Fragmento(@RutaNombre, 3, 'RUT');

    BEGIN TRANSACTION;
    BEGIN TRY
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

        SET @CodigoGenerado = CONCAT('PAR-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Paradero
        (
            IdParadero, IdRuta, CodigoParadero, Nombre, Direccion, Latitud, Longitud,
            OrdenParada, HoraEstimada, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRuta, @CodigoGenerado, @Nombre, @Direccion, @Latitud, @Longitud,
            @OrdenParada, @HoraEstimada, @Estado, SYSUTCDATETIME(), NULL, NULL
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

CREATE OR ALTER PROCEDURE dbo.sp_RutaEstudiante_Registrar
    @IdRuta UNIQUEIDENTIFIER,
    @IdEstudiante UNIQUEIDENTIFIER,
    @IdParaderoSubida UNIQUEIDENTIFIER = NULL,
    @IdParaderoBajada UNIQUEIDENTIFIER = NULL,
    @FechaInicioVigencia DATE,
    @FechaFinVigencia DATE = NULL,
    @EstadoAsignacion VARCHAR(20),
    @Observaciones VARCHAR(300) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @RutaNombre VARCHAR(120);
    DECLARE @EstudianteNombre VARCHAR(80);
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @RutaNombre = r.Nombre FROM dbo.Ruta r WHERE r.IdRuta = @IdRuta;
    SELECT @EstudianteNombre = e.Nombres FROM dbo.Estudiante e WHERE e.IdEstudiante = @IdEstudiante;

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@EstudianteNombre, 2, 'ES') +
        dbo.fn_Codigo_Fragmento(@RutaNombre, 4, 'RUTA');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'RAS')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('RAS', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'RAS';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'RAS';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRutaEstudiante, 4) AS INT)), 0) + 1
            FROM dbo.RutaEstudiante
            WHERE CodigoRutaEstudiante LIKE 'RAS-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('RAS-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.RutaEstudiante
        (
            IdRutaEstudiante, IdRuta, IdEstudiante, IdParaderoSubida, IdParaderoBajada,
            CodigoRutaEstudiante, FechaInicioVigencia, FechaFinVigencia, EstadoAsignacion,
            Observaciones, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRuta, @IdEstudiante, @IdParaderoSubida, @IdParaderoBajada,
            @CodigoGenerado, @FechaInicioVigencia, @FechaFinVigencia, @EstadoAsignacion,
            NULLIF(@Observaciones, ''), @Estado, SYSUTCDATETIME(), NULL, NULL
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

/* =========================================================
   OPERACIONES
   ========================================================= */
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
    DECLARE @RutaNombre VARCHAR(120);
    DECLARE @Turno VARCHAR(20);
    DECLARE @Fragmento VARCHAR(4);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @RutaNombre = r.Nombre, @Turno = r.Turno
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@RutaNombre, 2, 'RU') +
        dbo.fn_Codigo_Fragmento(@Turno, 2, 'TU');

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

CREATE OR ALTER PROCEDURE dbo.sp_UbicacionBus_Registrar
    @IdRecorrido UNIQUEIDENTIFIER,
    @Latitud DECIMAL(10,7),
    @Longitud DECIMAL(10,7),
    @Velocidad DECIMAL(8,2) = NULL,
    @PrecisionMetros DECIMAL(8,2) = NULL,
    @FechaHora DATETIME,
    @Fuente VARCHAR(30) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @CodigoRecorrido VARCHAR(20);
    DECLARE @Fragmento VARCHAR(4);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @CodigoRecorrido = r.CodigoRecorrido FROM dbo.Recorrido r WHERE r.IdRecorrido = @IdRecorrido;
    SET @Fragmento = dbo.fn_Codigo_Fragmento(REPLACE(@CodigoRecorrido, 'REC-', ''), 4, 'UBIC');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'UBI')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('UBI', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'UBI';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'UBI';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoUbicacion, 4) AS INT)), 0) + 1
            FROM dbo.UbicacionBus
            WHERE CodigoUbicacion LIKE 'UBI-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('UBI-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.UbicacionBus
        (
            IdUbicacion, IdRecorrido, CodigoUbicacion, Latitud, Longitud,
            Velocidad, PrecisionMetros, FechaHora, Fuente, Estado,
            FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRecorrido, @CodigoGenerado, @Latitud, @Longitud,
            @Velocidad, @PrecisionMetros, @FechaHora, NULLIF(@Fuente, ''), @Estado,
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

CREATE OR ALTER PROCEDURE dbo.sp_EventoAbordaje_Registrar
    @IdRecorrido UNIQUEIDENTIFIER,
    @IdEstudiante UNIQUEIDENTIFIER,
    @IdParadero UNIQUEIDENTIFIER = NULL,
    @RegistradoPor UNIQUEIDENTIFIER = NULL,
    @TipoEvento VARCHAR(20),
    @FechaHora DATETIME,
    @Observacion VARCHAR(300) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Nombre VARCHAR(80);
    DECLARE @Apellido VARCHAR(60);
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @Nombre = e.Nombres, @Apellido = e.ApellidoPaterno
    FROM dbo.Estudiante e
    WHERE e.IdEstudiante = @IdEstudiante;

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@TipoEvento, 2, 'EV') +
        dbo.fn_Codigo_Fragmento(@Nombre, 2, 'NO') +
        dbo.fn_Codigo_Fragmento(@Apellido, 2, 'AP');

    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @TotalSubidas INT = 0;
        DECLARE @TotalBajadas INT = 0;
        DECLARE @TotalAusentes INT = 0;
        DECLARE @TotalNoAbordo INT = 0;
        DECLARE @TipoEventoNormalizado VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@TipoEvento, ''))));

        IF @TipoEventoNormalizado NOT IN ('SUBIDA', 'BAJADA', 'AUSENTE', 'NO_ABORDO')
            THROW 50001, 'El tipo de evento no es válido.', 1;

        SELECT
            @TotalSubidas = SUM(CASE WHEN UPPER(LTRIM(RTRIM(TipoEvento))) = 'SUBIDA' THEN 1 ELSE 0 END),
            @TotalBajadas = SUM(CASE WHEN UPPER(LTRIM(RTRIM(TipoEvento))) = 'BAJADA' THEN 1 ELSE 0 END),
            @TotalAusentes = SUM(CASE WHEN UPPER(LTRIM(RTRIM(TipoEvento))) = 'AUSENTE' THEN 1 ELSE 0 END),
            @TotalNoAbordo = SUM(CASE WHEN UPPER(LTRIM(RTRIM(TipoEvento))) = 'NO_ABORDO' THEN 1 ELSE 0 END)
        FROM dbo.EventoAbordaje WITH (UPDLOCK, HOLDLOCK)
        WHERE IdRecorrido = @IdRecorrido
          AND IdEstudiante = @IdEstudiante
          AND FechaEliminacion IS NULL
          AND Estado = 1;

        SET @TotalSubidas = ISNULL(@TotalSubidas, 0);
        SET @TotalBajadas = ISNULL(@TotalBajadas, 0);
        SET @TotalAusentes = ISNULL(@TotalAusentes, 0);
        SET @TotalNoAbordo = ISNULL(@TotalNoAbordo, 0);

        IF @TipoEventoNormalizado = 'SUBIDA'
        BEGIN
            IF @TotalAusentes > 0
                THROW 50002, 'No se puede registrar este evento porque ya fue marcado como ausente.', 1;

            IF @TotalNoAbordo > 0
                THROW 50003, 'No se puede registrar este evento porque ya fue marcado como no abordó.', 1;

            IF @TotalSubidas > 0 AND @TotalBajadas = 0
                THROW 50004, 'El alumno ya registró una subida en este recorrido.', 1;

            IF @TotalSubidas > 0 AND @TotalBajadas > 0
                THROW 50005, 'El alumno ya completó su ciclo de abordaje en este recorrido.', 1;
        END
        ELSE IF @TipoEventoNormalizado = 'BAJADA'
        BEGIN
            IF @TotalAusentes > 0
                THROW 50006, 'No se puede registrar este evento porque ya fue marcado como ausente.', 1;

            IF @TotalNoAbordo > 0
                THROW 50007, 'No se puede registrar este evento porque ya fue marcado como no abordó.', 1;

            IF @TotalSubidas = 0
                THROW 50008, 'No se puede registrar la bajada porque el alumno aún no tiene una subida.', 1;

            IF @TotalBajadas > 0
                THROW 50009, 'El alumno ya registró una bajada en este recorrido.', 1;
        END
        ELSE IF @TipoEventoNormalizado = 'AUSENTE'
        BEGIN
            IF @TotalSubidas > 0 OR @TotalBajadas > 0
                THROW 50010, 'No se puede registrar este evento porque el alumno ya tiene eventos de abordaje en este recorrido.', 1;

            IF @TotalAusentes > 0
                THROW 50011, 'El alumno ya fue marcado como ausente en este recorrido.', 1;

            IF @TotalNoAbordo > 0
                THROW 50012, 'No se puede registrar ausente porque el alumno ya fue marcado como no abordó.', 1;
        END
        ELSE IF @TipoEventoNormalizado = 'NO_ABORDO'
        BEGIN
            IF @TotalSubidas > 0 OR @TotalBajadas > 0
                THROW 50013, 'No se puede registrar este evento porque el alumno ya tiene eventos de abordaje en este recorrido.', 1;

            IF @TotalNoAbordo > 0
                THROW 50014, 'El alumno ya fue marcado como no abordó en este recorrido.', 1;

            IF @TotalAusentes > 0
                THROW 50015, 'No se puede registrar no abordó porque el alumno ya fue marcado como ausente.', 1;
        END

        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'EVE')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('EVE', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'EVE';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'EVE';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoEvento, 4) AS INT)), 0) + 1
            FROM dbo.EventoAbordaje
            WHERE CodigoEvento LIKE 'EVE-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('EVE-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.EventoAbordaje
        (
            IdEvento, IdRecorrido, IdEstudiante, IdParadero, RegistradoPor,
            CodigoEvento, TipoEvento, FechaHora, Observacion, Estado,
            FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdRecorrido, @IdEstudiante, @IdParadero, @RegistradoPor,
            @CodigoGenerado, @TipoEventoNormalizado, @FechaHora, NULLIF(@Observacion, ''), @Estado,
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

CREATE OR ALTER PROCEDURE dbo.sp_EventoAbordaje_ObtenerResumenEstudianteRecorrido
    @IdRecorrido UNIQUEIDENTIFIER,
    @IdEstudiante UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Eventos AS
    (
        SELECT
            UPPER(LTRIM(RTRIM(e.TipoEvento))) AS TipoEvento,
            e.FechaHora
        FROM dbo.EventoAbordaje e
        WHERE e.IdRecorrido = @IdRecorrido
          AND e.IdEstudiante = @IdEstudiante
          AND e.FechaEliminacion IS NULL
          AND e.Estado = 1
    )
    SELECT
        @IdRecorrido AS IdRecorrido,
        @IdEstudiante AS IdEstudiante,
        COUNT(1) AS TotalEventos,
        SUM(CASE WHEN TipoEvento = 'SUBIDA' THEN 1 ELSE 0 END) AS TotalSubidas,
        SUM(CASE WHEN TipoEvento = 'BAJADA' THEN 1 ELSE 0 END) AS TotalBajadas,
        SUM(CASE WHEN TipoEvento = 'AUSENTE' THEN 1 ELSE 0 END) AS TotalAusentes,
        SUM(CASE WHEN TipoEvento = 'NO_ABORDO' THEN 1 ELSE 0 END) AS TotalNoAbordo,
        (SELECT TOP 1 TipoEvento FROM Eventos ORDER BY FechaHora DESC) AS UltimoTipoEvento
    FROM Eventos;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Notificacion_Registrar
    @IdPadre UNIQUEIDENTIFIER,
    @IdEstudiante UNIQUEIDENTIFIER = NULL,
    @Titulo VARCHAR(150),
    @Mensaje VARCHAR(500),
    @TipoNotificacion VARCHAR(30),
    @Canal VARCHAR(20),
    @Leido BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@TipoNotificacion, 3, 'NOT') +
        dbo.fn_Codigo_Fragmento(@Canal, 3, 'CAN');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'NOT')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('NOT', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'NOT';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'NOT';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoNotificacion, 4) AS INT)), 0) + 1
            FROM dbo.Notificacion
            WHERE CodigoNotificacion LIKE 'NOT-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('NOT-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Notificacion
        (
            IdNotificacion, IdPadre, IdEstudiante, CodigoNotificacion, Titulo, Mensaje,
            TipoNotificacion, Canal, Leido, FechaLectura, FechaEnvio, Estado,
            FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdPadre, @IdEstudiante, @CodigoGenerado, @Titulo, @Mensaje,
            @TipoNotificacion, @Canal, @Leido, CASE WHEN @Leido = 1 THEN SYSUTCDATETIME() ELSE NULL END,
            SYSUTCDATETIME(), 1, SYSUTCDATETIME(), NULL, NULL
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

CREATE OR ALTER PROCEDURE dbo.sp_Incidencia_Registrar
    @IdRecorrido UNIQUEIDENTIFIER,
    @IdConductor UNIQUEIDENTIFIER,
    @ReportadoPor UNIQUEIDENTIFIER = NULL,
    @TipoIncidencia VARCHAR(40),
    @Descripcion VARCHAR(500),
    @FechaHora DATETIME,
    @EstadoIncidencia VARCHAR(20),
    @Prioridad VARCHAR(20),
    @Solucion VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @IdGenerado UNIQUEIDENTIFIER = NEWID();

    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@TipoIncidencia, 3, 'INC') +
        dbo.fn_Codigo_Fragmento(@Prioridad, 3, 'PRI');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'INC')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('INC', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'INC';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'INC';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoIncidencia, 4) AS INT)), 0) + 1
            FROM dbo.Incidencia
            WHERE CodigoIncidencia LIKE 'INC-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('INC-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Incidencia
        (
            IdIncidencia, IdRecorrido, IdConductor, ReportadoPor, CodigoIncidencia,
            TipoIncidencia, Descripcion, FechaHora, EstadoIncidencia, Prioridad,
            FechaCierre, Solucion, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            @IdGenerado, @IdRecorrido, @IdConductor, @ReportadoPor, @CodigoGenerado,
            @TipoIncidencia, @Descripcion, @FechaHora, @EstadoIncidencia, @Prioridad,
            NULL, NULLIF(@Solucion, ''), 1, SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT CAST(1 AS INT) AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, @IdGenerado AS IdIncidencia;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT CAST(0 AS INT) AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, CAST(NULL AS UNIQUEIDENTIFIER) AS IdIncidencia, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO
/* =========================================================
   PERSONAS (extensión del mismo patrón)
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_Registrar
    @IdUsuario UNIQUEIDENTIFIER,
    @Nombres VARCHAR(80),
    @ApellidoPaterno VARCHAR(60),
    @ApellidoMaterno VARCHAR(60),
    @DNI VARCHAR(8) = NULL,
    @Telefono VARCHAR(20) = NULL,
    @TelefonoAlterno VARCHAR(20) = NULL,
    @Direccion VARCHAR(200) = NULL,
    @CorreoContacto VARCHAR(120) = NULL,
    @Estado BIT,
    @CodigoGenerado VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6) =
        dbo.fn_Codigo_Fragmento(@Nombres, 2, 'PA') +
        dbo.fn_Codigo_Fragmento(@ApellidoPaterno, 2, 'PA') +
        dbo.fn_Codigo_Fragmento(@ApellidoMaterno, 2, 'FA');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'PADRE')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('PADRE', 0);

            UPDATE dbo.CorrelativoDocumento SET UltimoNumero = UltimoNumero + 1 WHERE TipoCodigo = 'PADRE';
            SELECT @Correlativo = UltimoNumero FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'PADRE';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoPadre, 4) AS INT)), 0) + 1
            FROM dbo.PadreFamilia
            WHERE CodigoPadre LIKE 'PAD-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('PAD-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.PadreFamilia
        (
            IdPadre, IdUsuario, CodigoPadre, Nombres, ApellidoPaterno, ApellidoMaterno, DNI,
            Telefono, TelefonoAlterno, Direccion, CorreoContacto, Estado,
            FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdUsuario, @CodigoGenerado, @Nombres, @ApellidoPaterno, @ApellidoMaterno, NULLIF(@DNI, ''),
            NULLIF(@Telefono, ''), NULLIF(@TelefonoAlterno, ''), NULLIF(@Direccion, ''), NULLIF(@CorreoContacto, ''), @Estado,
            SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT CAST(1 AS INT) AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @CodigoGenerado = NULL;
        SELECT CAST(0 AS INT) AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_Registrar
    @IdUsuario UNIQUEIDENTIFIER,
    @Nombres VARCHAR(80),
    @ApellidoPaterno VARCHAR(60),
    @ApellidoMaterno VARCHAR(60),
    @DNI VARCHAR(8) = NULL,
    @Licencia VARCHAR(30),
    @CategoriaLicencia VARCHAR(10) = NULL,
    @Telefono VARCHAR(20) = NULL,
    @Direccion VARCHAR(200) = NULL,
    @Estado BIT,
    @CodigoGenerado VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6) =
        dbo.fn_Codigo_Fragmento(@Nombres, 2, 'CO') +
        dbo.fn_Codigo_Fragmento(@ApellidoPaterno, 2, 'ND') +
        dbo.fn_Codigo_Fragmento(@Licencia, 2, 'OR');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'CONDUCTOR')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('CONDUCTOR', 0);

            UPDATE dbo.CorrelativoDocumento SET UltimoNumero = UltimoNumero + 1 WHERE TipoCodigo = 'CONDUCTOR';
            SELECT @Correlativo = UltimoNumero FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'CONDUCTOR';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoConductor, 4) AS INT)), 0) + 1
            FROM dbo.Conductor
            WHERE CodigoConductor LIKE 'CON-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('CON-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Conductor
        (
            IdConductor, IdUsuario, CodigoConductor, Nombres, ApellidoPaterno, ApellidoMaterno,
            DNI, Licencia, CategoriaLicencia, Telefono, Direccion, Estado,
            FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdUsuario, @CodigoGenerado, @Nombres, @ApellidoPaterno, @ApellidoMaterno,
            NULLIF(@DNI, ''), @Licencia, NULLIF(@CategoriaLicencia, ''), NULLIF(@Telefono, ''), NULLIF(@Direccion, ''), @Estado,
            SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT CAST(1 AS INT) AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @CodigoGenerado = NULL;
        SELECT CAST(0 AS INT) AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Estudiante_Registrar
    @IdPadre UNIQUEIDENTIFIER,
    @Nombres VARCHAR(80),
    @ApellidoPaterno VARCHAR(60),
    @ApellidoMaterno VARCHAR(60),
    @DNI VARCHAR(8) = NULL,
    @FechaNacimiento DATE = NULL,
    @Genero VARCHAR(1) = NULL,
    @Grado VARCHAR(30) = NULL,
    @Seccion VARCHAR(20) = NULL,
    @Direccion VARCHAR(200) = NULL,
    @LatitudCasa DECIMAL(10,7) = NULL,
    @LongitudCasa DECIMAL(10,7) = NULL,
    @FotoUrl VARCHAR(250) = NULL,
    @Observaciones VARCHAR(300) = NULL,
    @Estado BIT,
    @CodigoGenerado VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(6) =
        dbo.fn_Codigo_Fragmento(@Nombres, 2, 'ES') +
        dbo.fn_Codigo_Fragmento(@ApellidoPaterno, 2, 'TU') +
        dbo.fn_Codigo_Fragmento(@Grado, 2, 'DI');

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'ESTUDIANTE')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('ESTUDIANTE', 0);

            UPDATE dbo.CorrelativoDocumento SET UltimoNumero = UltimoNumero + 1 WHERE TipoCodigo = 'ESTUDIANTE';
            SELECT @Correlativo = UltimoNumero FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'ESTUDIANTE';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoEstudiante, 4) AS INT)), 0) + 1
            FROM dbo.Estudiante
            WHERE CodigoEstudiante LIKE 'EST-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('EST-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Estudiante
        (
            IdEstudiante, IdPadre, CodigoEstudiante, Nombres, ApellidoPaterno, ApellidoMaterno,
            DNI, FechaNacimiento, Genero, Grado, Seccion, Direccion,
            LatitudCasa, LongitudCasa, FotoUrl, Observaciones,
            Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            NEWID(), @IdPadre, @CodigoGenerado, @Nombres, @ApellidoPaterno, @ApellidoMaterno,
            NULLIF(@DNI, ''), @FechaNacimiento, NULLIF(@Genero, ''), NULLIF(@Grado, ''), NULLIF(@Seccion, ''), NULLIF(@Direccion, ''),
            @LatitudCasa, @LongitudCasa, NULLIF(@FotoUrl, ''), NULLIF(@Observaciones, ''),
            @Estado, SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT CAST(1 AS INT) AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @CodigoGenerado = NULL;
        SELECT CAST(0 AS INT) AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

    IF @IdPadre IS NULL OR @IdPadre = '00000000-0000-0000-0000-000000000000'
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Debe indicar el padre de familia.' AS Mensaje;
        RETURN;
    END

    IF @DNI IS NOT NULL
    BEGIN
        SET @DNI = LTRIM(RTRIM(@DNI));
        IF LEN(@DNI) <> 8 OR @DNI LIKE '%[^0-9]%'
        BEGIN
            SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El DNI del estudiante debe tener 8 dígitos.' AS Mensaje;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM dbo.Estudiante e
            WHERE e.DNI = @DNI
              AND e.FechaEliminacion IS NULL)
        BEGIN
            SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Ya existe un estudiante con el DNI ingresado.' AS Mensaje;
            RETURN;
        END
    END

    IF @Estado = 0
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El estudiante debe registrarse en estado ACTIVO.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT;
    DECLARE @Fragmento NVARCHAR(6);

    SET @Fragmento =
        UPPER(LEFT(LTRIM(RTRIM(ISNULL(@Nombres, 'X'))), 2)) +
        UPPER(LEFT(LTRIM(RTRIM(ISNULL(@ApellidoPaterno, 'X'))), 2)) +
        UPPER(LEFT(LTRIM(RTRIM(ISNULL(@ApellidoMaterno, 'X'))), 2));

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'ESTUDIANTE')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('ESTUDIANTE', 0);

            UPDATE dbo.CorrelativoDocumento
            SET UltimoNumero = UltimoNumero + 1
            WHERE TipoCodigo = 'ESTUDIANTE';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'ESTUDIANTE';
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
            IdPadre, CodigoEstudiante, Nombres, ApellidoPaterno, ApellidoMaterno,
            DNI, FechaNacimiento, Genero, Grado, Seccion, Direccion,
            LatitudCasa, LongitudCasa, FotoUrl, Observaciones, Estado
        )
        VALUES
        (
            @IdPadre, @CodigoGenerado, @Nombres, @ApellidoPaterno, @ApellidoMaterno,
            @DNI, @FechaNacimiento, @Genero, @Grado, @Seccion, @Direccion,
            @LatitudCasa, @LongitudCasa, @FotoUrl, @Observaciones, 1
        );

        COMMIT TRANSACTION;
        SELECT 1 AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
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

    IF EXISTS (SELECT 1 FROM dbo.Ruta WHERE IdRuta = @IdRuta AND (FechaEliminacion IS NOT NULL OR Estado = 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede asignar estudiantes a una ruta inactiva.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Estudiante WHERE IdEstudiante = @IdEstudiante AND (FechaEliminacion IS NOT NULL OR Estado = 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede asignar un estudiante inactivo.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.RutaEstudiante re
        WHERE re.IdRuta = @IdRuta
          AND re.IdEstudiante = @IdEstudiante
          AND re.FechaEliminacion IS NULL
          AND re.Estado = 1)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El estudiante ya tiene una asignación activa en la ruta.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT;
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @RutaNombre VARCHAR(120);
    DECLARE @EstudianteNombre VARCHAR(80);

    SELECT @RutaNombre = r.Nombre FROM dbo.Ruta r WHERE r.IdRuta = @IdRuta;
    SELECT @EstudianteNombre = e.Nombres FROM dbo.Estudiante e WHERE e.IdEstudiante = @IdEstudiante;
    SET @Fragmento =
        dbo.fn_Codigo_Fragmento(@EstudianteNombre, 2, 'ES') +
        dbo.fn_Codigo_Fragmento(@RutaNombre, 4, 'RUTA');

    BEGIN TRY
        BEGIN TRAN;

        SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRutaEstudiante, 4) AS INT)), 0) + 1
        FROM dbo.RutaEstudiante
        WHERE CodigoRutaEstudiante LIKE 'RAS-%[0-9][0-9][0-9][0-9]';

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
            @CodigoGenerado, @FechaInicioVigencia, @FechaFinVigencia, UPPER(LTRIM(RTRIM(@EstadoAsignacion))),
            NULLIF(@Observaciones, ''), @Estado, SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT;
        SELECT 1 AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Recorrido_Iniciar
(
    @IdRecorrido UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdRuta UNIQUEIDENTIFIER;
    DECLARE @IdBus UNIQUEIDENTIFIER;
    DECLARE @IdConductor UNIQUEIDENTIFIER;

    SELECT
        @IdRuta = r.IdRuta,
        @IdBus = r.IdBus,
        @IdConductor = r.IdConductor
    FROM dbo.Recorrido r
    WHERE r.IdRecorrido = @IdRecorrido
      AND r.FechaEliminacion IS NULL
      AND r.EstadoRecorrido = 'PROGRAMADO';

    IF @IdRuta IS NULL
    BEGIN
        SELECT 0 AS FilasAfectadas, 'Solo se pueden iniciar recorridos PROGRAMADOS y activos.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Ruta WHERE IdRuta = @IdRuta AND (Estado = 0 OR FechaEliminacion IS NOT NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede iniciar: la ruta está inactiva.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Bus WHERE IdBus = @IdBus AND (Estado = 0 OR FechaEliminacion IS NOT NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede iniciar: el bus está inactivo.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Conductor WHERE IdConductor = @IdConductor AND (Estado = 0 OR FechaEliminacion IS NOT NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede iniciar: el conductor está inactivo.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.Recorrido r
        WHERE r.IdConductor = @IdConductor
          AND r.IdRecorrido <> @IdRecorrido
          AND r.FechaEliminacion IS NULL
          AND r.EstadoRecorrido = 'EN_CURSO')
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede iniciar: el conductor ya tiene un recorrido EN_CURSO.' AS Mensaje;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.RutaEstudiante re
        INNER JOIN dbo.Estudiante e ON e.IdEstudiante = re.IdEstudiante
        WHERE re.IdRuta = @IdRuta
          AND re.Estado = 1
          AND re.FechaEliminacion IS NULL
          AND e.Estado = 1
          AND e.FechaEliminacion IS NULL)
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede iniciar: la ruta no tiene estudiantes activos asignados.' AS Mensaje;
        RETURN;
    END

    UPDATE dbo.Recorrido
    SET EstadoRecorrido = 'EN_CURSO',
        HoraInicioReal = SYSDATETIME(),
        FechaActualizacion = SYSDATETIME()
    WHERE IdRecorrido = @IdRecorrido
      AND FechaEliminacion IS NULL
      AND EstadoRecorrido = 'PROGRAMADO';

    SELECT @@ROWCOUNT AS FilasAfectadas, CAST(NULL AS VARCHAR(200)) AS Mensaje;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Recorrido_Finalizar
(
    @IdRecorrido UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.EventoAbordaje ea
        WHERE ea.IdRecorrido = @IdRecorrido
          AND ea.FechaEliminacion IS NULL
          AND ea.TipoEvento = 'SUBIDA'
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.EventoAbordaje eb
              WHERE eb.IdRecorrido = ea.IdRecorrido
                AND eb.IdEstudiante = ea.IdEstudiante
                AND eb.TipoEvento = 'BAJADA'
                AND eb.FechaEliminacion IS NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, 'No se puede finalizar: existen estudiantes con SUBIDA sin BAJADA.' AS Mensaje;
        RETURN;
    END

    UPDATE dbo.Recorrido
    SET EstadoRecorrido = 'FINALIZADO',
        HoraFinReal = SYSDATETIME(),
        FechaActualizacion = SYSDATETIME()
    WHERE IdRecorrido = @IdRecorrido
      AND FechaEliminacion IS NULL
      AND EstadoRecorrido = 'EN_CURSO';

    SELECT @@ROWCOUNT AS FilasAfectadas, CAST(NULL AS VARCHAR(200)) AS Mensaje;
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

    SET @TipoEvento = UPPER(LTRIM(RTRIM(@TipoEvento)));

    IF @TipoEvento NOT IN ('SUBIDA', 'BAJADA', 'AUSENTE', 'NO_ABORDO')
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Tipo de evento no permitido.' AS Mensaje;
        RETURN;
    END

    DECLARE @IdRuta UNIQUEIDENTIFIER;

    SELECT @IdRuta = r.IdRuta
    FROM dbo.Recorrido r
    WHERE r.IdRecorrido = @IdRecorrido
      AND r.FechaEliminacion IS NULL
      AND r.EstadoRecorrido = 'EN_CURSO';

    IF @IdRuta IS NULL
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Solo se pueden registrar eventos en recorridos EN_CURSO.' AS Mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Estudiante e WHERE e.IdEstudiante = @IdEstudiante AND (e.Estado = 0 OR e.FechaEliminacion IS NOT NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar evento para un estudiante inactivo.' AS Mensaje;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.RutaEstudiante re
        WHERE re.IdRuta = @IdRuta
          AND re.IdEstudiante = @IdEstudiante
          AND re.Estado = 1
          AND re.FechaEliminacion IS NULL)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El estudiante no pertenece al recorrido indicado.' AS Mensaje;
        RETURN;
    END

    DECLARE @Subidas INT = 0;
    DECLARE @Bajadas INT = 0;
    DECLARE @Ausentes INT = 0;
    DECLARE @NoAbordo INT = 0;

    SELECT
        @Subidas = SUM(CASE WHEN TipoEvento = 'SUBIDA' THEN 1 ELSE 0 END),
        @Bajadas = SUM(CASE WHEN TipoEvento = 'BAJADA' THEN 1 ELSE 0 END),
        @Ausentes = SUM(CASE WHEN TipoEvento = 'AUSENTE' THEN 1 ELSE 0 END),
        @NoAbordo = SUM(CASE WHEN TipoEvento = 'NO_ABORDO' THEN 1 ELSE 0 END)
    FROM dbo.EventoAbordaje
    WHERE IdRecorrido = @IdRecorrido
      AND IdEstudiante = @IdEstudiante
      AND FechaEliminacion IS NULL;

    IF (@Ausentes > 0 OR @NoAbordo > 0)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El estudiante ya tiene un estado final en este recorrido.' AS Mensaje;
        RETURN;
    END

    IF (@Subidas > 0 AND @Bajadas > 0)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'El estudiante ya completó su ciclo de abordaje en este recorrido.' AS Mensaje;
        RETURN;
    END

    IF (@TipoEvento = 'SUBIDA' AND (@Subidas > 0 OR @Bajadas > 0 OR @Ausentes > 0 OR @NoAbordo > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar SUBIDA en el estado actual del estudiante.' AS Mensaje;
        RETURN;
    END

    IF (@TipoEvento = 'BAJADA' AND (@Subidas = 0 OR @Bajadas > 0 OR @Ausentes > 0 OR @NoAbordo > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar BAJADA sin una SUBIDA previa válida.' AS Mensaje;
        RETURN;
    END

    IF (@TipoEvento IN ('AUSENTE', 'NO_ABORDO') AND (@Subidas > 0 OR @Bajadas > 0 OR @Ausentes > 0 OR @NoAbordo > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar un estado final porque ya existen eventos previos.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT;
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoEvento, 4) AS INT)), 0) + 1
    FROM dbo.EventoAbordaje
    WHERE CodigoEvento LIKE 'EVE-%[0-9][0-9][0-9][0-9]';

    SET @CodigoGenerado = CONCAT('EVE-', RIGHT(CONCAT('0000', @Correlativo), 4));

    INSERT INTO dbo.EventoAbordaje
    (
        IdEvento, IdRecorrido, IdEstudiante, IdParadero, RegistradoPor,
        CodigoEvento, TipoEvento, FechaHora, Observacion, Estado,
        FechaCreacion, FechaActualizacion, FechaEliminacion
    )
    VALUES
    (
        NEWID(), @IdRecorrido, @IdEstudiante, @IdParadero, @RegistradoPor,
        @CodigoGenerado, @TipoEvento, @FechaHora, NULLIF(@Observacion, ''), @Estado,
        SYSUTCDATETIME(), NULL, NULL
    );

    SELECT 1 AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
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

    IF @Latitud < -90 OR @Latitud > 90 OR @Longitud < -180 OR @Longitud > 180
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Coordenadas fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Recorrido r
        WHERE r.IdRecorrido = @IdRecorrido
          AND r.EstadoRecorrido = 'EN_CURSO'
          AND r.FechaEliminacion IS NULL)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Solo se puede registrar ubicación en recorridos EN_CURSO.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT;
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoUbicacion, 4) AS INT)), 0) + 1
    FROM dbo.UbicacionBus
    WHERE CodigoUbicacion LIKE 'UBI-%[0-9][0-9][0-9][0-9]';

    SET @CodigoGenerado = CONCAT('UBI-', RIGHT(CONCAT('0000', @Correlativo), 4));

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

    SELECT 1 AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
END
GO

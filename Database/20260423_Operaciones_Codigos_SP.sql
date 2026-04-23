/*
    SPs de unificación de códigos de negocio para módulo Operaciones.
    Entidades: Recorrido, UbicacionBus, EventoAbordaje, Notificacion,
               Incidencia, Auditoria, IA_Consulta, IA_Prediccion.
*/
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
    DECLARE @RutaNombre VARCHAR(120);
    DECLARE @Turno VARCHAR(20);
    DECLARE @Fragmento VARCHAR(4);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @RutaNombre = r.Nombre, @Turno = r.Turno
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SET @Fragmento =
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@RutaNombre, 'RU'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Turno, 'MA'), ' ', ''), '-', ''), 2));

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
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
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

    SELECT @CodigoRecorrido = r.CodigoRecorrido
    FROM dbo.Recorrido r
    WHERE r.IdRecorrido = @IdRecorrido;

    SET @Fragmento = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@CodigoRecorrido, 'RECU'), 'REC-', ''), '-', ''), 4));

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
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
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
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@TipoEvento, 'EV'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Nombre, 'NO'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Apellido, 'AP'), ' ', ''), '-', ''), 2));

    BEGIN TRANSACTION;
    BEGIN TRY
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
            @CodigoGenerado, @TipoEvento, @FechaHora, NULLIF(@Observacion, ''), @Estado,
            SYSUTCDATETIME(), NULL, NULL
        );

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
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
    DECLARE @Fragmento VARCHAR(6) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@TipoNotificacion, 'NOTIFI'), ' ', ''), '-', ''), 6));
    DECLARE @CodigoGenerado VARCHAR(20);

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

        SET @CodigoGenerado = CONCAT('NOT-', RIGHT(CONCAT(@Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @Correlativo), 4));

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
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
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
    DECLARE @Fragmento VARCHAR(6) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@TipoIncidencia, 'INCIDE'), ' ', ''), '-', ''), 6));
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @IdGenerado UNIQUEIDENTIFIER = NEWID();

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

        SET @CodigoGenerado = CONCAT('INC-', RIGHT(CONCAT(@Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @Correlativo), 4));

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
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, @IdGenerado AS IdIncidencia;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auditoria_Registrar
    @Tabla VARCHAR(100),
    @IdRegistro UNIQUEIDENTIFIER = NULL,
    @Accion VARCHAR(30),
    @DatosAntes VARCHAR(MAX) = NULL,
    @DatosDespues VARCHAR(MAX) = NULL,
    @UsuarioId UNIQUEIDENTIFIER = NULL,
    @NombreUsuario VARCHAR(120) = NULL,
    @Ip VARCHAR(50) = NULL,
    @UserAgent VARCHAR(500) = NULL,
    @Modulo VARCHAR(60) = NULL,
    @Observacion VARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(3) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Tabla, 'AUD'), ' ', ''), '-', ''), 3));
    DECLARE @CodigoGenerado VARCHAR(20);

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'AUD')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('AUD', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'AUD';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'AUD';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoAuditoria, 4) AS INT)), 0) + 1
            FROM dbo.Auditoria
            WHERE CodigoAuditoria LIKE 'AUD-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('AUD-', RIGHT(CONCAT(@Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Auditoria
        (
            IdAuditoria, CodigoAuditoria, Tabla, IdRegistro, Accion, DatosAntes, DatosDespues,
            UsuarioId, NombreUsuario, Ip, UserAgent, Modulo, Observacion, Fecha
        )
        VALUES
        (
            NEWID(), @CodigoGenerado, @Tabla, @IdRegistro, @Accion, @DatosAntes, @DatosDespues,
            @UsuarioId, @NombreUsuario, @Ip, @UserAgent, @Modulo, @Observacion, SYSUTCDATETIME()
        );

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_IA_Consulta_Registrar
    @UsuarioId UNIQUEIDENTIFIER = NULL,
    @IdRecorrido UNIQUEIDENTIFIER = NULL,
    @IdEstudiante UNIQUEIDENTIFIER = NULL,
    @IdIncidencia UNIQUEIDENTIFIER = NULL,
    @Modulo VARCHAR(60),
    @TipoConsulta VARCHAR(60),
    @PromptEnviado VARCHAR(MAX),
    @ContextoEnviado VARCHAR(MAX) = NULL,
    @RespuestaIA VARCHAR(MAX) = NULL,
    @TokensEntrada INT = NULL,
    @TokensSalida INT = NULL,
    @CostoEstimado DECIMAL(10,4) = NULL,
    @EstadoConsulta VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(3) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@TipoConsulta, 'CON'), ' ', ''), '-', ''), 3));
    DECLARE @CodigoGenerado VARCHAR(20);

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'IAC')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('IAC', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'IAC';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'IAC';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoConsulta, 4) AS INT)), 0) + 1
            FROM dbo.IA_Consulta
            WHERE CodigoConsulta LIKE 'IAC-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('IAC-', RIGHT(CONCAT(@Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.IA_Consulta
        (
            IdConsulta, UsuarioId, IdRecorrido, IdEstudiante, IdIncidencia,
            CodigoConsulta, Modulo, TipoConsulta, PromptEnviado, ContextoEnviado,
            RespuestaIA, TokensEntrada, TokensSalida, CostoEstimado, EstadoConsulta, FechaConsulta
        )
        VALUES
        (
            NEWID(), @UsuarioId, @IdRecorrido, @IdEstudiante, @IdIncidencia,
            @CodigoGenerado, @Modulo, @TipoConsulta, @PromptEnviado, @ContextoEnviado,
            @RespuestaIA, @TokensEntrada, @TokensSalida, @CostoEstimado, @EstadoConsulta, SYSUTCDATETIME()
        );

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_IA_Prediccion_Registrar
    @IdRecorrido UNIQUEIDENTIFIER = NULL,
    @IdEstudiante UNIQUEIDENTIFIER = NULL,
    @TipoPrediccion VARCHAR(60),
    @ValorPredicho DECIMAL(18,6) = NULL,
    @Confianza DECIMAL(5,2) = NULL,
    @DatosEntrada VARCHAR(MAX) = NULL,
    @Explicacion VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Correlativo INT;
    DECLARE @Fragmento VARCHAR(3) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@TipoPrediccion, 'PRE'), ' ', ''), '-', ''), 3));
    DECLARE @CodigoGenerado VARCHAR(20);

    BEGIN TRANSACTION;
    BEGIN TRY
        IF OBJECT_ID('dbo.CorrelativoDocumento', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK) WHERE TipoCodigo = 'IAP')
                INSERT INTO dbo.CorrelativoDocumento (TipoCodigo, UltimoNumero) VALUES ('IAP', 0);

            UPDATE dbo.CorrelativoDocumento
               SET UltimoNumero = UltimoNumero + 1
             WHERE TipoCodigo = 'IAP';

            SELECT @Correlativo = UltimoNumero
            FROM dbo.CorrelativoDocumento WITH (UPDLOCK, HOLDLOCK)
            WHERE TipoCodigo = 'IAP';
        END
        ELSE
        BEGIN
            SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoPrediccion, 4) AS INT)), 0) + 1
            FROM dbo.IA_Prediccion
            WHERE CodigoPrediccion LIKE 'IAP-%[0-9][0-9][0-9][0-9]';
        END

        SET @CodigoGenerado = CONCAT('IAP-', RIGHT(CONCAT(@Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.IA_Prediccion
        (
            IdPrediccion, IdRecorrido, IdEstudiante, CodigoPrediccion, TipoPrediccion,
            ValorPredicho, Confianza, DatosEntrada, Explicacion, FechaGeneracion
        )
        VALUES
        (
            NEWID(), @IdRecorrido, @IdEstudiante, @CodigoGenerado, @TipoPrediccion,
            @ValorPredicho, @Confianza, @DatosEntrada, @Explicacion, SYSUTCDATETIME()
        );

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*
    Script consolidado - CapiMovil
    Fecha: 2026-04-27
    Incluye:
      1) Normalización de código de auditoría (AUD-YYYY-000001)
      2) Detalle de notificación con código de estudiante
      3) Módulo de calificación de conductor (tabla + SP)
*/

SET NOCOUNT ON;
GO

/* ============================================================
   1) AUDITORÍA - código consistente
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_Auditoria_Registrar
    @Tabla NVARCHAR(100),
    @IdRegistro UNIQUEIDENTIFIER = NULL,
    @Accion NVARCHAR(20),
    @DatosAntes NVARCHAR(MAX) = NULL,
    @DatosDespues NVARCHAR(MAX) = NULL,
    @UsuarioId UNIQUEIDENTIFIER = NULL,
    @NombreUsuario NVARCHAR(120) = NULL,
    @Ip NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(300) = NULL,
    @Modulo NVARCHAR(100) = NULL,
    @Observacion NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @IdAuditoria UNIQUEIDENTIFIER = NEWID();
        DECLARE @Correlativo INT;
        DECLARE @CodigoAuditoria NVARCHAR(25);
        DECLARE @Fecha DATE = CAST(GETDATE() AS DATE);
        DECLARE @Anio CHAR(4) = CONVERT(CHAR(4), YEAR(@Fecha));

        SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoAuditoria, 6) AS INT)), 0) + 1
        FROM dbo.Auditoria
        WHERE CodigoAuditoria LIKE CONCAT('AUD-', @Anio, '-%');

        SET @CodigoAuditoria = CONCAT('AUD-', @Anio, '-', RIGHT(CONCAT('000000', @Correlativo), 6));

        INSERT INTO dbo.Auditoria
        (
            IdAuditoria, CodigoAuditoria, Tabla, IdRegistro, Accion,
            DatosAntes, DatosDespues, UsuarioId, NombreUsuario, Ip,
            UserAgent, Modulo, Observacion, Fecha, Estado
        )
        VALUES
        (
            @IdAuditoria, @CodigoAuditoria, @Tabla, @IdRegistro, @Accion,
            @DatosAntes, @DatosDespues, @UsuarioId, @NombreUsuario, @Ip,
            @UserAgent, @Modulo, @Observacion, GETDATE(), 1
        );

        SELECT
            CAST(1 AS BIT) AS Exito,
            1 AS FilasAfectadas,
            @CodigoAuditoria AS CodigoGenerado,
            CAST(NULL AS NVARCHAR(400)) AS Mensaje;
    END TRY
    BEGIN CATCH
        SELECT
            CAST(0 AS BIT) AS Exito,
            0 AS FilasAfectadas,
            CAST('' AS NVARCHAR(30)) AS CodigoGenerado,
            ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

/* ============================================================
   4) EVENTO ABORDAJE - fecha/hora automática en servidor
   ============================================================ */
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
    SET @FechaHora = GETDATE();

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

    DECLARE @Subidas INT = 0, @Bajadas INT = 0, @Ausentes INT = 0, @NoAbordo INT = 0;
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

    IF (@TipoEvento = 'SUBIDA' AND (@Subidas > 0 OR @Bajadas > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar SUBIDA porque ya existe un evento de abordaje previo.' AS Mensaje;
        RETURN;
    END

    IF (@TipoEvento = 'BAJADA' AND (@Subidas = 0 OR @Bajadas > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar BAJADA sin una SUBIDA previa válida.' AS Mensaje;
        RETURN;
    END

    IF (@TipoEvento IN ('AUSENTE', 'NO_ABORDO') AND (@Subidas > 0 OR @Bajadas > 0 OR @Ausentes > 0 OR @NoAbordo > 0))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'No se puede registrar estado final porque ya existen eventos previos en el recorrido.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT, @CodigoGenerado VARCHAR(20);
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
        GETDATE(), NULL, NULL
    );

    SELECT 1 AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
END
GO

/* ============================================================
   5) INCIDENCIA - fecha/hora automática en servidor
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_Incidencia_Registrar
(
    @IdRecorrido UNIQUEIDENTIFIER,
    @IdConductor UNIQUEIDENTIFIER,
    @ReportadoPor UNIQUEIDENTIFIER = NULL,
    @TipoIncidencia VARCHAR(50),
    @Descripcion VARCHAR(300),
    @FechaHora DATETIME,
    @EstadoIncidencia VARCHAR(20),
    @Prioridad VARCHAR(20),
    @Solucion VARCHAR(300) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @FechaHora = GETDATE();

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Recorrido r
        WHERE r.IdRecorrido = @IdRecorrido
          AND r.IdConductor = @IdConductor
          AND r.EstadoRecorrido = 'EN_CURSO'
          AND r.FechaEliminacion IS NULL)
    BEGIN
        SELECT
            0 AS FilasAfectadas,
            CAST(NULL AS UNIQUEIDENTIFIER) AS IdIncidencia,
            CAST(NULL AS VARCHAR(20)) AS CodigoGenerado,
            'Solo se pueden registrar incidencias en recorridos EN_CURSO del conductor autenticado.' AS Mensaje;
        RETURN;
    END

    DECLARE @IdIncidencia UNIQUEIDENTIFIER = NEWID();
    DECLARE @Correlativo INT;
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @Fragmento VARCHAR(6) = dbo.fn_Codigo_Fragmento(@TipoIncidencia, 6, 'INCIDE');

    BEGIN TRY
        BEGIN TRAN;

        SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoIncidencia, 4) AS INT)), 0) + 1
        FROM dbo.Incidencia
        WHERE CodigoIncidencia LIKE 'INC-%[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]';

        SET @CodigoGenerado = CONCAT('INC-', @Fragmento, RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Incidencia
        (
            IdIncidencia, IdRecorrido, IdConductor, ReportadoPor, CodigoIncidencia,
            TipoIncidencia, Descripcion, FechaHora, EstadoIncidencia, Prioridad,
            FechaCierre, Solucion, Estado, FechaCreacion, FechaActualizacion, FechaEliminacion
        )
        VALUES
        (
            @IdIncidencia, @IdRecorrido, @IdConductor, @ReportadoPor, @CodigoGenerado,
            UPPER(LTRIM(RTRIM(@TipoIncidencia))), LTRIM(RTRIM(@Descripcion)), @FechaHora,
            UPPER(LTRIM(RTRIM(@EstadoIncidencia))), UPPER(LTRIM(RTRIM(@Prioridad))),
            NULL, NULLIF(LTRIM(RTRIM(@Solucion)), ''), 1, GETDATE(), NULL, NULL
        );

        COMMIT;

        SELECT
            1 AS FilasAfectadas,
            @IdIncidencia AS IdIncidencia,
            @CodigoGenerado AS CodigoGenerado,
            CAST(NULL AS VARCHAR(200)) AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;

        SELECT
            0 AS FilasAfectadas,
            CAST(NULL AS UNIQUEIDENTIFIER) AS IdIncidencia,
            CAST(NULL AS VARCHAR(20)) AS CodigoGenerado,
            ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

/* ============================================================
   2) NOTIFICACIONES - incluir código de estudiante
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_Notificacion_ObtenerPorId
    @IdNotificacion UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.*,
        CONCAT(p.Nombres, ' ', p.ApellidoPaterno, ' ', ISNULL(p.ApellidoMaterno, '')) AS NombrePadre,
        CONCAT(e.Nombres, ' ', e.ApellidoPaterno, ' ', ISNULL(e.ApellidoMaterno, '')) AS NombreEstudiante,
        e.CodigoEstudiante
    FROM dbo.Notificacion n
    INNER JOIN dbo.PadreFamilia p ON p.IdPadre = n.IdPadre
    LEFT JOIN dbo.Estudiante e ON e.IdEstudiante = n.IdEstudiante
    WHERE n.IdNotificacion = @IdNotificacion
      AND n.Estado = 1;
END
GO

/* ============================================================
   3) CALIFICACIÓN DE CONDUCTOR (Padre)
   ============================================================ */
IF OBJECT_ID('dbo.CalificacionConductor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CalificacionConductor
    (
        IdCalificacion UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdPadre UNIQUEIDENTIFIER NOT NULL,
        IdConductor UNIQUEIDENTIFIER NOT NULL,
        IdEstudiante UNIQUEIDENTIFIER NULL,
        Puntaje INT NOT NULL,
        Comentario NVARCHAR(250) NULL,
        FechaRegistro DATETIME NOT NULL CONSTRAINT DF_CalificacionConductor_FechaRegistro DEFAULT(GETDATE()),
        Estado BIT NOT NULL CONSTRAINT DF_CalificacionConductor_Estado DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CalificacionConductor_Padre')
BEGIN
    ALTER TABLE dbo.CalificacionConductor
    ADD CONSTRAINT FK_CalificacionConductor_Padre FOREIGN KEY (IdPadre) REFERENCES dbo.PadreFamilia(IdPadre);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CalificacionConductor_Conductor')
BEGIN
    ALTER TABLE dbo.CalificacionConductor
    ADD CONSTRAINT FK_CalificacionConductor_Conductor FOREIGN KEY (IdConductor) REFERENCES dbo.Conductor(IdConductor);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CalificacionConductor_Estudiante')
BEGIN
    ALTER TABLE dbo.CalificacionConductor
    ADD CONSTRAINT FK_CalificacionConductor_Estudiante FOREIGN KEY (IdEstudiante) REFERENCES dbo.Estudiante(IdEstudiante);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CalificacionConductor_Registrar
    @IdPadre UNIQUEIDENTIFIER,
    @IdConductor UNIQUEIDENTIFIER,
    @IdEstudiante UNIQUEIDENTIFIER = NULL,
    @Puntaje INT,
    @Comentario NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Puntaje < 1 OR @Puntaje > 5
    BEGIN
        SELECT 0;
        RETURN;
    END

    INSERT INTO dbo.CalificacionConductor
    (
        IdCalificacion, IdPadre, IdConductor, IdEstudiante, Puntaje, Comentario, FechaRegistro, Estado
    )
    VALUES
    (
        NEWID(), @IdPadre, @IdConductor, @IdEstudiante, @Puntaje, NULLIF(LTRIM(RTRIM(@Comentario)), ''), GETDATE(), 1
    );

    SELECT 1;
END
GO

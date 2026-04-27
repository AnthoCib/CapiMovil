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

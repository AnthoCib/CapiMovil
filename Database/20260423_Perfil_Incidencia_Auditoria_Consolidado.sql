/*
    CapiMovil - Consolidado Incidencia + Auditoría Recorrido + Perfil
    Incluye:
    1) Normalización de fragmento de código
    2) Corrección de sp_Incidencia_Registrar
    3) Refuerzo de sp_Auditoria_Registrar
    4) Soporte de foto de perfil en Usuario
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================
   SECCIÓN A: SOPORTE PERFIL
   ============================ */
IF COL_LENGTH('dbo.Usuario', 'FotoPerfilUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario ADD FotoPerfilUrl VARCHAR(260) NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ActualizarFotoPerfil
    @IdUsuario UNIQUEIDENTIFIER,
    @FotoPerfilUrl VARCHAR(260) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Usuario
       SET FotoPerfilUrl = NULLIF(LTRIM(RTRIM(@FotoPerfilUrl)), ''),
           FechaActualizacion = GETDATE()
     WHERE IdUsuario = @IdUsuario
       AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

/* ============================
   SECCIÓN B: HELPERS DE CÓDIGO
   ============================ */
CREATE OR ALTER FUNCTION dbo.fn_Codigo_SoloAlfanumerico (@Texto NVARCHAR(200))
RETURNS VARCHAR(200)
AS
BEGIN
    DECLARE @Entrada NVARCHAR(200) = UPPER(ISNULL(@Texto, ''));

    SET @Entrada = REPLACE(@Entrada, N'Á', N'A');
    SET @Entrada = REPLACE(@Entrada, N'É', N'E');
    SET @Entrada = REPLACE(@Entrada, N'Í', N'I');
    SET @Entrada = REPLACE(@Entrada, N'Ó', N'O');
    SET @Entrada = REPLACE(@Entrada, N'Ú', N'U');
    SET @Entrada = REPLACE(@Entrada, N'Ñ', N'N');

    DECLARE @I INT = 1;
    DECLARE @Salida VARCHAR(200) = '';
    DECLARE @Ch NCHAR(1);

    WHILE @I <= LEN(@Entrada)
    BEGIN
        SET @Ch = SUBSTRING(@Entrada, @I, 1);
        IF @Ch LIKE '[A-Z0-9]'
            SET @Salida += CONVERT(VARCHAR(1), @Ch);
        SET @I += 1;
    END

    RETURN @Salida;
END
GO

CREATE OR ALTER FUNCTION dbo.fn_Codigo_Fragmento
(
    @Texto NVARCHAR(200),
    @Longitud INT,
    @Fallback VARCHAR(20)
)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @Limpio VARCHAR(200) = dbo.fn_Codigo_SoloAlfanumerico(@Texto);

    IF @Longitud IS NULL OR @Longitud <= 0
        SET @Longitud = 6;

    IF LEN(@Limpio) = 0
        SET @Limpio = dbo.fn_Codigo_SoloAlfanumerico(@Fallback);

    IF LEN(@Limpio) = 0
        SET @Limpio = 'CAPI';

    RETURN LEFT(@Limpio + REPLICATE('X', @Longitud), @Longitud);
END
GO

/* =============================================
   SECCIÓN C: INCIDENCIA (CÓDIGO CONSISTENTE)
   ============================================= */
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

    DECLARE @IdIncidencia UNIQUEIDENTIFIER = NEWID();
    DECLARE @Correlativo INT = 0;
    DECLARE @CodigoGenerado VARCHAR(20);
    DECLARE @Fragmento VARCHAR(6) = dbo.fn_Codigo_Fragmento(@TipoIncidencia, 6, 'INCIDE');

    BEGIN TRY
        BEGIN TRAN;

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
            WHERE CodigoIncidencia LIKE 'INC-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]';
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
            @IdIncidencia, @IdRecorrido, @IdConductor, @ReportadoPor, @CodigoGenerado,
            UPPER(LTRIM(RTRIM(@TipoIncidencia))), LTRIM(RTRIM(@Descripcion)), @FechaHora, UPPER(LTRIM(RTRIM(@EstadoIncidencia))), UPPER(LTRIM(RTRIM(@Prioridad))),
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

/* =============================================
   SECCIÓN D: AUDITORÍA (CÓDIGO + DATOS LIMPIOS)
   ============================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Auditoria_Registrar
(
    @Tabla VARCHAR(100),
    @IdRegistro UNIQUEIDENTIFIER = NULL,
    @Accion VARCHAR(20),
    @DatosAntes NVARCHAR(MAX) = NULL,
    @DatosDespues NVARCHAR(MAX) = NULL,
    @UsuarioId UNIQUEIDENTIFIER = NULL,
    @NombreUsuario VARCHAR(120) = NULL,
    @Ip VARCHAR(50) = NULL,
    @UserAgent VARCHAR(300) = NULL,
    @Modulo VARCHAR(100) = NULL,
    @Observacion VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdAuditoria UNIQUEIDENTIFIER = NEWID();
    DECLARE @Correlativo INT = 0;
    DECLARE @CodigoGenerado VARCHAR(20);

    BEGIN TRY
        BEGIN TRAN;

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

        SET @CodigoGenerado = CONCAT('AUD-', RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Auditoria
        (
            IdAuditoria, CodigoAuditoria, Tabla, IdRegistro, Accion,
            DatosAntes, DatosDespues, UsuarioId, NombreUsuario, Ip,
            UserAgent, Modulo, Observacion, Fecha
        )
        VALUES
        (
            @IdAuditoria,
            @CodigoGenerado,
            LEFT(LTRIM(RTRIM(@Tabla)), 100),
            @IdRegistro,
            LEFT(UPPER(LTRIM(RTRIM(@Accion))), 20),
            @DatosAntes,
            @DatosDespues,
            @UsuarioId,
            NULLIF(LEFT(LTRIM(RTRIM(ISNULL(@NombreUsuario, ''))), 120), ''),
            NULLIF(LEFT(LTRIM(RTRIM(ISNULL(@Ip, ''))), 50), ''),
            NULLIF(LEFT(LTRIM(RTRIM(ISNULL(@UserAgent, ''))), 300), ''),
            NULLIF(LEFT(LTRIM(RTRIM(ISNULL(@Modulo, ''))), 100), ''),
            NULLIF(LEFT(LTRIM(RTRIM(ISNULL(@Observacion, ''))), 250), ''),
            GETDATE()
        );

        COMMIT;

        SELECT
            1 AS FilasAfectadas,
            @IdAuditoria AS IdAuditoria,
            @CodigoGenerado AS CodigoGenerado,
            CAST(NULL AS VARCHAR(200)) AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;

        SELECT
            0 AS FilasAfectadas,
            CAST(NULL AS UNIQUEIDENTIFIER) AS IdAuditoria,
            CAST(NULL AS VARCHAR(20)) AS CodigoGenerado,
            ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

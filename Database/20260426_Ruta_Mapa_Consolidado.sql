/*
    CapiMovil - Consolidado RUTA (Mapa Inicio/Fin)
    Incluye:
    1) ALTER TABLE Ruta para coordenadas/direcciones de inicio y fin
    2) CREATE OR ALTER SPs de Ruta para listar/obtener/registrar/actualizar
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   1) ESTRUCTURA TABLA RUTA
   ========================================================= */
IF COL_LENGTH('dbo.Ruta', 'LatitudInicio') IS NULL
    ALTER TABLE dbo.Ruta ADD LatitudInicio DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Ruta', 'LongitudInicio') IS NULL
    ALTER TABLE dbo.Ruta ADD LongitudInicio DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Ruta', 'DireccionInicio') IS NULL
    ALTER TABLE dbo.Ruta ADD DireccionInicio VARCHAR(250) NULL;
GO
IF COL_LENGTH('dbo.Ruta', 'LatitudFin') IS NULL
    ALTER TABLE dbo.Ruta ADD LatitudFin DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Ruta', 'LongitudFin') IS NULL
    ALTER TABLE dbo.Ruta ADD LongitudFin DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Ruta', 'DireccionFin') IS NULL
    ALTER TABLE dbo.Ruta ADD DireccionFin VARCHAR(250) NULL;
GO

/* =========================================================
   2) SP_RUTA_LISTAR
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Ruta_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdRuta,
        CodigoRuta,
        Nombre,
        Descripcion,
        Turno,
        HoraInicio,
        HoraFin,
        PuntoInicio,
        PuntoFin,
        LatitudInicio,
        LongitudInicio,
        DireccionInicio,
        LatitudFin,
        LongitudFin,
        DireccionFin,
        EstadoRuta,
        Estado,
        FechaCreacion,
        FechaActualizacion,
        FechaEliminacion
    FROM dbo.Ruta
    WHERE FechaEliminacion IS NULL
    ORDER BY Nombre;
END
GO

/* =========================================================
   3) SP_RUTA_OBTENERPORID
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Ruta_ObtenerPorId
(
    @IdRuta UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdRuta,
        CodigoRuta,
        Nombre,
        Descripcion,
        Turno,
        HoraInicio,
        HoraFin,
        PuntoInicio,
        PuntoFin,
        LatitudInicio,
        LongitudInicio,
        DireccionInicio,
        LatitudFin,
        LongitudFin,
        DireccionFin,
        EstadoRuta,
        Estado,
        FechaCreacion,
        FechaActualizacion,
        FechaEliminacion
    FROM dbo.Ruta
    WHERE IdRuta = @IdRuta
      AND FechaEliminacion IS NULL;
END
GO

/* =========================================================
   4) SP_RUTA_REGISTRAR
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Ruta_Registrar
(
    @Nombre VARCHAR(120),
    @Descripcion VARCHAR(300) = NULL,
    @Turno VARCHAR(20),
    @HoraInicio TIME,
    @HoraFin TIME,
    @PuntoInicio VARCHAR(200) = NULL,
    @PuntoFin VARCHAR(200) = NULL,
    @LatitudInicio DECIMAL(10,7) = NULL,
    @LongitudInicio DECIMAL(10,7) = NULL,
    @DireccionInicio VARCHAR(250) = NULL,
    @LatitudFin DECIMAL(10,7) = NULL,
    @LongitudFin DECIMAL(10,7) = NULL,
    @DireccionFin VARCHAR(250) = NULL,
    @EstadoRuta VARCHAR(20),
    @Estado BIT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @HoraInicio >= @HoraFin
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'La hora de inicio debe ser menor a la hora de fin.' AS Mensaje;
        RETURN;
    END

    IF @LatitudInicio IS NOT NULL AND (@LatitudInicio < -90 OR @LatitudInicio > 90)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Latitud de inicio fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LongitudInicio IS NOT NULL AND (@LongitudInicio < -180 OR @LongitudInicio > 180)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Longitud de inicio fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LatitudFin IS NOT NULL AND (@LatitudFin < -90 OR @LatitudFin > 90)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Latitud de fin fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LongitudFin IS NOT NULL AND (@LongitudFin < -180 OR @LongitudFin > 180)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Longitud de fin fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF ((@LatitudInicio IS NULL AND @LongitudInicio IS NOT NULL) OR (@LatitudInicio IS NOT NULL AND @LongitudInicio IS NULL)
        OR (@LatitudFin IS NULL AND @LongitudFin IS NOT NULL) OR (@LatitudFin IS NOT NULL AND @LongitudFin IS NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Debe registrar latitud y longitud completas por cada punto.' AS Mensaje;
        RETURN;
    END

    IF ((@LatitudInicio IS NULL AND @LatitudFin IS NOT NULL) OR (@LatitudInicio IS NOT NULL AND @LatitudFin IS NULL))
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Debe definir inicio y fin de la ruta.' AS Mensaje;
        RETURN;
    END

    DECLARE @Correlativo INT;
    DECLARE @CodigoGenerado VARCHAR(20);

    BEGIN TRY
        BEGIN TRAN;

        SELECT @Correlativo = ISNULL(MAX(TRY_CAST(RIGHT(CodigoRuta, 4) AS INT)), 0) + 1
        FROM dbo.Ruta
        WHERE CodigoRuta LIKE 'RUT-%[0-9][0-9][0-9][0-9]';

        SET @CodigoGenerado = CONCAT('RUT-', RIGHT(CONCAT('0000', @Correlativo), 4));

        INSERT INTO dbo.Ruta
        (
            IdRuta,
            CodigoRuta,
            Nombre,
            Descripcion,
            Turno,
            HoraInicio,
            HoraFin,
            PuntoInicio,
            PuntoFin,
            LatitudInicio,
            LongitudInicio,
            DireccionInicio,
            LatitudFin,
            LongitudFin,
            DireccionFin,
            EstadoRuta,
            Estado,
            FechaCreacion,
            FechaActualizacion,
            FechaEliminacion
        )
        VALUES
        (
            NEWID(),
            @CodigoGenerado,
            @Nombre,
            NULLIF(LTRIM(RTRIM(@Descripcion)), ''),
            UPPER(LTRIM(RTRIM(@Turno))),
            @HoraInicio,
            @HoraFin,
            NULLIF(LTRIM(RTRIM(@PuntoInicio)), ''),
            NULLIF(LTRIM(RTRIM(@PuntoFin)), ''),
            @LatitudInicio,
            @LongitudInicio,
            NULLIF(LTRIM(RTRIM(@DireccionInicio)), ''),
            @LatitudFin,
            @LongitudFin,
            NULLIF(LTRIM(RTRIM(@DireccionFin)), ''),
            UPPER(LTRIM(RTRIM(@EstadoRuta))),
            @Estado,
            SYSUTCDATETIME(),
            NULL,
            NULL
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

/* =========================================================
   5) SP_RUTA_ACTUALIZAR
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_Ruta_Actualizar
(
    @IdRuta UNIQUEIDENTIFIER,
    @Nombre VARCHAR(120),
    @Descripcion VARCHAR(300) = NULL,
    @Turno VARCHAR(20),
    @HoraInicio TIME,
    @HoraFin TIME,
    @PuntoInicio VARCHAR(200) = NULL,
    @PuntoFin VARCHAR(200) = NULL,
    @LatitudInicio DECIMAL(10,7) = NULL,
    @LongitudInicio DECIMAL(10,7) = NULL,
    @DireccionInicio VARCHAR(250) = NULL,
    @LatitudFin DECIMAL(10,7) = NULL,
    @LongitudFin DECIMAL(10,7) = NULL,
    @DireccionFin VARCHAR(250) = NULL,
    @EstadoRuta VARCHAR(20),
    @Estado BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @HoraInicio >= @HoraFin
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'La hora de inicio debe ser menor a la hora de fin.' AS Mensaje;
        RETURN;
    END

    IF @LatitudInicio IS NOT NULL AND (@LatitudInicio < -90 OR @LatitudInicio > 90)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Latitud de inicio fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LongitudInicio IS NOT NULL AND (@LongitudInicio < -180 OR @LongitudInicio > 180)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Longitud de inicio fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LatitudFin IS NOT NULL AND (@LatitudFin < -90 OR @LatitudFin > 90)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Latitud de fin fuera de rango.' AS Mensaje;
        RETURN;
    END

    IF @LongitudFin IS NOT NULL AND (@LongitudFin < -180 OR @LongitudFin > 180)
    BEGIN
        SELECT 0 AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, 'Longitud de fin fuera de rango.' AS Mensaje;
        RETURN;
    END

    UPDATE dbo.Ruta
    SET
        Nombre = @Nombre,
        Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), ''),
        Turno = UPPER(LTRIM(RTRIM(@Turno))),
        HoraInicio = @HoraInicio,
        HoraFin = @HoraFin,
        PuntoInicio = NULLIF(LTRIM(RTRIM(@PuntoInicio)), ''),
        PuntoFin = NULLIF(LTRIM(RTRIM(@PuntoFin)), ''),
        LatitudInicio = @LatitudInicio,
        LongitudInicio = @LongitudInicio,
        DireccionInicio = NULLIF(LTRIM(RTRIM(@DireccionInicio)), ''),
        LatitudFin = @LatitudFin,
        LongitudFin = @LongitudFin,
        DireccionFin = NULLIF(LTRIM(RTRIM(@DireccionFin)), ''),
        EstadoRuta = UPPER(LTRIM(RTRIM(@EstadoRuta))),
        Estado = @Estado,
        FechaActualizacion = SYSUTCDATETIME()
    WHERE IdRuta = @IdRuta
      AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas, CAST(NULL AS VARCHAR(20)) AS CodigoGenerado, CAST(NULL AS VARCHAR(200)) AS Mensaje;
END
GO

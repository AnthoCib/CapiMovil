/*
    SPs de unificación de códigos de negocio para módulo transporte.
    Entidades: Bus, Ruta, Paradero, RutaEstudiante.
    Fecha: 2026-04-23
*/
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
    DECLARE @PlacaFrag VARCHAR(3) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Placa, 'XXX'), '-', ''), ' ', ''), 3));
    DECLARE @MarcaFrag VARCHAR(3) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Marca, 'XXX'), '-', ''), ' ', ''), 3));

    SET @Fragmento =
        RIGHT(CONCAT(@PlacaFrag, 'XXX'), 3) +
        RIGHT(CONCAT(@MarcaFrag, 'XXX'), 3);

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

        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

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
    DECLARE @Fragmento VARCHAR(6) = UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Nombre, 'RUTAXX'), ' ', ''), '-', ''), 6));
    DECLARE @CodigoGenerado VARCHAR(20);

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

        SET @CodigoGenerado = CONCAT('RUT-', RIGHT(CONCAT(@Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @Correlativo), 4));

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

        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
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
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@RutaNombre, 'RU'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@Nombre, 'PARA'), ' ', ''), '-', ''), 4));

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

        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
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
    DECLARE @EstudianteApellido VARCHAR(60);
    DECLARE @Fragmento VARCHAR(6);
    DECLARE @CodigoGenerado VARCHAR(20);

    SELECT @RutaNombre = r.Nombre
    FROM dbo.Ruta r
    WHERE r.IdRuta = @IdRuta;

    SELECT
        @EstudianteNombre = e.Nombres,
        @EstudianteApellido = e.ApellidoPaterno
    FROM dbo.Estudiante e
    WHERE e.IdEstudiante = @IdEstudiante;

    SET @Fragmento =
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@RutaNombre, 'RU'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@EstudianteNombre, 'NO'), ' ', ''), '-', ''), 2)) +
        UPPER(LEFT(REPLACE(REPLACE(ISNULL(@EstudianteApellido, 'AP'), ' ', ''), '-', ''), 2));

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

        SELECT @@ROWCOUNT AS FilasAfectadas, @CodigoGenerado AS CodigoGenerado;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*
    Script de procedimientos almacenados para módulos PadreFamilia y Conductor.
    Fecha: 2026-04-23
    Convención: sp_<Entidad>_<Accion>
*/

/* =========================================================
   PADRE FAMILIA
   ========================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.IdPadre,
        p.IdUsuario,
        p.CodigoPadre,
        p.Nombres,
        p.ApellidoPaterno,
        p.ApellidoMaterno,
        p.DNI,
        p.Telefono,
        p.TelefonoAlterno,
        p.Direccion,
        p.CorreoContacto,
        p.Estado,
        p.FechaCreacion,
        p.FechaActualizacion,
        p.FechaEliminacion
    FROM dbo.PadreFamilia p
    WHERE p.FechaEliminacion IS NULL
    ORDER BY p.FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_ListarPorId
    @IdPadre UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.IdPadre,
        p.IdUsuario,
        p.CodigoPadre,
        p.Nombres,
        p.ApellidoPaterno,
        p.ApellidoMaterno,
        p.DNI,
        p.Telefono,
        p.TelefonoAlterno,
        p.Direccion,
        p.CorreoContacto,
        p.Estado,
        p.FechaCreacion,
        p.FechaActualizacion,
        p.FechaEliminacion
    FROM dbo.PadreFamilia p
    WHERE p.IdPadre = @IdPadre
      AND p.FechaEliminacion IS NULL;
END
GO

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

    DECLARE @Correlativo INT;

    SELECT @Correlativo = ISNULL(MAX(TRY_CONVERT(INT, RIGHT(CodigoPadre, 6))), 0) + 1
    FROM dbo.PadreFamilia;

    SET @CodigoGenerado = CONCAT('PAD-', RIGHT(CONCAT('000000', @Correlativo), 6));

    INSERT INTO dbo.PadreFamilia
    (
        IdPadre,
        IdUsuario,
        CodigoPadre,
        Nombres,
        ApellidoPaterno,
        ApellidoMaterno,
        DNI,
        Telefono,
        TelefonoAlterno,
        Direccion,
        CorreoContacto,
        Estado,
        FechaCreacion,
        FechaActualizacion,
        FechaEliminacion
    )
    VALUES
    (
        NEWID(),
        @IdUsuario,
        @CodigoGenerado,
        @Nombres,
        @ApellidoPaterno,
        @ApellidoMaterno,
        NULLIF(@DNI, ''),
        NULLIF(@Telefono, ''),
        NULLIF(@TelefonoAlterno, ''),
        NULLIF(@Direccion, ''),
        NULLIF(@CorreoContacto, ''),
        @Estado,
        SYSUTCDATETIME(),
        NULL,
        NULL
    );

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_Actualizar
    @IdPadre UNIQUEIDENTIFIER,
    @IdUsuario UNIQUEIDENTIFIER,
    @Nombres VARCHAR(80),
    @ApellidoPaterno VARCHAR(60),
    @ApellidoMaterno VARCHAR(60),
    @DNI VARCHAR(8) = NULL,
    @Telefono VARCHAR(20) = NULL,
    @TelefonoAlterno VARCHAR(20) = NULL,
    @Direccion VARCHAR(200) = NULL,
    @CorreoContacto VARCHAR(120) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.PadreFamilia
    SET
        IdUsuario = @IdUsuario,
        Nombres = @Nombres,
        ApellidoPaterno = @ApellidoPaterno,
        ApellidoMaterno = @ApellidoMaterno,
        DNI = NULLIF(@DNI, ''),
        Telefono = NULLIF(@Telefono, ''),
        TelefonoAlterno = NULLIF(@TelefonoAlterno, ''),
        Direccion = NULLIF(@Direccion, ''),
        CorreoContacto = NULLIF(@CorreoContacto, ''),
        Estado = @Estado,
        FechaActualizacion = SYSUTCDATETIME()
    WHERE IdPadre = @IdPadre
      AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_Eliminar
    @IdPadre UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.PadreFamilia
    SET
        Estado = 0,
        FechaEliminacion = SYSUTCDATETIME(),
        FechaActualizacion = SYSUTCDATETIME()
    WHERE IdPadre = @IdPadre
      AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_ExistePorIdUsuario
    @IdUsuario UNIQUEIDENTIFIER,
    @IdPadreExcluir UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.PadreFamilia p
    WHERE p.IdUsuario = @IdUsuario
      AND p.FechaEliminacion IS NULL
      AND (@IdPadreExcluir IS NULL OR p.IdPadre <> @IdPadreExcluir);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_ExistePorDni
    @DNI VARCHAR(8),
    @IdPadreExcluir UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.PadreFamilia p
    WHERE p.DNI = @DNI
      AND p.FechaEliminacion IS NULL
      AND (@IdPadreExcluir IS NULL OR p.IdPadre <> @IdPadreExcluir);
END
GO

/* =========================================================
   CONDUCTOR
   ========================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_Conductor_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.IdConductor,
        c.IdUsuario,
        c.CodigoConductor,
        c.Nombres,
        c.ApellidoPaterno,
        c.ApellidoMaterno,
        c.DNI,
        c.Licencia,
        c.CategoriaLicencia,
        c.Telefono,
        c.Direccion,
        c.Estado,
        c.FechaCreacion,
        c.FechaActualizacion,
        c.FechaEliminacion
    FROM dbo.Conductor c
    WHERE c.FechaEliminacion IS NULL
    ORDER BY c.FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ListarPorId
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.IdConductor,
        c.IdUsuario,
        c.CodigoConductor,
        c.Nombres,
        c.ApellidoPaterno,
        c.ApellidoMaterno,
        c.DNI,
        c.Licencia,
        c.CategoriaLicencia,
        c.Telefono,
        c.Direccion,
        c.Estado,
        c.FechaCreacion,
        c.FechaActualizacion,
        c.FechaEliminacion
    FROM dbo.Conductor c
    WHERE c.IdConductor = @IdConductor
      AND c.FechaEliminacion IS NULL;
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

    DECLARE @Correlativo INT;

    SELECT @Correlativo = ISNULL(MAX(TRY_CONVERT(INT, RIGHT(CodigoConductor, 6))), 0) + 1
    FROM dbo.Conductor;

    SET @CodigoGenerado = CONCAT('CON-', RIGHT(CONCAT('000000', @Correlativo), 6));

    INSERT INTO dbo.Conductor
    (
        IdConductor,
        IdUsuario,
        CodigoConductor,
        Nombres,
        ApellidoPaterno,
        ApellidoMaterno,
        DNI,
        Licencia,
        CategoriaLicencia,
        Telefono,
        Direccion,
        Estado,
        FechaCreacion,
        FechaActualizacion,
        FechaEliminacion
    )
    VALUES
    (
        NEWID(),
        @IdUsuario,
        @CodigoGenerado,
        @Nombres,
        @ApellidoPaterno,
        @ApellidoMaterno,
        NULLIF(@DNI, ''),
        @Licencia,
        NULLIF(@CategoriaLicencia, ''),
        NULLIF(@Telefono, ''),
        NULLIF(@Direccion, ''),
        @Estado,
        SYSUTCDATETIME(),
        NULL,
        NULL
    );

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_Actualizar
    @IdConductor UNIQUEIDENTIFIER,
    @IdUsuario UNIQUEIDENTIFIER,
    @Nombres VARCHAR(80),
    @ApellidoPaterno VARCHAR(60),
    @ApellidoMaterno VARCHAR(60),
    @DNI VARCHAR(8) = NULL,
    @Licencia VARCHAR(30),
    @CategoriaLicencia VARCHAR(10) = NULL,
    @Telefono VARCHAR(20) = NULL,
    @Direccion VARCHAR(200) = NULL,
    @Estado BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Conductor
    SET
        IdUsuario = @IdUsuario,
        Nombres = @Nombres,
        ApellidoPaterno = @ApellidoPaterno,
        ApellidoMaterno = @ApellidoMaterno,
        DNI = NULLIF(@DNI, ''),
        Licencia = @Licencia,
        CategoriaLicencia = NULLIF(@CategoriaLicencia, ''),
        Telefono = NULLIF(@Telefono, ''),
        Direccion = NULLIF(@Direccion, ''),
        Estado = @Estado,
        FechaActualizacion = SYSUTCDATETIME()
    WHERE IdConductor = @IdConductor
      AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_Eliminar
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Conductor
    SET
        Estado = 0,
        FechaEliminacion = SYSUTCDATETIME(),
        FechaActualizacion = SYSUTCDATETIME()
    WHERE IdConductor = @IdConductor
      AND FechaEliminacion IS NULL;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ExistePorIdUsuario
    @IdUsuario UNIQUEIDENTIFIER,
    @IdConductorExcluir UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Conductor c
    WHERE c.IdUsuario = @IdUsuario
      AND c.FechaEliminacion IS NULL
      AND (@IdConductorExcluir IS NULL OR c.IdConductor <> @IdConductorExcluir);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ExistePorDni
    @DNI VARCHAR(8),
    @IdConductorExcluir UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Conductor c
    WHERE c.DNI = @DNI
      AND c.FechaEliminacion IS NULL
      AND (@IdConductorExcluir IS NULL OR c.IdConductor <> @IdConductorExcluir);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ExistePorLicencia
    @Licencia VARCHAR(30),
    @IdConductorExcluir UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.Conductor c
    WHERE c.Licencia = @Licencia
      AND c.FechaEliminacion IS NULL
      AND (@IdConductorExcluir IS NULL OR c.IdConductor <> @IdConductorExcluir);
END
GO

/* =========================================================
   COMPATIBILIDAD (wrappers de SP antiguos)
   ========================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_ObtenerPorId
    @IdPadre UNIQUEIDENTIFIER
AS
BEGIN
    EXEC dbo.sp_PadreFamilia_ListarPorId @IdPadre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PadreFamilia_EliminarLogico
    @IdPadre UNIQUEIDENTIFIER
AS
BEGIN
    EXEC dbo.sp_PadreFamilia_Eliminar @IdPadre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ObtenerPorId
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    EXEC dbo.sp_Conductor_ListarPorId @IdConductor;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Conductor_EliminarLogico
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    EXEC dbo.sp_Conductor_Eliminar @IdConductor;
END
GO

/* =========================================================
   SOPORTE NAVEGACIÓN CONDUCTOR (FILTRADO POR USUARIO/CONDUCTOR)
   ========================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_Conductor_ListarPorIdUsuario
    @IdUsuario UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        c.IdConductor,
        c.IdUsuario,
        c.CodigoConductor,
        c.Nombres,
        c.ApellidoPaterno,
        c.ApellidoMaterno,
        c.DNI,
        c.Licencia,
        c.CategoriaLicencia,
        c.Telefono,
        c.Direccion,
        c.Estado,
        c.FechaCreacion,
        c.FechaActualizacion,
        c.FechaEliminacion
    FROM dbo.Conductor c
    WHERE c.IdUsuario = @IdUsuario
      AND c.FechaEliminacion IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Recorrido_ListarPorConductor
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.IdRecorrido,
        r.IdRuta,
        r.IdBus,
        r.IdConductor,
        r.CodigoRecorrido,
        r.Fecha,
        r.EstadoRecorrido,
        r.Estado,
        ru.CodigoRuta,
        ru.Nombre AS NombreRuta,
        b.CodigoBus,
        b.Placa
    FROM dbo.Recorrido r
    INNER JOIN dbo.Ruta ru ON ru.IdRuta = r.IdRuta
    INNER JOIN dbo.Bus b ON b.IdBus = r.IdBus
    WHERE r.IdConductor = @IdConductor
      AND r.FechaEliminacion IS NULL
    ORDER BY r.Fecha DESC, r.FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Recorrido_ObtenerActivoPorConductor
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        r.IdRecorrido,
        r.IdRuta,
        r.IdBus,
        r.IdConductor,
        r.CodigoRecorrido,
        r.Fecha,
        r.EstadoRecorrido,
        r.Estado,
        ru.CodigoRuta,
        ru.Nombre AS NombreRuta,
        b.CodigoBus,
        b.Placa
    FROM dbo.Recorrido r
    INNER JOIN dbo.Ruta ru ON ru.IdRuta = r.IdRuta
    INNER JOIN dbo.Bus b ON b.IdBus = r.IdBus
    WHERE r.IdConductor = @IdConductor
      AND r.FechaEliminacion IS NULL
      AND r.Estado = 1
      AND r.EstadoRecorrido IN ('PROGRAMADO', 'EN_CURSO')
    ORDER BY CASE WHEN r.EstadoRecorrido = 'EN_CURSO' THEN 0 ELSE 1 END, r.Fecha DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Incidencia_ListarPorConductor
    @IdConductor UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.IdIncidencia,
        i.IdRecorrido,
        i.IdConductor,
        i.ReportadoPor,
        i.CodigoIncidencia,
        i.TipoIncidencia,
        i.Descripcion,
        i.FechaHora,
        i.EstadoIncidencia,
        i.Prioridad,
        i.FechaCierre,
        i.Solucion,
        i.Estado,
        i.FechaCreacion,
        i.FechaActualizacion,
        i.FechaEliminacion,
        r.CodigoRecorrido,
        (c.Nombres + ' ' + c.ApellidoPaterno + ' ' + c.ApellidoMaterno) AS NombreConductor,
        u.Username AS UsernameReportadoPor
    FROM dbo.Incidencia i
    INNER JOIN dbo.Recorrido r ON r.IdRecorrido = i.IdRecorrido
    INNER JOIN dbo.Conductor c ON c.IdConductor = i.IdConductor
    LEFT JOIN dbo.Usuario u ON u.IdUsuario = i.ReportadoPor
    WHERE i.IdConductor = @IdConductor
      AND i.FechaEliminacion IS NULL
    ORDER BY i.FechaHora DESC;
END
GO

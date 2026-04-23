/*
    CapiMovil - Fix consolidado de CódigoBus
    Fecha: 2026-04-23
    Objetivo:
      1) Corregir generación de fragmento BUS (placa + marca) sin caer en XXXXXX.
      2) Mantener correlativo BUS en CorrelativoDocumento.
      3) Reparar códigos BUS legacy con fragmento inválido.
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

CREATE OR ALTER FUNCTION dbo.fn_Bus_Codigo_Fragmento
(
    @Placa NVARCHAR(100),
    @Marca NVARCHAR(100)
)
RETURNS VARCHAR(6)
AS
BEGIN
    DECLARE @placaSan NVARCHAR(100) = dbo.fn_Codigo_Sanitizar(@Placa);
    DECLARE @marcaSan NVARCHAR(100) = dbo.fn_Codigo_Sanitizar(@Marca);

    DECLARE @placaFrag NVARCHAR(6) = LEFT(@placaSan, 3);
    DECLARE @marcaFrag NVARCHAR(6) = LEFT(@marcaSan, 3);

    IF LEN(@placaFrag) < 3
        SET @placaFrag = @placaFrag + LEFT(@marcaSan, 3 - LEN(@placaFrag));

    IF LEN(@marcaFrag) < 3
        SET @marcaFrag = @marcaFrag + LEFT(@placaSan, 3 - LEN(@marcaFrag));

    IF LEN(@placaFrag) < 3
        SET @placaFrag = @placaFrag + REPLICATE('B', 3 - LEN(@placaFrag));

    IF LEN(@marcaFrag) < 3
        SET @marcaFrag = @marcaFrag + REPLICATE('U', 3 - LEN(@marcaFrag));

    RETURN CONVERT(VARCHAR(6), LEFT(@placaFrag, 3) + LEFT(@marcaFrag, 3));
END
GO

/* =========================================================
   SP BUS REGISTRAR (fuente oficial de CodigoBus)
   ========================================================= */
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

    SET @Fragmento = dbo.fn_Bus_Codigo_Fragmento(@Placa, @Marca);

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

/* =========================================================
   NORMALIZACIÓN DE CÓDIGOS BUS LEGACY INVÁLIDOS
   ========================================================= */
BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @BaseCorrelativo INT = (
        SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoBus, 4) AS INT)), 0)
        FROM dbo.Bus
        WHERE CodigoBus LIKE 'BUS-%[0-9][0-9][0-9][0-9]'
    );

    ;WITH Pendientes AS
    (
        SELECT
            b.IdBus,
            ROW_NUMBER() OVER (ORDER BY b.FechaCreacion, b.IdBus) AS Nro,
            dbo.fn_Bus_Codigo_Fragmento(b.Placa, b.Marca) AS Fragmento
        FROM dbo.Bus b
        WHERE b.CodigoBus IS NULL
           OR b.CodigoBus LIKE 'BUS-XXXXXX%'
           OR b.CodigoBus NOT LIKE 'BUS-[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][0-9][0-9][0-9][0-9]'
    )
    UPDATE b
       SET b.CodigoBus = CONCAT('BUS-', p.Fragmento, RIGHT(CONCAT('0000', @BaseCorrelativo + p.Nro), 4))
    FROM dbo.Bus b
    INNER JOIN Pendientes p ON p.IdBus = b.IdBus;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

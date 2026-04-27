/*
    CapiMovil - Corrección de combos de usuario por rol
    Objetivo:
      - PadreFamilia: solo usuarios rol PADRE / PADRE DE FAMILIA no vinculados
      - Conductor: solo usuarios rol CONDUCTOR no vinculados
*/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ListarDisponiblesParaPadre
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.Username,
        U.Correo,
        U.Estado
    FROM dbo.Usuario U
    INNER JOIN dbo.Rol R ON R.IdRol = U.IdRol
    WHERE U.Estado = 1
      AND U.FechaEliminacion IS NULL
      AND R.FechaEliminacion IS NULL
      AND R.Estado = 1
      AND (
            UPPER(LTRIM(RTRIM(R.Nombre))) IN ('PADRE', 'PADRE DE FAMILIA')
            OR UPPER(LTRIM(RTRIM(R.CodigoRol))) LIKE '%PAD%'
          )
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PadreFamilia P
          WHERE P.IdUsuario = U.IdUsuario
            AND P.FechaEliminacion IS NULL
      )
    ORDER BY U.Username;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ListarDisponiblesParaConductor
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.Username,
        U.Correo,
        U.Estado
    FROM dbo.Usuario U
    INNER JOIN dbo.Rol R ON R.IdRol = U.IdRol
    WHERE U.Estado = 1
      AND U.FechaEliminacion IS NULL
      AND R.FechaEliminacion IS NULL
      AND R.Estado = 1
      AND (
            UPPER(LTRIM(RTRIM(R.Nombre))) = 'CONDUCTOR'
            OR UPPER(LTRIM(RTRIM(R.CodigoRol))) LIKE '%COND%'
          )
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.Conductor C
          WHERE C.IdUsuario = U.IdUsuario
            AND C.FechaEliminacion IS NULL
      )
    ORDER BY U.Username;
END
GO

/*
   Normalización opcional de códigos legacy del módulo Operaciones.
   Entidades: Recorrido, UbicacionBus, EventoAbordaje, Notificacion,
              Incidencia, Auditoria, IA_Consulta, IA_Prediccion.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @InicioRec INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoRecorrido, 4) AS INT)), 0) FROM dbo.Recorrido WHERE CodigoRecorrido LIKE 'REC-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioUbi INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoUbicacion, 4) AS INT)), 0) FROM dbo.UbicacionBus WHERE CodigoUbicacion LIKE 'UBI-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioEve INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoEvento, 4) AS INT)), 0) FROM dbo.EventoAbordaje WHERE CodigoEvento LIKE 'EVE-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioNot INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoNotificacion, 4) AS INT)), 0) FROM dbo.Notificacion WHERE CodigoNotificacion LIKE 'NOT-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioInc INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoIncidencia, 4) AS INT)), 0) FROM dbo.Incidencia WHERE CodigoIncidencia LIKE 'INC-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioAud INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoAuditoria, 4) AS INT)), 0) FROM dbo.Auditoria WHERE CodigoAuditoria LIKE 'AUD-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioIac INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoConsulta, 4) AS INT)), 0) FROM dbo.IA_Consulta WHERE CodigoConsulta LIKE 'IAC-%[0-9][0-9][0-9][0-9]');
    DECLARE @InicioIap INT = (SELECT ISNULL(MAX(TRY_CAST(RIGHT(CodigoPrediccion, 4) AS INT)), 0) FROM dbo.IA_Prediccion WHERE CodigoPrediccion LIKE 'IAP-%[0-9][0-9][0-9][0-9]');

    ;WITH Base AS (
        SELECT r.IdRecorrido, ROW_NUMBER() OVER (ORDER BY r.FechaCreacion, r.IdRecorrido) AS Nro,
               LEFT(UPPER(REPLACE(REPLACE(ISNULL(ru.CodigoRuta, 'RUTA'), ' ', ''), '-', '')) + 'XXXX', 4) AS Fragmento
        FROM dbo.Recorrido r
        INNER JOIN dbo.Ruta ru ON ru.IdRuta = r.IdRuta
        WHERE r.CodigoRecorrido IS NULL OR r.CodigoRecorrido NOT LIKE 'REC-[A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE r
       SET r.CodigoRecorrido = CONCAT('REC-', b.Fragmento, RIGHT(CONCAT('0000', @InicioRec + b.Nro), 4))
    FROM dbo.Recorrido r
    INNER JOIN Base b ON b.IdRecorrido = r.IdRecorrido;

    ;WITH Base AS (
        SELECT u.IdUbicacion, ROW_NUMBER() OVER (ORDER BY u.FechaCreacion, u.IdUbicacion) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(r.CodigoRecorrido, 'RECU'), 'REC-', ''), '-', ''), 4)) AS Fragmento
        FROM dbo.UbicacionBus u
        INNER JOIN dbo.Recorrido r ON r.IdRecorrido = u.IdRecorrido
        WHERE u.CodigoUbicacion IS NULL OR u.CodigoUbicacion NOT LIKE 'UBI-[A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE u
       SET u.CodigoUbicacion = CONCAT('UBI-', b.Fragmento, RIGHT(CONCAT('0000', @InicioUbi + b.Nro), 4))
    FROM dbo.UbicacionBus u
    INNER JOIN Base b ON b.IdUbicacion = u.IdUbicacion;

    ;WITH Base AS (
        SELECT e.IdEvento, ROW_NUMBER() OVER (ORDER BY e.FechaCreacion, e.IdEvento) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(e.TipoEvento, 'EV'), ' ', ''), '-', ''), 2)) +
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(es.Nombres, 'NO'), ' ', ''), '-', ''), 2)) +
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(es.ApellidoPaterno, 'AP'), ' ', ''), '-', ''), 2)) AS Fragmento
        FROM dbo.EventoAbordaje e
        INNER JOIN dbo.Estudiante es ON es.IdEstudiante = e.IdEstudiante
        WHERE e.CodigoEvento IS NULL OR e.CodigoEvento NOT LIKE 'EVE-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE e
       SET e.CodigoEvento = CONCAT('EVE-', b.Fragmento, RIGHT(CONCAT('0000', @InicioEve + b.Nro), 4))
    FROM dbo.EventoAbordaje e
    INNER JOIN Base b ON b.IdEvento = e.IdEvento;

    ;WITH Base AS (
        SELECT n.IdNotificacion, ROW_NUMBER() OVER (ORDER BY n.FechaCreacion, n.IdNotificacion) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(n.TipoNotificacion, 'NOTIFI'), ' ', ''), '-', ''), 6)) AS Fragmento
        FROM dbo.Notificacion n
        WHERE n.CodigoNotificacion IS NULL OR n.CodigoNotificacion NOT LIKE 'NOT-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE n
       SET n.CodigoNotificacion = CONCAT('NOT-', RIGHT(CONCAT(b.Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @InicioNot + b.Nro), 4))
    FROM dbo.Notificacion n
    INNER JOIN Base b ON b.IdNotificacion = n.IdNotificacion;

    ;WITH Base AS (
        SELECT i.IdIncidencia, ROW_NUMBER() OVER (ORDER BY i.FechaCreacion, i.IdIncidencia) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(i.TipoIncidencia, 'INCIDE'), ' ', ''), '-', ''), 6)) AS Fragmento
        FROM dbo.Incidencia i
        WHERE i.CodigoIncidencia IS NULL OR i.CodigoIncidencia NOT LIKE 'INC-[A-Z][A-Z][A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE i
       SET i.CodigoIncidencia = CONCAT('INC-', RIGHT(CONCAT(b.Fragmento, 'XXXXXX'), 6), RIGHT(CONCAT('0000', @InicioInc + b.Nro), 4))
    FROM dbo.Incidencia i
    INNER JOIN Base b ON b.IdIncidencia = i.IdIncidencia;

    ;WITH Base AS (
        SELECT a.IdAuditoria, ROW_NUMBER() OVER (ORDER BY a.Fecha, a.IdAuditoria) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(a.Tabla, 'AUD'), ' ', ''), '-', ''), 3)) AS Fragmento
        FROM dbo.Auditoria a
        WHERE a.CodigoAuditoria IS NULL OR a.CodigoAuditoria NOT LIKE 'AUD-[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE a
       SET a.CodigoAuditoria = CONCAT('AUD-', RIGHT(CONCAT(b.Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @InicioAud + b.Nro), 4))
    FROM dbo.Auditoria a
    INNER JOIN Base b ON b.IdAuditoria = a.IdAuditoria;

    ;WITH Base AS (
        SELECT c.IdConsulta, ROW_NUMBER() OVER (ORDER BY c.FechaConsulta, c.IdConsulta) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(c.TipoConsulta, 'CON'), ' ', ''), '-', ''), 3)) AS Fragmento
        FROM dbo.IA_Consulta c
        WHERE c.CodigoConsulta IS NULL OR c.CodigoConsulta NOT LIKE 'IAC-[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE c
       SET c.CodigoConsulta = CONCAT('IAC-', RIGHT(CONCAT(b.Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @InicioIac + b.Nro), 4))
    FROM dbo.IA_Consulta c
    INNER JOIN Base b ON b.IdConsulta = c.IdConsulta;

    ;WITH Base AS (
        SELECT p.IdPrediccion, ROW_NUMBER() OVER (ORDER BY p.FechaGeneracion, p.IdPrediccion) AS Nro,
               UPPER(LEFT(REPLACE(REPLACE(ISNULL(p.TipoPrediccion, 'PRE'), ' ', ''), '-', ''), 3)) AS Fragmento
        FROM dbo.IA_Prediccion p
        WHERE p.CodigoPrediccion IS NULL OR p.CodigoPrediccion NOT LIKE 'IAP-[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]'
    )
    UPDATE p
       SET p.CodigoPrediccion = CONCAT('IAP-', RIGHT(CONCAT(b.Fragmento, 'XXX'), 3), RIGHT(CONCAT('0000', @InicioIap + b.Nro), 4))
    FROM dbo.IA_Prediccion p
    INNER JOIN Base b ON b.IdPrediccion = p.IdPrediccion;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

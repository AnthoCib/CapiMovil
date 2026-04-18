using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionApiController : ControllerBase
    {
        private readonly NotificacionBC _notificacionBC;

        public NotificacionApiController(NotificacionBC notificacionBC)
        {
            _notificacionBC = notificacionBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(_notificacionBC.Listar());
        }

        [HttpGet("{id:guid}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var entidad = _notificacionBC.ListarPorId(id);

            if (entidad == null)
                return NotFound(new { mensaje = "La notificación no existe." });

            return Ok(entidad);
        }

        [HttpGet("padre/{idPadre:guid}")]
        public IActionResult ListarPorPadre(Guid idPadre)
        {
            return Ok(_notificacionBC.ListarPorPadre(idPadre));
        }

        [HttpGet("padre/{idPadre:guid}/noleidas")]
        public IActionResult ListarNoLeidasPorPadre(Guid idPadre)
        {
            return Ok(_notificacionBC.ListarNoLeidasPorPadre(idPadre));
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] NotificacionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                NotificacionBE entidad = new NotificacionBE
                {
                    IdPadre = request.IdPadre,
                    IdEstudiante = request.IdEstudiante,
                    Titulo = request.Titulo,
                    Mensaje = request.Mensaje,
                    TipoNotificacion = request.TipoNotificacion,
                    Canal = request.Canal,
                    Leido = request.Leido
                };

                bool ok = _notificacionBC.Registrar(entidad);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo registrar la notificación." });

                return Ok(new
                {
                    mensaje = "Notificación registrada correctamente.",
                    codigo = entidad.CodigoNotificacion
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al registrar la notificación." });
            }
        }

        [HttpPut("{id:guid}")]
        public IActionResult Actualizar(Guid id, [FromBody] NotificacionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                NotificacionBE entidad = new NotificacionBE
                {
                    IdNotificacion = id,
                    IdPadre = request.IdPadre,
                    IdEstudiante = request.IdEstudiante,
                    Titulo = request.Titulo,
                    Mensaje = request.Mensaje,
                    TipoNotificacion = request.TipoNotificacion,
                    Canal = request.Canal,
                    Leido = request.Leido
                };

                bool ok = _notificacionBC.Actualizar(entidad);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo actualizar la notificación." });

                return Ok(new { mensaje = "Notificación actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al actualizar la notificación." });
            }
        }

        [HttpPatch("{id:guid}/leer")]
        public IActionResult MarcarLeida(Guid id)
        {
            try
            {
                bool ok = _notificacionBC.MarcarLeida(id);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo marcar la notificación." });

                return Ok(new { mensaje = "Notificación marcada como leída." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al marcar la notificación." });
            }
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _notificacionBC.Eliminar(id);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo eliminar la notificación." });

                return Ok(new { mensaje = "Notificación eliminada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al eliminar la notificación." });
            }
        }
    }
}
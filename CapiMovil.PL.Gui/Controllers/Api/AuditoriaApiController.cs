using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaApiController : ControllerBase
    {
        private readonly AuditoriaBC _auditoriaBC;

        public AuditoriaApiController(AuditoriaBC auditoriaBC)
        {
            _auditoriaBC = auditoriaBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(_auditoriaBC.Listar());
        }

        [HttpGet("{id:guid}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var entidad = _auditoriaBC.ListarPorId(id);

            if (entidad == null)
                return NotFound(new { mensaje = "El registro de auditoría no existe." });

            return Ok(entidad);
        }

        [HttpGet("tabla/{tabla}")]
        public IActionResult ListarPorTabla(string tabla)
        {
            return Ok(_auditoriaBC.ListarPorTabla(tabla));
        }

        [HttpGet("accion/{accion}")]
        public IActionResult ListarPorAccion(string accion)
        {
            return Ok(_auditoriaBC.ListarPorAccion(accion));
        }

        [HttpGet("usuario/{usuarioId:guid}")]
        public IActionResult ListarPorUsuario(Guid usuarioId)
        {
            return Ok(_auditoriaBC.ListarPorUsuario(usuarioId));
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] AuditoriaRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                AuditoriaBE entidad = new AuditoriaBE
                {
                    Tabla = request.Tabla,
                    IdRegistro = request.IdRegistro,
                    Accion = request.Accion,
                    DatosAntes = request.DatosAntes,
                    DatosDespues = request.DatosDespues,
                    UsuarioId = request.UsuarioId,
                    NombreUsuario = request.NombreUsuario,
                    Ip = request.Ip,
                    UserAgent = request.UserAgent,
                    Modulo = request.Modulo,
                    Observacion = request.Observacion
                };

                bool ok = _auditoriaBC.Registrar(entidad);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo registrar la auditoría." });

                return Ok(new
                {
                    mensaje = "Auditoría registrada correctamente.",
                    codigo = entidad.CodigoAuditoria
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al registrar la auditoría." });
            }
        }
    }
}
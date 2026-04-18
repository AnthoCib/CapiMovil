using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidenciaApiController : ControllerBase
    {
        private readonly IncidenciaBC _incidenciaBC;
        private readonly RecorridoBC _recorridoBC;

        public IncidenciaApiController(
            IncidenciaBC incidenciaBC,
            RecorridoBC recorridoBC)
        {
            _incidenciaBC = incidenciaBC;
            _recorridoBC = recorridoBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _incidenciaBC.Listar();
            return Ok(lista);
        }

        [HttpGet("{id:guid}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var entidad = _incidenciaBC.ListarPorId(id);

            if (entidad == null)
                return NotFound(new { mensaje = "La incidencia no existe." });

            return Ok(entidad);
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] IncidenciaRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                RecorridoBE? recorrido = _recorridoBC.ListarPorId(request.IdRecorrido);

                if (recorrido == null)
                    return BadRequest(new { mensaje = "No se encontró el recorrido seleccionado." });

                if (recorrido.IdConductor == Guid.Empty)
                    return BadRequest(new { mensaje = "El recorrido no tiene un conductor asociado." });

                IncidenciaBE entidad = new IncidenciaBE
                {
                    IdRecorrido = request.IdRecorrido,
                    IdConductor = recorrido.IdConductor,
                    ReportadoPor = request.ReportadoPor,
                    TipoIncidencia = request.TipoIncidencia,
                    Descripcion = request.Descripcion,
                    FechaHora = request.FechaHora,
                    EstadoIncidencia = request.EstadoIncidencia,
                    Prioridad = request.Prioridad,
                    Solucion = request.Solucion
                };

                bool ok = _incidenciaBC.Registrar(entidad);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo registrar la incidencia." });

                return Ok(new
                {
                    mensaje = "Incidencia registrada correctamente.",
                    codigo = entidad.CodigoIncidencia,
                    id = entidad.IdIncidencia
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al registrar la incidencia." });
            }
        }

        [HttpPut("{id:guid}")]
        public IActionResult Actualizar(Guid id, [FromBody] IncidenciaRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                RecorridoBE? recorrido = _recorridoBC.ListarPorId(request.IdRecorrido);

                if (recorrido == null)
                    return BadRequest(new { mensaje = "No se encontró el recorrido seleccionado." });

                if (recorrido.IdConductor == Guid.Empty)
                    return BadRequest(new { mensaje = "El recorrido no tiene un conductor asociado." });

                IncidenciaBE entidad = new IncidenciaBE
                {
                    IdIncidencia = id,
                    IdRecorrido = request.IdRecorrido,
                    IdConductor = recorrido.IdConductor,
                    ReportadoPor = request.ReportadoPor,
                    TipoIncidencia = request.TipoIncidencia,
                    Descripcion = request.Descripcion,
                    FechaHora = request.FechaHora,
                    EstadoIncidencia = request.EstadoIncidencia,
                    Prioridad = request.Prioridad,
                    Solucion = request.Solucion
                };

                bool ok = _incidenciaBC.Actualizar(entidad);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo actualizar la incidencia." });

                return Ok(new { mensaje = "Incidencia actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al actualizar la incidencia." });
            }
        }

        [HttpPatch("{id:guid}/estado")]
        public IActionResult CambiarEstado(Guid id, [FromBody] CambiarEstadoIncidenciaRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool ok = _incidenciaBC.CambiarEstado(id, request.EstadoIncidencia);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo actualizar el estado." });

                return Ok(new { mensaje = "Estado actualizado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al cambiar el estado." });
            }
        }

        [HttpPatch("{id:guid}/cerrar")]
        public IActionResult Cerrar(Guid id, [FromBody] CerrarIncidenciaRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool ok = _incidenciaBC.Cerrar(id, request.Solucion);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo cerrar la incidencia." });

                return Ok(new { mensaje = "Incidencia cerrada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al cerrar la incidencia." });
            }
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _incidenciaBC.Eliminar(id);

                if (!ok)
                    return BadRequest(new { mensaje = "No se pudo eliminar la incidencia." });

                return Ok(new { mensaje = "Incidencia eliminada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al eliminar la incidencia." });
            }
        }
    }
}
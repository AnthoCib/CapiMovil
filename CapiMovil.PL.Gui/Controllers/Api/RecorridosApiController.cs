using CapiMovil.BL.BC;
using CapiMovil.PL.Gui.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers.Api
{
    [ApiController]
    [Route("api/recorridos")]
    public class RecorridosApiController : ControllerBase
    {
        private readonly RecorridoBC _recorridoBC;
        private readonly RutaBC _rutaBC;
        private readonly ParaderoBC _paraderoBC;
        private readonly UbicacionBusBC _ubicacionBusBC;

        public RecorridosApiController(RecorridoBC recorridoBC, RutaBC rutaBC, ParaderoBC paraderoBC, UbicacionBusBC ubicacionBusBC)
        {
            _recorridoBC = recorridoBC;
            _rutaBC = rutaBC;
            _paraderoBC = paraderoBC;
            _ubicacionBusBC = ubicacionBusBC;
        }

        [HttpGet("{idRecorrido:guid}/mapa")]
        public ActionResult<RecorridoMapaDTO> ObtenerMapa(Guid idRecorrido)
        {
            if (idRecorrido == Guid.Empty)
            {
                return BadRequest(new { mensaje = "Id de recorrido inválido." });
            }

            try
            {
                var recorrido = _recorridoBC.ListarPorId(idRecorrido);
                if (recorrido == null)
                {
                    return NotFound(new { mensaje = "No se encontró el recorrido." });
                }

                var ruta = _rutaBC.ListarPorId(recorrido.IdRuta);
                var paraderos = _paraderoBC.ListarPorRuta(recorrido.IdRuta)
                    .OrderBy(p => p.OrdenParada)
                    .ToList();

                var ubicacionActual = _ubicacionBusBC.Listar()
                    .Where(u => u.IdRecorrido == idRecorrido && u.Estado)
                    .OrderByDescending(u => u.FechaHora)
                    .FirstOrDefault();

                var dto = new RecorridoMapaDTO
                {
                    IdRecorrido = recorrido.IdRecorrido,
                    CodigoRecorrido = recorrido.CodigoRecorrido,
                    EstadoRecorrido = recorrido.EstadoRecorrido,
                    Bus = recorrido.Bus == null ? null : new BusResumenDTO
                    {
                        IdBus = recorrido.Bus.IdBus,
                        CodigoBus = recorrido.Bus.CodigoBus,
                        Placa = recorrido.Bus.Placa
                    },
                    Ruta = ruta == null ? null : new RutaMapaDTO
                    {
                        IdRuta = ruta.IdRuta,
                        Nombre = ruta.Nombre,
                        PuntoInicio = ruta.PuntoInicio ?? ruta.DireccionInicio ?? "Sin referencia",
                        PuntoFin = ruta.PuntoFin ?? ruta.DireccionFin ?? "Sin referencia",
                        LatitudInicio = ruta.LatitudInicio,
                        LongitudInicio = ruta.LongitudInicio,
                        LatitudFin = ruta.LatitudFin,
                        LongitudFin = ruta.LongitudFin
                    },
                    UbicacionActual = ubicacionActual == null ? null : new UbicacionBusDTO
                    {
                        Latitud = ubicacionActual.Latitud,
                        Longitud = ubicacionActual.Longitud,
                        FechaHora = ubicacionActual.FechaHora
                    },
                    Paraderos = paraderos.Select(p => new ParaderoMapaDTO
                    {
                        IdParadero = p.IdParadero,
                        Nombre = p.Nombre,
                        Direccion = p.Direccion,
                        OrdenParada = p.OrdenParada,
                        HoraEstimada = p.HoraEstimada.HasValue ? DateTime.Today.Add(p.HoraEstimada.Value).ToString("HH:mm") : string.Empty,
                        Latitud = p.Latitud,
                        Longitud = p.Longitud,
                        TotalAlumnos = 0
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al obtener el mapa del recorrido." });
            }
        }
    }
}

using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class RecorridoDALC : ICrudDALC<RecorridoBE>
    {
        private readonly BDConexion _bdConexion;

        public RecorridoDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<RecorridoBE> Listar()
        {
            List<RecorridoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RecorridoBE
                {
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    HoraInicioProgramada = dr["HoraInicioProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraInicioProgramada"],
                    HoraFinProgramada = dr["HoraFinProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraFinProgramada"],
                    HoraInicioReal = dr["HoraInicioReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraInicioReal"]),
                    HoraFinReal = dr["HoraFinReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraFinReal"]),
                    EstadoRecorrido = dr["EstadoRecorrido"]?.ToString() ?? "PROGRAMADO",
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Ruta = new RutaBE
                    {
                        IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                        CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreRuta"]?.ToString() ?? string.Empty
                    },
                    Bus = new BusBE
                    {
                        IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                        CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                        Placa = dr["Placa"]?.ToString() ?? string.Empty
                    },
                    Conductor = new ConductorBE
                    {
                        IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                        CodigoConductor = dr["CodigoConductor"]?.ToString() ?? string.Empty,
                        Nombres = dr["NombresConductor"]?.ToString() ?? string.Empty,
                        ApellidoPaterno = dr["ApellidoPaternoConductor"]?.ToString() ?? string.Empty
                    }
                });
            }

            return lista;
        }

        public RecorridoBE? ListarPorId(Guid id)
        {
            RecorridoBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new RecorridoBE
                {
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    HoraInicioProgramada = dr["HoraInicioProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraInicioProgramada"],
                    HoraFinProgramada = dr["HoraFinProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraFinProgramada"],
                    HoraInicioReal = dr["HoraInicioReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraInicioReal"]),
                    HoraFinReal = dr["HoraFinReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraFinReal"]),
                    EstadoRecorrido = dr["EstadoRecorrido"]?.ToString() ?? "PROGRAMADO",
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"])
                };
            }

            return entidad;
        }

        public bool Registrar(RecorridoBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@IdBus", entidad.IdBus);
            cmd.Parameters.AddWithValue("@IdConductor", entidad.IdConductor);
            cmd.Parameters.AddWithValue("@Fecha", entidad.Fecha);
            cmd.Parameters.AddWithValue("@HoraInicioProgramada", (object?)entidad.HoraInicioProgramada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HoraFinProgramada", (object?)entidad.HoraFinProgramada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoRecorrido", entidad.EstadoRecorrido);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);
                if (filas > 0)
                {
                    entidad.CodigoRecorrido = dr["CodigoGenerado"]?.ToString() ?? string.Empty;
                    return true;
                }
            }

            return false;
        }

        public bool Actualizar(RecorridoBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@IdBus", entidad.IdBus);
            cmd.Parameters.AddWithValue("@IdConductor", entidad.IdConductor);
            cmd.Parameters.AddWithValue("@Fecha", entidad.Fecha);
            cmd.Parameters.AddWithValue("@HoraInicioProgramada", (object?)entidad.HoraInicioProgramada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HoraFinProgramada", (object?)entidad.HoraFinProgramada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoRecorrido", entidad.EstadoRecorrido);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Eliminar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Iniciar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Iniciar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Finalizar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Finalizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Cancelar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Cancelar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }
        public List<RecorridoBE> ListarActivosParaOperacion()
        {
            List<RecorridoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ListarActivosParaOperacion", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RecorridoBE
                {
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                    Fecha = Convert.ToDateTime(dr["Fecha"])
                });
            }

            return lista;
        }
        public List<EstudianteBE> ListarDestinatariosPorRecorrido(Guid idRecorrido)
        {
            List<EstudianteBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ListarDestinatariosNotificacion", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante"))
                });
            }

            return lista;
        }
        public List<EstudianteBE> ListarEstudiantesPorRecorrido(Guid idRecorrido)
        {
            List<EstudianteBE> lista = new List<EstudianteBE>();
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ListarEstudiantesPorRecorrido", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);
            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty
                    // NombreCompleto se calcula automáticamente porque es propiedad de solo lectura
                });
            }
            return lista;
        }

        public bool RegistrarEventoAbordaje(Guid idRecorrido, Guid idEstudiante, string tipoEvento, string? observacion)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_Registrar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);
            cmd.Parameters.AddWithValue("@IdEstudiante", idEstudiante);
            cmd.Parameters.AddWithValue("@TipoEvento", tipoEvento);
            cmd.Parameters.AddWithValue("@FechaHora", DateTime.Now);
            cmd.Parameters.AddWithValue("@Observacion", (object?)observacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", true);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }
            return false;
        }

        public List<RecorridoBE> ListarHoyPorConductor(Guid idConductor, DateTime fecha)
        {
            List<RecorridoBE> lista = new List<RecorridoBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarRecorridosHoy", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);
            cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RecorridoBE
                {
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    HoraInicioProgramada = dr["HoraInicioProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraInicioProgramada"],
                    HoraFinProgramada = dr["HoraFinProgramada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraFinProgramada"],
                    HoraInicioReal = dr["HoraInicioReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraInicioReal"]),
                    HoraFinReal = dr["HoraFinReal"] == DBNull.Value ? null : Convert.ToDateTime(dr["HoraFinReal"]),
                    EstadoRecorrido = dr["EstadoRecorrido"]?.ToString() ?? "PROGRAMADO",
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),

                    Ruta = new RutaBE
                    {
                        IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                        CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreRuta"]?.ToString() ?? string.Empty
                    },

                    Bus = new BusBE
                    {
                        IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                        CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                        Placa = dr["Placa"]?.ToString() ?? string.Empty
                    },

                    Conductor = new ConductorBE
                    {
                        IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                        CodigoConductor = dr["CodigoConductor"]?.ToString() ?? string.Empty,
                        Nombres = dr["NombresConductor"]?.ToString() ?? string.Empty,
                        ApellidoPaterno = dr["ApellidoPaternoConductor"]?.ToString() ?? string.Empty
                    }
                });
            }

            return lista;
        }

        public List<EstudianteRutaEstadoBE> ListarEstudiantesPorRecorridoConductor(Guid idRecorrido)
{
            List<EstudianteRutaEstadoBE> lista = new List<EstudianteRutaEstadoBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarEstudiantesPorRecorrido", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteRutaEstadoBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty,
                    Grado = dr["Grado"] == DBNull.Value ? string.Empty : dr["Grado"].ToString() ?? string.Empty,
                    Seccion = dr["Seccion"] == DBNull.Value ? string.Empty : dr["Seccion"].ToString() ?? string.Empty,
                    EstadoEvento = dr["EstadoEvento"] == DBNull.Value ? "PENDIENTE" : dr["EstadoEvento"].ToString() ?? "PENDIENTE",

                    NombrePadre = dr["NombrePadre"] == DBNull.Value ? string.Empty : dr["NombrePadre"].ToString() ?? string.Empty,
                    TelefonoPadre = dr["TelefonoPadre"] == DBNull.Value ? null : dr["TelefonoPadre"].ToString(),

                    ParaderoSubidaNombre = dr["ParaderoSubidaNombre"] == DBNull.Value ? null : dr["ParaderoSubidaNombre"].ToString(),
                    ParaderoSubidaDireccion = dr["ParaderoSubidaDireccion"] == DBNull.Value ? null : dr["ParaderoSubidaDireccion"].ToString(),

                    ParaderoBajadaNombre = dr["ParaderoBajadaNombre"] == DBNull.Value ? null : dr["ParaderoBajadaNombre"].ToString(),
                    ParaderoBajadaDireccion = dr["ParaderoBajadaDireccion"] == DBNull.Value ? null : dr["ParaderoBajadaDireccion"].ToString()
                });
            }

            return lista;
        }

        public List<ParaderoConductorBE> ListarParaderosPorRecorrido(Guid idRecorrido, string tipoParadero)
        {
            List<ParaderoConductorBE> lista = new List<ParaderoConductorBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarParaderosPorRecorrido", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);
            cmd.Parameters.AddWithValue("@TipoParadero", tipoParadero);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ParaderoConductorBE
                {
                    IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Direccion = dr["Direccion"]?.ToString() ?? string.Empty,
                    OrdenParada = Convert.ToInt32(dr["OrdenParada"]),
                    HoraEstimada = dr["HoraEstimada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraEstimada"],
                    TotalAlumnos = Convert.ToInt32(dr["TotalAlumnos"]),
                    TotalRegistrados = Convert.ToInt32(dr["TotalRegistrados"]),
                    Completado = Convert.ToBoolean(dr["Completado"])
                });
            }

            return lista;
        }

        public List<IncidenciaBE> ListarIncidenciasPendientesConductor(Guid idRecorrido)
        {
            List<IncidenciaBE> lista = new List<IncidenciaBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarIncidenciasPendientes", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new IncidenciaBE
                {
                    IdIncidencia = dr.GetGuid(dr.GetOrdinal("IdIncidencia")),
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    ReportadoPor = dr["ReportadoPor"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("ReportadoPor")),
                    CodigoIncidencia = dr["CodigoIncidencia"]?.ToString() ?? string.Empty,
                    TipoIncidencia = dr["TipoIncidencia"]?.ToString() ?? string.Empty,
                    Descripcion = dr["Descripcion"]?.ToString() ?? string.Empty,
                    FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                    EstadoIncidencia = dr["EstadoIncidencia"]?.ToString() ?? string.Empty,
                    Prioridad = dr["Prioridad"]?.ToString() ?? string.Empty,
                    FechaCierre = dr["FechaCierre"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaCierre"]),
                    Solucion = dr["Solucion"] == DBNull.Value ? null : dr["Solucion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public bool PuedeIniciarSegunOrden(Guid idRecorrido)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_PuedeIniciarSegunOrden", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToBoolean(result);
            }

            return false;
        }
        public List<EstudianteBE> ListarEstudiantesPorParaderoConductor(Guid idRecorrido, Guid idParadero, string tipoParadero)
        {
            List<EstudianteBE> lista = new List<EstudianteBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarEstudiantesPorParadero", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);
            cmd.Parameters.AddWithValue("@IdParadero", idParadero);
            cmd.Parameters.AddWithValue("@TipoParadero", tipoParadero);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    CodigoEstudiante = dr["CodigoEstudiante"]?.ToString() ?? string.Empty,
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    FechaNacimiento = dr["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaNacimiento"]),
                    Genero = dr["Genero"] == DBNull.Value ? null : dr["Genero"].ToString(),
                    Grado = dr["Grado"] == DBNull.Value ? null : dr["Grado"].ToString(),
                    Seccion = dr["Seccion"] == DBNull.Value ? null : dr["Seccion"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"])
                });
            }

            return lista;
        }

        public bool RegistrarEventoAbordajeConductor(
            Guid idRecorrido,
            Guid idEstudiante,
            Guid idParadero,
            Guid registradoPor,
            string tipoEvento,
            string? observacion)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdRecorrido", idRecorrido);
            cmd.Parameters.AddWithValue("@IdEstudiante", idEstudiante);
            cmd.Parameters.AddWithValue("@IdParadero", idParadero);
            cmd.Parameters.AddWithValue("@RegistradoPor", registradoPor);
            cmd.Parameters.AddWithValue("@TipoEvento", tipoEvento);
            cmd.Parameters.AddWithValue("@FechaHora", DateTime.Now);
            cmd.Parameters.AddWithValue("@Observacion", (object?)observacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", true);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }
    }
}
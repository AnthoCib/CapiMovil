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

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                entidad.CodigoRecorrido = codigoGenerado;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

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
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }

        public bool Eliminar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }

        public bool Iniciar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Iniciar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out string? mensaje))
                return true;

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

            return false;
        }

        public bool Finalizar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Finalizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out string? mensaje))
                return true;

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

            return false;
        }

        public bool Cancelar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_Cancelar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
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

        public List<RecorridoBE> ListarPorConductor(Guid idConductor)
        {
            List<RecorridoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ListarPorConductor", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);

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
                    EstadoRecorrido = dr["EstadoRecorrido"]?.ToString() ?? "PROGRAMADO",
                    Estado = Convert.ToBoolean(dr["Estado"]),

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
                    }
                });
            }

            return lista;
        }

        public RecorridoBE? ObtenerActivoPorConductor(Guid idConductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Recorrido_ObtenerActivoPorConductor", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read())
                return null;

            return new RecorridoBE
            {
                IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                Fecha = Convert.ToDateTime(dr["Fecha"]),
                EstadoRecorrido = dr["EstadoRecorrido"]?.ToString() ?? "PROGRAMADO",
                Estado = Convert.ToBoolean(dr["Estado"]),

                Bus = new BusBE
                {
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                    Placa = dr["Placa"]?.ToString() ?? string.Empty
                },

                Ruta = new RutaBE
                {
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                    Nombre = dr["NombreRuta"]?.ToString() ?? string.Empty
                }
            };
        }

        public List<RecorridoBE> ListarHoyPorConductor(Guid idConductor, DateTime fecha)
        {
            List<RecorridoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarRecorridosHoy", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@IdConductor", SqlDbType.UniqueIdentifier).Value = idConductor;
            cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                RecorridoBE recorrido = new RecorridoBE
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
                };

                if (ExisteColumna(dr, "Marca"))
                    recorrido.Bus.Marca = dr["Marca"] == DBNull.Value ? null : dr["Marca"].ToString();

                if (ExisteColumna(dr, "Modelo"))
                    recorrido.Bus.Modelo = dr["Modelo"] == DBNull.Value ? null : dr["Modelo"].ToString();

                if (ExisteColumna(dr, "Color"))
                    recorrido.Bus.Color = dr["Color"] == DBNull.Value ? null : dr["Color"].ToString();

                if (ExisteColumna(dr, "Anio"))
                    recorrido.Bus.Anio = dr["Anio"] == DBNull.Value ? null : Convert.ToInt32(dr["Anio"]);

                if (ExisteColumna(dr, "Capacidad"))
                    recorrido.Bus.Capacidad = dr["Capacidad"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Capacidad"]);

                if (ExisteColumna(dr, "EstadoOperacion"))
                    recorrido.Bus.EstadoOperacion = dr["EstadoOperacion"]?.ToString() ?? "ACTIVO";

                if (ExisteColumna(dr, "SeguroVigente"))
                    recorrido.Bus.SeguroVigente = dr["SeguroVigente"] != DBNull.Value && Convert.ToBoolean(dr["SeguroVigente"]);

                if (ExisteColumna(dr, "FechaVencimientoSOAT"))
                    recorrido.Bus.FechaVencimientoSOAT = dr["FechaVencimientoSOAT"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaVencimientoSOAT"]);

                if (ExisteColumna(dr, "FechaRevisionTecnica"))
                    recorrido.Bus.FechaRevisionTecnica = dr["FechaRevisionTecnica"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaRevisionTecnica"]);

                lista.Add(recorrido);
            }

            return lista;
        }

        public List<EstudianteRutaEstadoBE> ListarEstudiantesPorRecorridoConductor(Guid idRecorrido)
        {
            List<EstudianteRutaEstadoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarEstudiantesPorRecorrido", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@IdRecorrido", SqlDbType.UniqueIdentifier).Value = idRecorrido;

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                EstudianteRutaEstadoBE estudiante = new EstudianteRutaEstadoBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty,
                    Grado = dr["Grado"] == DBNull.Value ? null : dr["Grado"].ToString(),
                    Seccion = dr["Seccion"] == DBNull.Value ? null : dr["Seccion"].ToString(),
                    EstadoEvento = dr["EstadoEvento"] == DBNull.Value ? "PENDIENTE" : dr["EstadoEvento"].ToString() ?? "PENDIENTE"
                };

                if (ExisteColumna(dr, "IdPadre"))
                    estudiante.IdPadre = dr["IdPadre"] == DBNull.Value ? Guid.Empty : dr.GetGuid(dr.GetOrdinal("IdPadre"));

                if (ExisteColumna(dr, "CodigoEstudiante"))
                    estudiante.CodigoEstudiante = dr["CodigoEstudiante"]?.ToString() ?? string.Empty;

                if (ExisteColumna(dr, "DNI"))
                    estudiante.DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString();

                if (ExisteColumna(dr, "FechaNacimiento"))
                    estudiante.FechaNacimiento = dr["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaNacimiento"]);

                if (ExisteColumna(dr, "Genero"))
                    estudiante.Genero = dr["Genero"] == DBNull.Value ? null : dr["Genero"].ToString();

                if (ExisteColumna(dr, "Direccion"))
                    estudiante.Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString();

                if (ExisteColumna(dr, "Estado"))
                    estudiante.Estado = dr["Estado"] != DBNull.Value && Convert.ToBoolean(dr["Estado"]);

                if (ExisteColumna(dr, "NombrePadre"))
                    estudiante.NombrePadre = dr["NombrePadre"] == DBNull.Value ? string.Empty : dr["NombrePadre"].ToString() ?? string.Empty;

                if (ExisteColumna(dr, "TelefonoPadre"))
                    estudiante.TelefonoPadre = dr["TelefonoPadre"] == DBNull.Value ? null : dr["TelefonoPadre"].ToString();

                if (ExisteColumna(dr, "ParaderoSubidaNombre"))
                    estudiante.ParaderoSubidaNombre = dr["ParaderoSubidaNombre"] == DBNull.Value ? null : dr["ParaderoSubidaNombre"].ToString();

                if (ExisteColumna(dr, "ParaderoSubidaDireccion"))
                    estudiante.ParaderoSubidaDireccion = dr["ParaderoSubidaDireccion"] == DBNull.Value ? null : dr["ParaderoSubidaDireccion"].ToString();

                if (ExisteColumna(dr, "ParaderoBajadaNombre"))
                    estudiante.ParaderoBajadaNombre = dr["ParaderoBajadaNombre"] == DBNull.Value ? null : dr["ParaderoBajadaNombre"].ToString();

                if (ExisteColumna(dr, "ParaderoBajadaDireccion"))
                    estudiante.ParaderoBajadaDireccion = dr["ParaderoBajadaDireccion"] == DBNull.Value ? null : dr["ParaderoBajadaDireccion"].ToString();

                lista.Add(estudiante);
            }

            return lista;
        }

        public List<ParaderoConductorBE> ListarParaderosPorRecorrido(Guid idRecorrido, string tipoParadero)
        {
            List<ParaderoConductorBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarParaderosPorRecorrido", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@IdRecorrido", SqlDbType.UniqueIdentifier).Value = idRecorrido;
            cmd.Parameters.Add("@TipoParadero", SqlDbType.VarChar, 10).Value = tipoParadero;

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
                    TotalAlumnos = ExisteColumna(dr, "TotalAlumnos") ? Convert.ToInt32(dr["TotalAlumnos"]) : 0,
                    TotalRegistrados = ExisteColumna(dr, "TotalRegistrados") ? Convert.ToInt32(dr["TotalRegistrados"]) : 0,
                    Completado = ExisteColumna(dr, "Completado") && Convert.ToBoolean(dr["Completado"])
                });
            }

            return lista;
        }

        public List<EstudianteBE> ListarEstudiantesPorParaderoConductor(
            Guid idRecorrido,
            Guid idParadero,
            string tipoParadero)
        {
            List<EstudianteBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarEstudiantesPorParadero", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@IdRecorrido", SqlDbType.UniqueIdentifier).Value = idRecorrido;
            cmd.Parameters.Add("@IdParadero", SqlDbType.UniqueIdentifier).Value = idParadero;
            cmd.Parameters.Add("@TipoParadero", SqlDbType.VarChar, 10).Value = tipoParadero;

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdPadre = dr["IdPadre"] == DBNull.Value ? Guid.Empty : dr.GetGuid(dr.GetOrdinal("IdPadre")),
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
                    Estado = dr["Estado"] != DBNull.Value && Convert.ToBoolean(dr["Estado"])
                });
            }

            return lista;
        }

        public List<IncidenciaBE> ListarIncidenciasPendientesConductor(Guid idRecorrido)
        {
            List<IncidenciaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarIncidenciasPendientes", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@IdRecorrido", SqlDbType.UniqueIdentifier).Value = idRecorrido;

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
                    Estado = dr["Estado"] != DBNull.Value && Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
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
            cmd.Parameters.Add("@IdRecorrido", SqlDbType.UniqueIdentifier).Value = idRecorrido;
            cmd.Parameters.Add("@IdEstudiante", SqlDbType.UniqueIdentifier).Value = idEstudiante;
            cmd.Parameters.Add("@IdParadero", SqlDbType.UniqueIdentifier).Value = idParadero;
            cmd.Parameters.Add("@RegistradoPor", SqlDbType.UniqueIdentifier).Value = registradoPor;
            cmd.Parameters.Add("@TipoEvento", SqlDbType.VarChar, 20).Value = tipoEvento;
            cmd.Parameters.Add("@FechaHora", SqlDbType.DateTime2).Value = DateTime.Now;
            cmd.Parameters.Add("@Observacion", SqlDbType.VarChar, 250).Value = (object?)observacion ?? DBNull.Value;
            cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = true;

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                if (ExisteColumna(dr, "FilasAfectadas"))
                    return Convert.ToInt32(dr["FilasAfectadas"]) > 0;

                return true;
            }

            return false;
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

        private static bool ExisteColumna(SqlDataReader dr, string nombreColumna)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(nombreColumna, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
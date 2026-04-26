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
    }
}

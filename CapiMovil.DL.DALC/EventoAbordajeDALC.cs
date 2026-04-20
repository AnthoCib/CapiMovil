using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class EventoAbordajeDALC : ICrudDALC<EventoAbordajeBE>
    {
        private readonly BDConexion _bdConexion;

        public EventoAbordajeDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<EventoAbordajeBE> Listar()
        {
            List<EventoAbordajeBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EventoAbordajeBE
                {
                    IdEvento = dr.GetGuid(dr.GetOrdinal("IdEvento")),
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdParadero = dr["IdParadero"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    RegistradoPor = dr["RegistradoPor"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("RegistradoPor")),
                    CodigoEvento = dr["CodigoEvento"]?.ToString() ?? string.Empty,
                    TipoEvento = dr["TipoEvento"]?.ToString() ?? "SUBIDA",
                    FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                    Observacion = dr["Observacion"] == DBNull.Value ? null : dr["Observacion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Recorrido = new RecorridoBE
                    {
                        IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                        CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty
                    },
                    Estudiante = new EstudianteBE
                    {
                        IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                        CodigoEstudiante = dr["CodigoEstudiante"]?.ToString() ?? string.Empty,
                        Nombres = dr["NombresEstudiante"]?.ToString() ?? string.Empty,
                        ApellidoPaterno = dr["ApellidoPaternoEstudiante"]?.ToString() ?? string.Empty
                    },
                    Paradero = dr["IdParadero"] == DBNull.Value ? null : new ParaderoBE
                    {
                        IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                        CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreParadero"]?.ToString() ?? string.Empty
                    }
                });
            }

            return lista;
        }

        public EventoAbordajeBE? ListarPorId(Guid id)
        {
            EventoAbordajeBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEvento", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new EventoAbordajeBE
                {
                    IdEvento = dr.GetGuid(dr.GetOrdinal("IdEvento")),
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdParadero = dr["IdParadero"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    RegistradoPor = dr["RegistradoPor"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("RegistradoPor")),
                    CodigoEvento = dr["CodigoEvento"]?.ToString() ?? string.Empty,
                    TipoEvento = dr["TipoEvento"]?.ToString() ?? "SUBIDA",
                    FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                    Observacion = dr["Observacion"] == DBNull.Value ? null : dr["Observacion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"])
                };
            }

            return entidad;
        }

        public bool Registrar(EventoAbordajeBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@IdEstudiante", entidad.IdEstudiante);
            cmd.Parameters.AddWithValue("@IdParadero", (object?)entidad.IdParadero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistradoPor", (object?)entidad.RegistradoPor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoEvento", entidad.TipoEvento);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@Observacion", (object?)entidad.Observacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);
                if (filas > 0)
                {
                    entidad.CodigoEvento = dr["CodigoGenerado"]?.ToString() ?? string.Empty;

                    if (dr["IdGenerado"] != DBNull.Value)
                        entidad.IdEvento = (Guid)dr["IdGenerado"];

                    return true;
                }
            }

            return false;
        }

        public bool Actualizar(EventoAbordajeBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEvento", entidad.IdEvento);
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@IdEstudiante", entidad.IdEstudiante);
            cmd.Parameters.AddWithValue("@IdParadero", (object?)entidad.IdParadero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistradoPor", (object?)entidad.RegistradoPor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoEvento", entidad.TipoEvento);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@Observacion", (object?)entidad.Observacion ?? DBNull.Value);
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
            using SqlCommand cmd = new SqlCommand("sp_EventoAbordaje_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEvento", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }
    }
}
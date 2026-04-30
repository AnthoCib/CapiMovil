using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class NotificacionDALC : ICrudDALC<NotificacionBE>
    {
        private readonly BDConexion _bdConexion;

        public NotificacionDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<NotificacionBE> Listar()
        {
            List<NotificacionBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_Listar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public NotificacionBE? ListarPorId(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdNotificacion", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
                return Mapear(dr);

            return null;
        }

        public bool Registrar(NotificacionBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", entidad.IdPadre);
            cmd.Parameters.AddWithValue("@IdEstudiante", (object?)entidad.IdEstudiante ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Titulo", entidad.Titulo);
            cmd.Parameters.AddWithValue("@Mensaje", entidad.Mensaje);
            cmd.Parameters.AddWithValue("@TipoNotificacion", entidad.TipoNotificacion);
            cmd.Parameters.AddWithValue("@Canal", entidad.Canal);
            cmd.Parameters.AddWithValue("@Leido", entidad.Leido);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                entidad.CodigoNotificacion = codigoGenerado;
                    return true;
            }

            return false;
        }

        public bool Actualizar(NotificacionBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdNotificacion", entidad.IdNotificacion);
            cmd.Parameters.AddWithValue("@IdPadre", entidad.IdPadre);
            cmd.Parameters.AddWithValue("@IdEstudiante", (object?)entidad.IdEstudiante ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Titulo", entidad.Titulo);
            cmd.Parameters.AddWithValue("@Mensaje", entidad.Mensaje);
            cmd.Parameters.AddWithValue("@TipoNotificacion", entidad.TipoNotificacion);
            cmd.Parameters.AddWithValue("@Canal", entidad.Canal);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool Eliminar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdNotificacion", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool MarcarLeida(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_MarcarLeida", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdNotificacion", id);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public List<NotificacionBE> ListarPorPadre(Guid idPadre)
        {
            List<NotificacionBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_ListarPorPadre", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", idPadre);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public List<NotificacionBE> ListarNoLeidasPorPadre(Guid idPadre)
        {
            List<NotificacionBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Notificacion_ListarNoLeidasPorPadre", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", idPadre);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        private NotificacionBE Mapear(SqlDataReader dr)
        {
            return new NotificacionBE
            {
                IdNotificacion = (Guid)dr["IdNotificacion"],
                IdPadre = (Guid)dr["IdPadre"],
                IdEstudiante = dr["IdEstudiante"] == DBNull.Value ? null : (Guid?)dr["IdEstudiante"],
                CodigoNotificacion = dr["CodigoNotificacion"]?.ToString() ?? string.Empty,
                Titulo = dr["Titulo"]?.ToString() ?? string.Empty,
                Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty,
                TipoNotificacion = dr["TipoNotificacion"]?.ToString() ?? string.Empty,
                Canal = dr["Canal"]?.ToString() ?? string.Empty,
                Leido = Convert.ToBoolean(dr["Leido"]),
                FechaLectura = dr["FechaLectura"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaLectura"]),
                FechaEnvio = Convert.ToDateTime(dr["FechaEnvio"]),
                Estado = Convert.ToBoolean(dr["Estado"]),
                FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                NombrePadre = ExisteColumna(dr, "NombrePadre") && dr["NombrePadre"] != DBNull.Value ? dr["NombrePadre"].ToString() : null,
                NombreEstudiante = ExisteColumna(dr, "NombreEstudiante") && dr["NombreEstudiante"] != DBNull.Value ? dr["NombreEstudiante"].ToString() : null,
                CodigoEstudiante = ExisteColumna(dr, "CodigoEstudiante") && dr["CodigoEstudiante"] != DBNull.Value ? dr["CodigoEstudiante"].ToString() : null
            };
        }

        private bool ExisteColumna(SqlDataReader dr, string nombre)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

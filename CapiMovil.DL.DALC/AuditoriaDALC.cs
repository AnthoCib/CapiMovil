using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class AuditoriaDALC
    {
        private readonly BDConexion _bdConexion;

        public AuditoriaDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<AuditoriaBE> Listar()
        {
            List<AuditoriaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_Listar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public AuditoriaBE? ListarPorId(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_ObtenerPorId", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdAuditoria", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
                return Mapear(dr);

            return null;
        }

        public bool Registrar(AuditoriaBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_Registrar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Tabla", entidad.Tabla);
            cmd.Parameters.AddWithValue("@IdRegistro", (object?)entidad.IdRegistro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Accion", entidad.Accion);
            cmd.Parameters.AddWithValue("@DatosAntes", (object?)entidad.DatosAntes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DatosDespues", (object?)entidad.DatosDespues ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UsuarioId", (object?)entidad.UsuarioId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NombreUsuario", (object?)entidad.NombreUsuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ip", (object?)entidad.Ip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserAgent", (object?)entidad.UserAgent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Modulo", (object?)entidad.Modulo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Observacion", (object?)entidad.Observacion ?? DBNull.Value);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                if (!string.IsNullOrWhiteSpace(codigoGenerado) &&
                    !System.Text.RegularExpressions.Regex.IsMatch(codigoGenerado, @"^AUD-\d{4}-\d{6}$"))
                {
                    throw new InvalidOperationException($"Formato de código de auditoría inválido: {codigoGenerado}");
                }

                entidad.CodigoAuditoria = codigoGenerado;
                    return true;
            }

            return false;
        }

        public List<AuditoriaBE> ListarPorTabla(string tabla)
        {
            List<AuditoriaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_ListarPorTabla", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Tabla", tabla);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public List<AuditoriaBE> ListarPorAccion(string accion)
        {
            List<AuditoriaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_ListarPorAccion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Accion", accion);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public List<AuditoriaBE> ListarPorUsuario(Guid usuarioId)
        {
            List<AuditoriaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Auditoria_ListarPorUsuario", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        private AuditoriaBE Mapear(SqlDataReader dr)
        {
            return new AuditoriaBE
            {
                IdAuditoria = (Guid)dr["IdAuditoria"],
                CodigoAuditoria = dr["CodigoAuditoria"]?.ToString() ?? string.Empty,
                Tabla = dr["Tabla"]?.ToString() ?? string.Empty,
                IdRegistro = dr["IdRegistro"] == DBNull.Value ? null : (Guid?)dr["IdRegistro"],
                Accion = dr["Accion"]?.ToString() ?? string.Empty,
                DatosAntes = dr["DatosAntes"] == DBNull.Value ? null : dr["DatosAntes"].ToString(),
                DatosDespues = dr["DatosDespues"] == DBNull.Value ? null : dr["DatosDespues"].ToString(),
                UsuarioId = dr["UsuarioId"] == DBNull.Value ? null : (Guid?)dr["UsuarioId"],
                NombreUsuario = dr["NombreUsuario"] == DBNull.Value ? null : dr["NombreUsuario"].ToString(),
                Ip = dr["Ip"] == DBNull.Value ? null : dr["Ip"].ToString(),
                UserAgent = dr["UserAgent"] == DBNull.Value ? null : dr["UserAgent"].ToString(),
                Modulo = dr["Modulo"] == DBNull.Value ? null : dr["Modulo"].ToString(),
                Observacion = dr["Observacion"] == DBNull.Value ? null : dr["Observacion"].ToString(),
                Fecha = Convert.ToDateTime(dr["Fecha"])
            };
        }
    }
}

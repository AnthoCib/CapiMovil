using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class UsuarioDALC : ICrudDALC<UsuarioBE>
    {
        private readonly BDConexion _bdConexion;

        public UsuarioDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<UsuarioBE> Listar()
        {
            List<UsuarioBE> lista = new List<UsuarioBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new UsuarioBE
                {
                    IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                    IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                    CodigoUsuario = dr["CodigoUsuario"].ToString() ?? string.Empty,
                    Username = dr["Username"].ToString() ?? string.Empty,
                    Correo = dr["Correo"].ToString() ?? string.Empty,
                    PasswordHash = dr["PasswordHash"].ToString() ?? string.Empty,
                    FotoPerfilUrl = ExisteColumna(dr, "FotoPerfilUrl") && dr["FotoPerfilUrl"] != DBNull.Value
                        ? dr["FotoPerfilUrl"].ToString()
                        : null,
                    UltimoAcceso = dr["UltimoAcceso"] == DBNull.Value ? null : Convert.ToDateTime(dr["UltimoAcceso"]),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Rol = new RolBE
                    {
                        IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                        CodigoRol = dr["CodigoRol"].ToString() ?? string.Empty,
                        Nombre = dr["NombreRol"].ToString() ?? string.Empty

                    }
                });
            }

            return lista;
        }

        public UsuarioBE? ListarPorId(Guid idUsuario)
        {
            UsuarioBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new UsuarioBE
                {
                    IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                    IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                    CodigoUsuario = dr["CodigoUsuario"].ToString() ?? string.Empty,
                    Username = dr["Username"].ToString() ?? string.Empty,
                    Correo = dr["Correo"].ToString() ?? string.Empty,
                    PasswordHash = dr["PasswordHash"].ToString() ?? string.Empty,
                    FotoPerfilUrl = ExisteColumna(dr, "FotoPerfilUrl") && dr["FotoPerfilUrl"] != DBNull.Value
                        ? dr["FotoPerfilUrl"].ToString()
                        : null,
                    UltimoAcceso = dr["UltimoAcceso"] == DBNull.Value ? null : Convert.ToDateTime(dr["UltimoAcceso"]),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Rol = new RolBE
                    {
                        IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                        CodigoRol = dr["CodigoRol"].ToString() ?? string.Empty,
                        Nombre = dr["NombreRol"].ToString() ?? string.Empty

                    }
                };
            }

            return entidad;
        }

        public bool Registrar(UsuarioBE usuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
            cmd.Parameters.AddWithValue("@Username", usuario.Username);
            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
            cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);

                if (filas > 0)
                {
                    usuario.CodigoUsuario = dr["CodigoGenerado"]?.ToString() ?? string.Empty;
                    return true;
                }
            }

            return false;
        }

        public bool Actualizar(UsuarioBE usuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
            cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
            cmd.Parameters.AddWithValue("@Username", usuario.Username);
            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }
        public bool CambiarPassword(Guid idUsuario, string passwordHash)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_CambiarPassword", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool ActualizarUltimoAcceso(Guid idUsuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_ActualizarUltimoAcceso", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public bool Eliminar(Guid idUsuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public UsuarioBE? ObtenerPorUsuarioOCorreo(string usuarioOCorreo)
        {
            UsuarioBE? usuario = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_Login", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UsuarioOCorreo", usuarioOCorreo);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                usuario = new UsuarioBE
                {
                    IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                    IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                    CodigoUsuario = dr["CodigoUsuario"]?.ToString() ?? string.Empty,
                    Username = dr["Username"]?.ToString() ?? string.Empty,
                    Correo = dr["Correo"]?.ToString() ?? string.Empty,
                    PasswordHash = dr["PasswordHash"]?.ToString() ?? string.Empty,
                    FotoPerfilUrl = ExisteColumna(dr, "FotoPerfilUrl") && dr["FotoPerfilUrl"] != DBNull.Value
                        ? dr["FotoPerfilUrl"].ToString()
                        : null,
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    Rol = new RolBE
                    {
                        IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                        CodigoRol = dr["CodigoRol"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreRol"]?.ToString() ?? string.Empty

                    }
                };
            }

            return usuario;
        }

        public bool ActualizarFotoPerfil(Guid idUsuario, string? fotoPerfilUrl)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_ActualizarFotoPerfil", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@FotoPerfilUrl", (object?)fotoPerfilUrl ?? DBNull.Value);

            cn.Open();
            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
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

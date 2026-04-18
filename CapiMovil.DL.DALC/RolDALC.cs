using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class RolDALC : ICrudDALC<RolBE>
    {
        private readonly BDConexion _bdConexion;

        public RolDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<RolBE> Listar()
        {
            List<RolBE> lista = new List<RolBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Rol_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RolBE
                {
                    IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                    CodigoRol = dr["CodigoRol"].ToString() ?? string.Empty,
                    Nombre = dr["Nombre"].ToString() ?? string.Empty,
                    Descripcion = dr["Descripcion"] == DBNull.Value ? null : dr["Descripcion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public RolBE? ListarPorId(Guid idRol)
        {
            RolBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Rol_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRol", idRol);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new RolBE
                {
                    IdRol = dr.GetGuid(dr.GetOrdinal("IdRol")),
                    CodigoRol = dr["CodigoRol"].ToString() ?? string.Empty,
                    Nombre = dr["Nombre"].ToString() ?? string.Empty,
                    Descripcion = dr["Descripcion"] == DBNull.Value ? null : dr["Descripcion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(RolBE rol)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Rol_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CodigoRol", rol.CodigoRol);
            cmd.Parameters.AddWithValue("@Nombre", rol.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)rol.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", rol.Estado);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }
            return false;
        }

        public bool Actualizar(RolBE rol)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Rol_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRol", rol.IdRol);
            cmd.Parameters.AddWithValue("@CodigoRol", rol.CodigoRol);
            cmd.Parameters.AddWithValue("@Nombre", rol.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)rol.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", rol.Estado);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public bool Eliminar(Guid idRol)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Rol_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRol", idRol);

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
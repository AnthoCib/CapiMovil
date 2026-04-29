using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class ConductorDALC :ICrudDALC<ConductorBE>
    {
        private readonly BDConexion _bdConexion;

        public ConductorDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<ConductorBE> Listar()
        {
            List<ConductorBE> lista = new List<ConductorBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ConductorBE
                {
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    IdUsuario = dr["IdUsuario"] != DBNull.Value ? Guid.Parse(dr["IdUsuario"].ToString()!) : Guid.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    Licencia = dr["Licencia"].ToString() ?? string.Empty,
                    CategoriaLicencia = dr["CategoriaLicencia"] == DBNull.Value ? null : dr["CategoriaLicencia"].ToString(),
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public ConductorBE? ListarPorId(Guid idConductor)
        {
            ConductorBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new ConductorBE
                {
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),  
                    IdUsuario = dr["IdUsuario"] != DBNull.Value ? Guid.Parse(dr["IdUsuario"].ToString()!) : Guid.Empty,
                    CodigoConductor = dr["CodigoConductor"].ToString() ?? string.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    Licencia = dr["Licencia"].ToString() ?? string.Empty,
                    CategoriaLicencia = dr["CategoriaLicencia"] == DBNull.Value ? null : dr["CategoriaLicencia"].ToString(),
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(ConductorBE conductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", (object?)conductor.IdUsuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CodigoConductor", conductor.CodigoConductor);
            cmd.Parameters.AddWithValue("@Nombres", conductor.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", conductor.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", conductor.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", (object?)conductor.DNI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Licencia", conductor.Licencia);
            cmd.Parameters.AddWithValue("@CategoriaLicencia", (object?)conductor.CategoriaLicencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)conductor.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)conductor.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", conductor.Estado);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public bool Actualizar(ConductorBE conductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", conductor.IdConductor);
            cmd.Parameters.AddWithValue("@IdUsuario", (object?)conductor.IdUsuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CodigoConductor", conductor.CodigoConductor);
            cmd.Parameters.AddWithValue("@Nombres", conductor.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", conductor.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", conductor.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", (object?)conductor.DNI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Licencia", conductor.Licencia);
            cmd.Parameters.AddWithValue("@CategoriaLicencia", (object?)conductor.CategoriaLicencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)conductor.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)conductor.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", conductor.Estado);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Eliminar(Guid idConductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            List<UsuarioBE> lista = new List<UsuarioBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_ListarDisponiblesParaConductor", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                UsuarioBE usuario = new UsuarioBE
                {
                    IdUsuario = dr["IdUsuario"] != DBNull.Value ? Guid.Parse(dr["IdUsuario"].ToString()!) : Guid.Empty,
                    Username = dr["Username"]?.ToString(),
                    Correo = dr["Correo"]?.ToString(),
                    Estado = dr["Estado"] != DBNull.Value && Convert.ToBoolean(dr["Estado"])
                };

                lista.Add(usuario);
            }

            return lista;
        }
        public bool ExistePorIdUsuario(Guid idUsuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ExistePorIdUsuario", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public List<ConductorBE> ListarActivos()
        {
            List<ConductorBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ListarActivos", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ConductorBE
                {
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    CodigoConductor = dr["CodigoConductor"]?.ToString() ?? string.Empty,
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty
                });
            }

            return lista;
        }

        public ConductorBE? ObtenerPorIdUsuario(Guid idUsuario)
        {
            ConductorBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Conductor_ObtenerPorIdUsuario", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new ConductorBE
                {
                    IdConductor = dr.GetGuid(dr.GetOrdinal("IdConductor")),
                    IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                    CodigoConductor = dr["CodigoConductor"]?.ToString() ?? string.Empty,
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    Licencia = dr["Licencia"]?.ToString() ?? string.Empty,
                    CategoriaLicencia = dr["CategoriaLicencia"] == DBNull.Value ? null : dr["CategoriaLicencia"].ToString(),
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }
    }
}
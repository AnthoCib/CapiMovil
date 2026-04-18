using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class PadreFamiliaDALC : ICrudDALC<PadreFamiliaBE>
    {
        private readonly BDConexion _bdConexion;

        public PadreFamiliaDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<PadreFamiliaBE> Listar()
        {
            List<PadreFamiliaBE> lista = new List<PadreFamiliaBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new PadreFamiliaBE
                {
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    IdUsuario = dr["IdUsuario"] != DBNull.Value ? Guid.Parse(dr["IdUsuario"].ToString()!) : Guid.Empty,
                    CodigoPadre = dr["CodigoPadre"].ToString() ?? string.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    TelefonoAlterno = dr["TelefonoAlterno"] == DBNull.Value ? null : dr["TelefonoAlterno"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    CorreoContacto = dr["CorreoContacto"] == DBNull.Value ? null : dr["CorreoContacto"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public PadreFamiliaBE? ListarPorId(Guid idPadre)
        {
            PadreFamiliaBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", idPadre);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new PadreFamiliaBE
                {
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    IdUsuario = dr["IdUsuario"] != DBNull.Value ? Guid.Parse(dr["IdUsuario"].ToString()!) : Guid.Empty,
                    CodigoPadre = dr["CodigoPadre"].ToString() ?? string.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    TelefonoAlterno = dr["TelefonoAlterno"] == DBNull.Value ? null : dr["TelefonoAlterno"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    CorreoContacto = dr["CorreoContacto"] == DBNull.Value ? null : dr["CorreoContacto"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(PadreFamiliaBE padre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdUsuario", (object?)padre.IdUsuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombres", padre.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", padre.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", padre.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", string.IsNullOrWhiteSpace(padre.DNI) ? DBNull.Value : padre.DNI);
            cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(padre.Telefono) ? DBNull.Value : padre.Telefono);
            cmd.Parameters.AddWithValue("@TelefonoAlterno", string.IsNullOrWhiteSpace(padre.TelefonoAlterno) ? DBNull.Value : padre.TelefonoAlterno);
            cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrWhiteSpace(padre.Direccion) ? DBNull.Value : padre.Direccion);
            cmd.Parameters.AddWithValue("@CorreoContacto", string.IsNullOrWhiteSpace(padre.CorreoContacto) ? DBNull.Value : padre.CorreoContacto);
            cmd.Parameters.AddWithValue("@Estado", padre.Estado);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);

                if (filas > 0)
                {
                    padre.CodigoPadre = dr["CodigoGenerado"]?.ToString() ?? string.Empty;
                    return true;
                }
            }

            return false;
        }

        public bool Actualizar(PadreFamiliaBE padre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", padre.IdPadre);
            cmd.Parameters.AddWithValue("@IdUsuario", (object?)padre.IdUsuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CodigoPadre", padre.CodigoPadre);
            cmd.Parameters.AddWithValue("@Nombres", padre.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", padre.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", padre.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", (object?)padre.DNI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)padre.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TelefonoAlterno", (object?)padre.TelefonoAlterno ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)padre.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CorreoContacto", (object?)padre.CorreoContacto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", padre.Estado);

            cn.Open();

            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Eliminar(Guid idPadre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", idPadre);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }


        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            List<UsuarioBE> lista = new List<UsuarioBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Usuario_ListarDisponiblesParaPadre", cn);

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
            using SqlCommand cmd = new SqlCommand("sp_PadreFamilia_ExistePorIdUsuario", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

     
    }
}
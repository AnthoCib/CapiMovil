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
            List<PadreFamiliaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_Listar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(MapearPadreFamilia(dr));
            }

            return lista;
        }

        public PadreFamiliaBE? ListarPorId(Guid idPadre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_ListarPorId", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdPadre", SqlDbType.UniqueIdentifier).Value = idPadre;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            return dr.Read() ? MapearPadreFamilia(dr) : null;
        }

        public bool Registrar(PadreFamiliaBE padre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_Registrar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = padre.IdUsuario;
            cmd.Parameters.Add("@Nombres", SqlDbType.VarChar, 80).Value = padre.Nombres;
            cmd.Parameters.Add("@ApellidoPaterno", SqlDbType.VarChar, 60).Value = padre.ApellidoPaterno;
            cmd.Parameters.Add("@ApellidoMaterno", SqlDbType.VarChar, 60).Value = padre.ApellidoMaterno;
            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = (object?)padre.DNI ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 20).Value = (object?)padre.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@TelefonoAlterno", SqlDbType.VarChar, 20).Value = (object?)padre.TelefonoAlterno ?? DBNull.Value;
            cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = (object?)padre.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@CorreoContacto", SqlDbType.VarChar, 120).Value = (object?)padre.CorreoContacto ?? DBNull.Value;
            cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = padre.Estado;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                padre.CodigoPadre = codigoGenerado;
                return true;
            }

            return false;
        }

        public bool Actualizar(PadreFamiliaBE padre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_Actualizar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdPadre", SqlDbType.UniqueIdentifier).Value = padre.IdPadre;
            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = padre.IdUsuario;
            cmd.Parameters.Add("@Nombres", SqlDbType.VarChar, 80).Value = padre.Nombres;
            cmd.Parameters.Add("@ApellidoPaterno", SqlDbType.VarChar, 60).Value = padre.ApellidoPaterno;
            cmd.Parameters.Add("@ApellidoMaterno", SqlDbType.VarChar, 60).Value = padre.ApellidoMaterno;
            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = (object?)padre.DNI ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 20).Value = (object?)padre.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@TelefonoAlterno", SqlDbType.VarChar, 20).Value = (object?)padre.TelefonoAlterno ?? DBNull.Value;
            cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = (object?)padre.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@CorreoContacto", SqlDbType.VarChar, 120).Value = (object?)padre.CorreoContacto ?? DBNull.Value;
            cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = padre.Estado;

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public bool Eliminar(Guid idPadre)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_Eliminar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdPadre", SqlDbType.UniqueIdentifier).Value = idPadre;

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            List<UsuarioBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Usuario_ListarDisponiblesParaPadre", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new UsuarioBE
                {
                    IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                    Username = dr["Username"]?.ToString(),
                    Correo = dr["Correo"]?.ToString(),
                    Estado = dr["Estado"] != DBNull.Value && Convert.ToBoolean(dr["Estado"])
                });
            }

            return lista;
        }

        public List<PadreFamiliaBE> ListarParaCombo()
        {
            List<PadreFamiliaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_ListarParaCombo", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                PadreFamiliaBE padre = new()
                {
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    CodigoPadre = dr["CodigoPadre"]?.ToString() ?? string.Empty,
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty
                };

                if (ExisteColumna(dr, "Username") || ExisteColumna(dr, "Correo"))
                {
                    padre.Usuario = new UsuarioBE
                    {
                        Username = ExisteColumna(dr, "Username") ? dr["Username"]?.ToString() ?? string.Empty : string.Empty,
                        Correo = ExisteColumna(dr, "Correo") ? dr["Correo"]?.ToString() ?? string.Empty : string.Empty
                    };
                }

                lista.Add(padre);
            }

            return lista;
        }


        public PadreFamiliaBE? ObtenerPorIdUsuario(Guid idUsuario)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_ListarPorIdUsuario", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = idUsuario;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            return dr.Read() ? MapearPadreFamilia(dr) : null;
        }

        public bool ExistePorIdUsuario(Guid idUsuario)
            => ExistePorIdUsuario(idUsuario, null);

        public bool ExistePorIdUsuario(Guid idUsuario, Guid? idPadreExcluir)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_ExistePorIdUsuario", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = idUsuario;
            cmd.Parameters.Add("@IdPadreExcluir", SqlDbType.UniqueIdentifier).Value = (object?)idPadreExcluir ?? DBNull.Value;

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool ExistePorDni(string dni)
            => ExistePorDni(dni, null);

        public bool ExistePorDni(string dni, Guid? idPadreExcluir)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_PadreFamilia_ExistePorDni", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = dni.Trim();
            cmd.Parameters.Add("@IdPadreExcluir", SqlDbType.UniqueIdentifier).Value = (object?)idPadreExcluir ?? DBNull.Value;

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        private static PadreFamiliaBE MapearPadreFamilia(SqlDataReader dr)
        {
            return new PadreFamiliaBE
            {
                IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                IdUsuario = dr.GetGuid(dr.GetOrdinal("IdUsuario")),
                CodigoPadre = dr["CodigoPadre"]?.ToString() ?? string.Empty,
                Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty,
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

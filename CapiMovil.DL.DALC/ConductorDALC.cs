using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class ConductorDALC : ICrudDALC<ConductorBE>
    {
        private readonly BDConexion _bdConexion;

        public ConductorDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<ConductorBE> Listar()
        {
            List<ConductorBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_Listar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(MapearConductor(dr));
            }

            return lista;
        }

        public ConductorBE? ListarPorId(Guid idConductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ListarPorId", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdConductor", SqlDbType.UniqueIdentifier).Value = idConductor;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            return dr.Read() ? MapearConductor(dr) : null;
        }

        public bool Registrar(ConductorBE conductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_Registrar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = conductor.IdUsuario;
            cmd.Parameters.Add("@Nombres", SqlDbType.VarChar, 80).Value = conductor.Nombres;
            cmd.Parameters.Add("@ApellidoPaterno", SqlDbType.VarChar, 60).Value = conductor.ApellidoPaterno;
            cmd.Parameters.Add("@ApellidoMaterno", SqlDbType.VarChar, 60).Value = conductor.ApellidoMaterno;
            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = (object?)conductor.DNI ?? DBNull.Value;
            cmd.Parameters.Add("@Licencia", SqlDbType.VarChar, 30).Value = conductor.Licencia;
            cmd.Parameters.Add("@CategoriaLicencia", SqlDbType.VarChar, 10).Value = (object?)conductor.CategoriaLicencia ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 20).Value = (object?)conductor.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = (object?)conductor.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = conductor.Estado;
            SqlParameter codigoOutput = cmd.Parameters.Add("@CodigoGenerado", SqlDbType.VarChar, 20);
            codigoOutput.Direction = ParameterDirection.Output;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                if (string.IsNullOrWhiteSpace(codigoGenerado))
                    codigoGenerado = codigoOutput.Value?.ToString() ?? string.Empty;

                conductor.CodigoConductor = codigoGenerado;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

            return false;
        }

        public bool Actualizar(ConductorBE conductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_Actualizar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdConductor", SqlDbType.UniqueIdentifier).Value = conductor.IdConductor;
            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = conductor.IdUsuario;
            cmd.Parameters.Add("@Nombres", SqlDbType.VarChar, 80).Value = conductor.Nombres;
            cmd.Parameters.Add("@ApellidoPaterno", SqlDbType.VarChar, 60).Value = conductor.ApellidoPaterno;
            cmd.Parameters.Add("@ApellidoMaterno", SqlDbType.VarChar, 60).Value = conductor.ApellidoMaterno;
            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = (object?)conductor.DNI ?? DBNull.Value;
            cmd.Parameters.Add("@Licencia", SqlDbType.VarChar, 30).Value = conductor.Licencia;
            cmd.Parameters.Add("@CategoriaLicencia", SqlDbType.VarChar, 10).Value = (object?)conductor.CategoriaLicencia ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 20).Value = (object?)conductor.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = (object?)conductor.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = conductor.Estado;

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public bool Eliminar(Guid idConductor)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_Eliminar", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdConductor", SqlDbType.UniqueIdentifier).Value = idConductor;

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            List<UsuarioBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Usuario_ListarDisponiblesParaConductor", cn)
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

        public bool ExistePorIdUsuario(Guid idUsuario)
            => ExistePorIdUsuario(idUsuario, null);

        public bool ExistePorIdUsuario(Guid idUsuario, Guid? idConductorExcluir)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ExistePorIdUsuario", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = idUsuario;
            cmd.Parameters.Add("@IdConductorExcluir", SqlDbType.UniqueIdentifier).Value = (object?)idConductorExcluir ?? DBNull.Value;

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool ExistePorDni(string dni)
            => ExistePorDni(dni, null);

        public bool ExistePorDni(string dni, Guid? idConductorExcluir)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ExistePorDni", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@DNI", SqlDbType.VarChar, 8).Value = dni.Trim();
            cmd.Parameters.Add("@IdConductorExcluir", SqlDbType.UniqueIdentifier).Value = (object?)idConductorExcluir ?? DBNull.Value;

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool ExistePorLicencia(string licencia)
            => ExistePorLicencia(licencia, null);

        public bool ExistePorLicencia(string licencia, Guid? idConductorExcluir)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ExistePorLicencia", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Licencia", SqlDbType.VarChar, 30).Value = licencia.Trim().ToUpperInvariant();
            cmd.Parameters.Add("@IdConductorExcluir", SqlDbType.UniqueIdentifier).Value = (object?)idConductorExcluir ?? DBNull.Value;

            cn.Open();

            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public List<ConductorBE> ListarActivos()
        {
            List<ConductorBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ListarActivos", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

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
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new("sp_Conductor_ListarPorIdUsuario", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.UniqueIdentifier).Value = idUsuario;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            return dr.Read() ? MapearConductor(dr) : null;
        }

        private static ConductorBE MapearConductor(SqlDataReader dr)
        {
            return new ConductorBE
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
    }
}

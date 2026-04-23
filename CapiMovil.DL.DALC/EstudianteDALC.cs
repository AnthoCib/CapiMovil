using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class EstudianteDALC:ICrudDALC<EstudianteBE>
    {
        private readonly BDConexion _bdConexion;

        public EstudianteDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }


        public List<EstudianteBE> Listar()
        {
            List<EstudianteBE> lista = new List<EstudianteBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    CodigoEstudiante = ExisteColumna(dr, "CodigoEstudiante") && dr["CodigoEstudiante"] != DBNull.Value
                        ? dr["CodigoEstudiante"].ToString() ?? string.Empty
                        : string.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    FechaNacimiento = dr["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaNacimiento"]),
                    Genero = dr["Genero"] == DBNull.Value ? null : dr["Genero"].ToString(),
                    Grado = dr["Grado"] == DBNull.Value ? null : dr["Grado"].ToString(),
                    Seccion = dr["Seccion"] == DBNull.Value ? null : dr["Seccion"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    LatitudCasa = dr["LatitudCasa"] == DBNull.Value ? null : Convert.ToDecimal(dr["LatitudCasa"]),
                    LongitudCasa = dr["LongitudCasa"] == DBNull.Value ? null : Convert.ToDecimal(dr["LongitudCasa"]),
                    FotoUrl = dr["FotoUrl"] == DBNull.Value ? null : dr["FotoUrl"].ToString(),
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public EstudianteBE? ListarPorId(Guid idEstudiante)
        {
            EstudianteBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEstudiante", idEstudiante);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdPadre = dr.GetGuid(dr.GetOrdinal("IdPadre")),
                    CodigoEstudiante = dr["CodigoEstudiante"].ToString() ?? string.Empty,
                    Nombres = dr["Nombres"].ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"].ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"].ToString() ?? string.Empty,
                    DNI = dr["DNI"] == DBNull.Value ? null : dr["DNI"].ToString(),
                    FechaNacimiento = dr["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaNacimiento"]),
                    Genero = dr["Genero"] == DBNull.Value ? null : dr["Genero"].ToString(),
                    Grado = dr["Grado"] == DBNull.Value ? null : dr["Grado"].ToString(),
                    Seccion = dr["Seccion"] == DBNull.Value ? null : dr["Seccion"].ToString(),
                    Direccion = dr["Direccion"] == DBNull.Value ? null : dr["Direccion"].ToString(),
                    LatitudCasa = dr["LatitudCasa"] == DBNull.Value ? null : Convert.ToDecimal(dr["LatitudCasa"]),
                    LongitudCasa = dr["LongitudCasa"] == DBNull.Value ? null : Convert.ToDecimal(dr["LongitudCasa"]),
                    FotoUrl = dr["FotoUrl"] == DBNull.Value ? null : dr["FotoUrl"].ToString(),
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(EstudianteBE estudiante)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", estudiante.IdPadre);
            cmd.Parameters.AddWithValue("@CodigoEstudiante", DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombres", estudiante.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", estudiante.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", estudiante.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", (object?)estudiante.DNI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaNacimiento", (object?)estudiante.FechaNacimiento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Genero", (object?)estudiante.Genero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Grado", (object?)estudiante.Grado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Seccion", (object?)estudiante.Seccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)estudiante.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LatitudCasa", (object?)estudiante.LatitudCasa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LongitudCasa", (object?)estudiante.LongitudCasa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FotoUrl", (object?)estudiante.FotoUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)estudiante.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", estudiante.Estado);
            SqlParameter codigoOutput = cmd.Parameters.Add("@CodigoGenerado", SqlDbType.VarChar, 20);
            codigoOutput.Direction = ParameterDirection.Output;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);
                estudiante.CodigoEstudiante = dr["CodigoGenerado"]?.ToString()
                    ?? codigoOutput.Value?.ToString()
                    ?? string.Empty;
                return filas > 0;
            }

            return false;
        }

        public bool Actualizar(EstudianteBE estudiante)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEstudiante", estudiante.IdEstudiante);
            cmd.Parameters.AddWithValue("@IdPadre", estudiante.IdPadre);
            cmd.Parameters.AddWithValue("@CodigoEstudiante", estudiante.CodigoEstudiante);
            cmd.Parameters.AddWithValue("@Nombres", estudiante.Nombres);
            cmd.Parameters.AddWithValue("@ApellidoPaterno", estudiante.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ApellidoMaterno", estudiante.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@DNI", (object?)estudiante.DNI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaNacimiento", (object?)estudiante.FechaNacimiento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Genero", (object?)estudiante.Genero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Grado", (object?)estudiante.Grado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Seccion", (object?)estudiante.Seccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)estudiante.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LatitudCasa", (object?)estudiante.LatitudCasa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LongitudCasa", (object?)estudiante.LongitudCasa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FotoUrl", (object?)estudiante.FotoUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)estudiante.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", estudiante.Estado);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Eliminar(Guid idEstudiante)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEstudiante", idEstudiante);

            cn.Open();
            int filas = cmd.ExecuteNonQuery();

            return filas > 0;
        }

        public List<EstudianteBE> ListarActivos()
        {
            List<EstudianteBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_ListarActivos", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new EstudianteBE
                {
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    CodigoEstudiante = dr["CodigoEstudiante"]?.ToString() ?? string.Empty,
                    Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                    ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                    ApellidoMaterno = dr["ApellidoMaterno"]?.ToString() ?? string.Empty
                });
            }

            return lista;
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

        public Guid ObtenerPadrePorEstudiante(Guid idEstudiante)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Estudiante_ObtenerPadrePorEstudiante", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEstudiante", idEstudiante);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return Guid.Empty;

            return (Guid)result;
        }
    }
}

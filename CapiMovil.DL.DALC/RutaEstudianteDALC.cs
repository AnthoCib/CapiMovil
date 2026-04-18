using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class RutaEstudianteDALC : ICrudDALC<RutaEstudianteBE>
    {
        private readonly BDConexion _bdConexion;

        public RutaEstudianteDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<RutaEstudianteBE> Listar()
        {
            List<RutaEstudianteBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_RutaEstudiante_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RutaEstudianteBE
                {
                    IdRutaEstudiante = dr.GetGuid(dr.GetOrdinal("IdRutaEstudiante")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdParaderoSubida = dr["IdParaderoSubida"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParaderoSubida")),
                    IdParaderoBajada = dr["IdParaderoBajada"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParaderoBajada")),
                    CodigoRutaEstudiante = dr["CodigoRutaEstudiante"]?.ToString() ?? string.Empty,
                    FechaInicioVigencia = Convert.ToDateTime(dr["FechaInicioVigencia"]),
                    FechaFinVigencia = dr["FechaFinVigencia"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaFinVigencia"]),
                    EstadoAsignacion = dr["EstadoAsignacion"]?.ToString() ?? "ACTIVO",
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Ruta = new RutaBE
                    {
                        IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                        CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreRuta"]?.ToString() ?? string.Empty
                    },
                    Estudiante = new EstudianteBE
                    {
                        IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                        CodigoEstudiante = dr["CodigoEstudiante"]?.ToString() ?? string.Empty,
                        Nombres = dr["NombresEstudiante"]?.ToString() ?? string.Empty,
                        ApellidoPaterno = dr["ApellidoPaternoEstudiante"]?.ToString() ?? string.Empty,
                        ApellidoMaterno = dr["ApellidoMaternoEstudiante"]?.ToString() ?? string.Empty
                    }
                });
            }

            return lista;
        }

        public RutaEstudianteBE? ListarPorId(Guid id)
        {
            RutaEstudianteBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_RutaEstudiante_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRutaEstudiante", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new RutaEstudianteBE
                {
                    IdRutaEstudiante = dr.GetGuid(dr.GetOrdinal("IdRutaEstudiante")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    IdEstudiante = dr.GetGuid(dr.GetOrdinal("IdEstudiante")),
                    IdParaderoSubida = dr["IdParaderoSubida"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParaderoSubida")),
                    IdParaderoBajada = dr["IdParaderoBajada"] == DBNull.Value ? null : dr.GetGuid(dr.GetOrdinal("IdParaderoBajada")),
                    CodigoRutaEstudiante = dr["CodigoRutaEstudiante"]?.ToString() ?? string.Empty,
                    FechaInicioVigencia = Convert.ToDateTime(dr["FechaInicioVigencia"]),
                    FechaFinVigencia = dr["FechaFinVigencia"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaFinVigencia"]),
                    EstadoAsignacion = dr["EstadoAsignacion"]?.ToString() ?? "ACTIVO",
                    Observaciones = dr["Observaciones"] == DBNull.Value ? null : dr["Observaciones"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"])
                };
            }

            return entidad;
        }

        public bool Registrar(RutaEstudianteBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_RutaEstudiante_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@IdEstudiante", entidad.IdEstudiante);
            cmd.Parameters.AddWithValue("@IdParaderoSubida", (object?)entidad.IdParaderoSubida ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdParaderoBajada", (object?)entidad.IdParaderoBajada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaInicioVigencia", entidad.FechaInicioVigencia);
            cmd.Parameters.AddWithValue("@FechaFinVigencia", (object?)entidad.FechaFinVigencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoAsignacion", entidad.EstadoAsignacion);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);
                if (filas > 0)
                {
                    entidad.CodigoRutaEstudiante = dr["CodigoGenerado"]?.ToString() ?? string.Empty;
                    return true;
                }
            }

            return false;
        }

        public bool Actualizar(RutaEstudianteBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_RutaEstudiante_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRutaEstudiante", entidad.IdRutaEstudiante);
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@IdEstudiante", entidad.IdEstudiante);
            cmd.Parameters.AddWithValue("@IdParaderoSubida", (object?)entidad.IdParaderoSubida ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdParaderoBajada", (object?)entidad.IdParaderoBajada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaInicioVigencia", entidad.FechaInicioVigencia);
            cmd.Parameters.AddWithValue("@FechaFinVigencia", (object?)entidad.FechaFinVigencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoAsignacion", entidad.EstadoAsignacion);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
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
            using SqlCommand cmd = new SqlCommand("sp_RutaEstudiante_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRutaEstudiante", id);

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
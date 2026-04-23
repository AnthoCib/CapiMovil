using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class IncidenciaDALC
    {
        private readonly BDConexion _bdConexion;

        public IncidenciaDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<IncidenciaBE> Listar()
        {
            List<IncidenciaBE> lista = new List<IncidenciaBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_Listar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public List<IncidenciaBE> ListarPorConductor(Guid idConductor)
        {
            List<IncidenciaBE> lista = new List<IncidenciaBE>();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_ListarPorConductor", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdConductor", idConductor);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(Mapear(dr));
            }

            return lista;
        }

        public IncidenciaBE? ListarPorId(Guid idIncidencia)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_ListarPorId", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdIncidencia", idIncidencia);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                return Mapear(dr);
            }

            return null;
        }

        public bool Registrar(IncidenciaBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_Registrar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@IdConductor", entidad.IdConductor);
            cmd.Parameters.AddWithValue("@ReportadoPor", (object?)entidad.ReportadoPor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoIncidencia", entidad.TipoIncidencia);
            cmd.Parameters.AddWithValue("@Descripcion", entidad.Descripcion);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@EstadoIncidencia", entidad.EstadoIncidencia);
            cmd.Parameters.AddWithValue("@Prioridad", entidad.Prioridad);
            cmd.Parameters.AddWithValue("@Solucion", (object?)entidad.Solucion ?? DBNull.Value);

            cn.Open();

            using SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                int filas = Convert.ToInt32(dr["FilasAfectadas"]);

                if (dr["CodigoGenerado"] != DBNull.Value)
                    entidad.CodigoIncidencia = dr["CodigoGenerado"].ToString() ?? string.Empty;

                if (ExisteColumna(dr, "IdIncidencia") && dr["IdIncidencia"] != DBNull.Value)
                    entidad.IdIncidencia = (Guid)dr["IdIncidencia"];

                return filas > 0;
            }

            return false;
        }

        public bool Actualizar(IncidenciaBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_Actualizar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdIncidencia", entidad.IdIncidencia);
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@IdConductor", entidad.IdConductor);
            cmd.Parameters.AddWithValue("@TipoIncidencia", entidad.TipoIncidencia);
            cmd.Parameters.AddWithValue("@Descripcion", entidad.Descripcion);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@EstadoIncidencia", entidad.EstadoIncidencia);
            cmd.Parameters.AddWithValue("@Prioridad", entidad.Prioridad);
            cmd.Parameters.AddWithValue("@Solucion", (object?)entidad.Solucion ?? DBNull.Value);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool CambiarEstado(Guid idIncidencia, string estadoIncidencia)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_CambiarEstado", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdIncidencia", idIncidencia);
            cmd.Parameters.AddWithValue("@EstadoIncidencia", estadoIncidencia);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool Cerrar(Guid idIncidencia, string solucion)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_Cerrar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdIncidencia", idIncidencia);
            cmd.Parameters.AddWithValue("@Solucion", solucion);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool Eliminar(Guid idIncidencia)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Incidencia_Eliminar", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdIncidencia", idIncidencia);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            return result != null && Convert.ToInt32(result) > 0;
        }

        private IncidenciaBE Mapear(SqlDataReader dr)
        {
            return new IncidenciaBE
            {
                IdIncidencia = (Guid)dr["IdIncidencia"],
                IdRecorrido = (Guid)dr["IdRecorrido"],
                IdConductor = (Guid)dr["IdConductor"],
                ReportadoPor = dr["ReportadoPor"] == DBNull.Value ? null : (Guid?)dr["ReportadoPor"],

                CodigoIncidencia = dr["CodigoIncidencia"].ToString() ?? string.Empty,
                TipoIncidencia = dr["TipoIncidencia"].ToString() ?? string.Empty,
                Descripcion = dr["Descripcion"].ToString() ?? string.Empty,
                FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                EstadoIncidencia = dr["EstadoIncidencia"].ToString() ?? string.Empty,
                Prioridad = dr["Prioridad"].ToString() ?? string.Empty,
                FechaCierre = dr["FechaCierre"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["FechaCierre"]),
                Solucion = dr["Solucion"] == DBNull.Value ? null : dr["Solucion"].ToString(),

                Estado = Convert.ToBoolean(dr["Estado"]),
                FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["FechaActualizacion"]),
                FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["FechaEliminacion"]),

                CodigoRecorrido = ExisteColumna(dr, "CodigoRecorrido") && dr["CodigoRecorrido"] != DBNull.Value
                    ? dr["CodigoRecorrido"].ToString()
                    : null,

                NombreConductor = ExisteColumna(dr, "NombreConductor") && dr["NombreConductor"] != DBNull.Value
                    ? dr["NombreConductor"].ToString()
                    : null,

                UsernameReportadoPor = ExisteColumna(dr, "UsernameReportadoPor") && dr["UsernameReportadoPor"] != DBNull.Value
                    ? dr["UsernameReportadoPor"].ToString()
                    : null
            };
        }

        private bool ExisteColumna(SqlDataReader dr, string nombreColumna)
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

using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class CalificacionConductorDALC
    {
        private readonly BDConexion _bdConexion;

        public CalificacionConductorDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public bool Registrar(CalificacionConductorBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_CalificacionConductor_Registrar", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPadre", entidad.IdPadre);
            cmd.Parameters.AddWithValue("@IdConductor", entidad.IdConductor);
            cmd.Parameters.AddWithValue("@IdEstudiante", (object?)entidad.IdEstudiante ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Puntaje", entidad.Puntaje);
            cmd.Parameters.AddWithValue("@Comentario", (object?)entidad.Comentario ?? DBNull.Value);

            cn.Open();
            object? result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }
    }
}

using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    internal static class RegistroResultadoDALC
    {
        public static bool EsRegistroExitoso(SqlDataReader dr, out int filasAfectadas, out string codigoGenerado, out string? mensaje)
        {
            filasAfectadas = 0;
            codigoGenerado = string.Empty;
            mensaje = null;

            if (!dr.Read())
                return false;

            if (ExisteColumna(dr, "FilasAfectadas") && dr["FilasAfectadas"] != DBNull.Value)
            {
                int.TryParse(dr["FilasAfectadas"]?.ToString(), out filasAfectadas);
            }

            if (ExisteColumna(dr, "CodigoGenerado") && dr["CodigoGenerado"] != DBNull.Value)
            {
                codigoGenerado = dr["CodigoGenerado"]?.ToString() ?? string.Empty;
            }

            if (ExisteColumna(dr, "Mensaje") && dr["Mensaje"] != DBNull.Value)
            {
                mensaje = dr["Mensaje"]?.ToString();
            }

            if (filasAfectadas > 0)
                return true;

            return filasAfectadas == 0
                   && !string.IsNullOrWhiteSpace(codigoGenerado)
                   && string.IsNullOrWhiteSpace(mensaje);
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

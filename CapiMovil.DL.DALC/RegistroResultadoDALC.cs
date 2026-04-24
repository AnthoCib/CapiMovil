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

            filasAfectadas = ObtenerEntero(dr, "FilasAfectadas", "Filas", "Resultado", "RowsAffected");
            codigoGenerado = ObtenerTexto(dr, "CodigoGenerado", "Codigo", "CodigoRecorrido", "CodigoIncidencia", "CodigoAuditoria");
            mensaje = ObtenerTexto(dr, "Mensaje", "Error", "Detalle");

            if (filasAfectadas > 0)
                return true;

            return filasAfectadas == 0
                   && !string.IsNullOrWhiteSpace(codigoGenerado)
                   && string.IsNullOrWhiteSpace(mensaje);
        }

        private static int ObtenerEntero(SqlDataReader dr, params string[] nombresColumna)
        {
            foreach (string nombre in nombresColumna)
            {
                if (ExisteColumna(dr, nombre) && dr[nombre] != DBNull.Value && int.TryParse(dr[nombre]?.ToString(), out int valor))
                    return valor;
            }

            return 0;
        }

        private static string ObtenerTexto(SqlDataReader dr, params string[] nombresColumna)
        {
            foreach (string nombre in nombresColumna)
            {
                if (ExisteColumna(dr, nombre) && dr[nombre] != DBNull.Value)
                    return dr[nombre]?.ToString() ?? string.Empty;
            }

            return string.Empty;
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

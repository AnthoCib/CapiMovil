using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class BDConexion
    {
        private readonly string _cadenaConexion;

        public BDConexion(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("cn")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'cn'.");
        }
        
        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }

    }
}

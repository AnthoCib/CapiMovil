using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class UbicacionBusDALC : ICrudDALC<UbicacionBusBE>
    {
        private readonly BDConexion _bdConexion;

        public UbicacionBusDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<UbicacionBusBE> Listar()
        {
            List<UbicacionBusBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_UbicacionBus_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new UbicacionBusBE
                {
                    IdUbicacion = dr.GetGuid(dr.GetOrdinal("IdUbicacion")),
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    CodigoUbicacion = dr["CodigoUbicacion"]?.ToString() ?? string.Empty,
                    Latitud = Convert.ToDecimal(dr["Latitud"]),
                    Longitud = Convert.ToDecimal(dr["Longitud"]),
                    Velocidad = dr["Velocidad"] == DBNull.Value ? null : Convert.ToDecimal(dr["Velocidad"]),
                    PrecisionMetros = dr["PrecisionMetros"] == DBNull.Value ? null : Convert.ToDecimal(dr["PrecisionMetros"]),
                    FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                    Fuente = dr["Fuente"] == DBNull.Value ? null : dr["Fuente"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Recorrido = new RecorridoBE
                    {
                        IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                        CodigoRecorrido = dr["CodigoRecorrido"]?.ToString() ?? string.Empty,
                        Fecha = Convert.ToDateTime(dr["FechaRecorrido"])
                    }
                });
            }

            return lista;
        }

        public UbicacionBusBE? ListarPorId(Guid id)
        {
            UbicacionBusBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_UbicacionBus_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUbicacion", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new UbicacionBusBE
                {
                    IdUbicacion = dr.GetGuid(dr.GetOrdinal("IdUbicacion")),
                    IdRecorrido = dr.GetGuid(dr.GetOrdinal("IdRecorrido")),
                    CodigoUbicacion = dr["CodigoUbicacion"]?.ToString() ?? string.Empty,
                    Latitud = Convert.ToDecimal(dr["Latitud"]),
                    Longitud = Convert.ToDecimal(dr["Longitud"]),
                    Velocidad = dr["Velocidad"] == DBNull.Value ? null : Convert.ToDecimal(dr["Velocidad"]),
                    PrecisionMetros = dr["PrecisionMetros"] == DBNull.Value ? null : Convert.ToDecimal(dr["PrecisionMetros"]),
                    FechaHora = Convert.ToDateTime(dr["FechaHora"]),
                    Fuente = dr["Fuente"] == DBNull.Value ? null : dr["Fuente"].ToString(),
                    Estado = Convert.ToBoolean(dr["Estado"])
                };
            }

            return entidad;
        }

        public bool Registrar(UbicacionBusBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_UbicacionBus_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@Latitud", entidad.Latitud);
            cmd.Parameters.AddWithValue("@Longitud", entidad.Longitud);
            cmd.Parameters.AddWithValue("@Velocidad", (object?)entidad.Velocidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrecisionMetros", (object?)entidad.PrecisionMetros ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@Fuente", (object?)entidad.Fuente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                entidad.CodigoUbicacion = codigoGenerado;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

            return false;
        }

        public bool Actualizar(UbicacionBusBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_UbicacionBus_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUbicacion", entidad.IdUbicacion);
            cmd.Parameters.AddWithValue("@IdRecorrido", entidad.IdRecorrido);
            cmd.Parameters.AddWithValue("@Latitud", entidad.Latitud);
            cmd.Parameters.AddWithValue("@Longitud", entidad.Longitud);
            cmd.Parameters.AddWithValue("@Velocidad", (object?)entidad.Velocidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrecisionMetros", (object?)entidad.PrecisionMetros ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaHora", entidad.FechaHora);
            cmd.Parameters.AddWithValue("@Fuente", (object?)entidad.Fuente ?? DBNull.Value);
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
            using SqlCommand cmd = new SqlCommand("sp_UbicacionBus_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUbicacion", id);

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

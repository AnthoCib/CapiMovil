using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class ParaderoDALC : ICrudDALC<ParaderoBE>
    {
        private readonly BDConexion _bdConexion;

        public ParaderoDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<ParaderoBE> Listar()
        {
            List<ParaderoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ParaderoBE
                {
                    IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Direccion = dr["Direccion"]?.ToString() ?? string.Empty,
                    Latitud = dr["Latitud"] == DBNull.Value ? null : Convert.ToDecimal(dr["Latitud"]),
                    Longitud = dr["Longitud"] == DBNull.Value ? null : Convert.ToDecimal(dr["Longitud"]),
                    OrdenParada = Convert.ToInt32(dr["OrdenParada"]),
                    HoraEstimada = dr["HoraEstimada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraEstimada"],
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"]),
                    Ruta = new RutaBE
                    {
                        IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                        CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                        Nombre = dr["NombreRuta"]?.ToString() ?? string.Empty
                    }
                });
            }

            return lista;
        }

        public ParaderoBE? ListarPorId(Guid id)
        {
            ParaderoBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdParadero", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new ParaderoBE
                {
                    IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Direccion = dr["Direccion"]?.ToString() ?? string.Empty,
                    Latitud = dr["Latitud"] == DBNull.Value ? null : Convert.ToDecimal(dr["Latitud"]),
                    Longitud = dr["Longitud"] == DBNull.Value ? null : Convert.ToDecimal(dr["Longitud"]),
                    OrdenParada = Convert.ToInt32(dr["OrdenParada"]),
                    HoraEstimada = dr["HoraEstimada"] == DBNull.Value ? null : (TimeSpan?)dr["HoraEstimada"],
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(ParaderoBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@Nombre", entidad.Nombre);
            cmd.Parameters.AddWithValue("@Direccion", entidad.Direccion);
            cmd.Parameters.AddWithValue("@Latitud", (object?)entidad.Latitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitud", (object?)entidad.Longitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrdenParada", entidad.OrdenParada);
            cmd.Parameters.AddWithValue("@HoraEstimada", (object?)entidad.HoraEstimada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                entidad.CodigoParadero = codigoGenerado;
                    return true;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException(mensaje);

            return false;
        }

        public bool Actualizar(ParaderoBE entidad)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdParadero", entidad.IdParadero);
            cmd.Parameters.AddWithValue("@IdRuta", entidad.IdRuta);
            cmd.Parameters.AddWithValue("@Nombre", entidad.Nombre);
            cmd.Parameters.AddWithValue("@Direccion", entidad.Direccion);
            cmd.Parameters.AddWithValue("@Latitud", (object?)entidad.Latitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitud", (object?)entidad.Longitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrdenParada", entidad.OrdenParada);
            cmd.Parameters.AddWithValue("@HoraEstimada", (object?)entidad.HoraEstimada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", entidad.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }

        public bool Eliminar(Guid id)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdParadero", id);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }

        public List<ParaderoBE> ListarPorRuta(Guid idRuta)
        {
            List<ParaderoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_ListarPorRuta", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", idRuta);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ParaderoBE
                {
                    IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Direccion = ExisteColumna(dr, "Direccion") && dr["Direccion"] != DBNull.Value
                        ? dr["Direccion"]?.ToString() ?? string.Empty
                        : string.Empty,
                    Latitud = ExisteColumna(dr, "Latitud") && dr["Latitud"] != DBNull.Value
                        ? Convert.ToDecimal(dr["Latitud"])
                        : null,
                    Longitud = ExisteColumna(dr, "Longitud") && dr["Longitud"] != DBNull.Value
                        ? Convert.ToDecimal(dr["Longitud"])
                        : null,
                    OrdenParada = Convert.ToInt32(dr["OrdenParada"])
                });
            }

            return lista;
        }
        public List<ParaderoBE> ListarActivos()
        {
            List<ParaderoBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Paradero_ListarActivos", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new ParaderoBE
                {
                    IdParadero = dr.GetGuid(dr.GetOrdinal("IdParadero")),
                    CodigoParadero = dr["CodigoParadero"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty
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
    }
}

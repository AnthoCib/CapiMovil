using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class BusDALC : ICrudDALC<BusBE>
    {
        private readonly BDConexion _bdConexion;

        public BusDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<BusBE> Listar()
        {
            List<BusBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new BusBE
                {
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                    Placa = dr["Placa"]?.ToString() ?? string.Empty,
                    Marca = dr["Marca"] == DBNull.Value ? null : dr["Marca"].ToString(),
                    Modelo = dr["Modelo"] == DBNull.Value ? null : dr["Modelo"].ToString(),
                    Color = dr["Color"] == DBNull.Value ? null : dr["Color"].ToString(),
                    Anio = dr["Anio"] == DBNull.Value ? null : Convert.ToInt32(dr["Anio"]),
                    Capacidad = Convert.ToInt32(dr["Capacidad"]),
                    EstadoOperacion = dr["EstadoOperacion"]?.ToString() ?? "ACTIVO",
                    SeguroVigente = Convert.ToBoolean(dr["SeguroVigente"]),
                    FechaVencimientoSOAT = dr["FechaVencimientoSOAT"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaVencimientoSOAT"]),
                    FechaRevisionTecnica = dr["FechaRevisionTecnica"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaRevisionTecnica"]),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public BusBE? ListarPorId(Guid idBus)
        {
            BusBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdBus", idBus);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new BusBE
                {
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                    Placa = dr["Placa"]?.ToString() ?? string.Empty,
                    Marca = dr["Marca"] == DBNull.Value ? null : dr["Marca"].ToString(),
                    Modelo = dr["Modelo"] == DBNull.Value ? null : dr["Modelo"].ToString(),
                    Color = dr["Color"] == DBNull.Value ? null : dr["Color"].ToString(),
                    Anio = dr["Anio"] == DBNull.Value ? null : Convert.ToInt32(dr["Anio"]),
                    Capacidad = Convert.ToInt32(dr["Capacidad"]),
                    EstadoOperacion = dr["EstadoOperacion"]?.ToString() ?? "ACTIVO",
                    SeguroVigente = Convert.ToBoolean(dr["SeguroVigente"]),
                    FechaVencimientoSOAT = dr["FechaVencimientoSOAT"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaVencimientoSOAT"]),
                    FechaRevisionTecnica = dr["FechaRevisionTecnica"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaRevisionTecnica"]),
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(BusBE bus)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Placa", bus.Placa);
            cmd.Parameters.AddWithValue("@Marca", (object?)bus.Marca ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Modelo", (object?)bus.Modelo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Color", (object?)bus.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Anio", (object?)bus.Anio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Capacidad", bus.Capacidad);
            cmd.Parameters.AddWithValue("@EstadoOperacion", bus.EstadoOperacion);
            cmd.Parameters.AddWithValue("@SeguroVigente", bus.SeguroVigente);
            cmd.Parameters.AddWithValue("@FechaVencimientoSOAT", (object?)bus.FechaVencimientoSOAT ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaRevisionTecnica", (object?)bus.FechaRevisionTecnica ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", bus.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                bus.CodigoBus = codigoGenerado;
                    return true;
            }

            return false;
        }

        public bool Actualizar(BusBE bus)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdBus", bus.IdBus);
            cmd.Parameters.AddWithValue("@Placa", bus.Placa);
            cmd.Parameters.AddWithValue("@Marca", (object?)bus.Marca ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Modelo", (object?)bus.Modelo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Color", (object?)bus.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Anio", (object?)bus.Anio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Capacidad", bus.Capacidad);
            cmd.Parameters.AddWithValue("@EstadoOperacion", bus.EstadoOperacion);
            cmd.Parameters.AddWithValue("@SeguroVigente", bus.SeguroVigente);
            cmd.Parameters.AddWithValue("@FechaVencimientoSOAT", (object?)bus.FechaVencimientoSOAT ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaRevisionTecnica", (object?)bus.FechaRevisionTecnica ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", bus.Estado);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }

        public bool Eliminar(Guid idBus)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdBus", idBus);

            cn.Open();
            object? result = cmd.ExecuteScalar();

            if (result != null)
            {
                int filas = Convert.ToInt32(result);
                return filas > 0;
            }

            return false;
        }
        public List<BusBE> ListarActivos()
        {
            List<BusBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Bus_ListarActivos", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new BusBE
                {
                    IdBus = dr.GetGuid(dr.GetOrdinal("IdBus")),
                    CodigoBus = dr["CodigoBus"]?.ToString() ?? string.Empty,
                    Placa = dr["Placa"]?.ToString() ?? string.Empty
                });
            }

            return lista;
        }
    }
}
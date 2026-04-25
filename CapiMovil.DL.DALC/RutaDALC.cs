using CapiMovil.BL.BE;
using System.Data;
using System.Data.SqlClient;

namespace CapiMovil.DL.DALC
{
    public class RutaDALC : ICrudDALC<RutaBE>
    {
        private readonly BDConexion _bdConexion;

        public RutaDALC(BDConexion bdConexion)
        {
            _bdConexion = bdConexion;
        }

        public List<RutaBE> Listar()
        {
            List<RutaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_Listar", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RutaBE
                {
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Descripcion = dr["Descripcion"] == DBNull.Value ? null : dr["Descripcion"].ToString(),
                    Turno = dr["Turno"]?.ToString() ?? "MANANA",
                    HoraInicio = (TimeSpan)dr["HoraInicio"],
                    HoraFin = (TimeSpan)dr["HoraFin"],
                    PuntoInicio = dr["PuntoInicio"] == DBNull.Value ? null : dr["PuntoInicio"].ToString(),
                    PuntoFin = dr["PuntoFin"] == DBNull.Value ? null : dr["PuntoFin"].ToString(),
                    EstadoRuta = dr["EstadoRuta"]?.ToString() ?? "ACTIVA",
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                });
            }

            return lista;
        }

        public RutaBE? ListarPorId(Guid idRuta)
        {
            RutaBE? entidad = null;

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_ObtenerPorId", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", idRuta);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                entidad = new RutaBE
                {
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                    Descripcion = dr["Descripcion"] == DBNull.Value ? null : dr["Descripcion"].ToString(),
                    Turno = dr["Turno"]?.ToString() ?? "MANANA",
                    HoraInicio = (TimeSpan)dr["HoraInicio"],
                    HoraFin = (TimeSpan)dr["HoraFin"],
                    PuntoInicio = dr["PuntoInicio"] == DBNull.Value ? null : dr["PuntoInicio"].ToString(),
                    PuntoFin = dr["PuntoFin"] == DBNull.Value ? null : dr["PuntoFin"].ToString(),
                    EstadoRuta = dr["EstadoRuta"]?.ToString() ?? "ACTIVA",
                    Estado = Convert.ToBoolean(dr["Estado"]),
                    FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                    FechaActualizacion = dr["FechaActualizacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaActualizacion"]),
                    FechaEliminacion = dr["FechaEliminacion"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaEliminacion"])
                };
            }

            return entidad;
        }

        public bool Registrar(RutaBE ruta)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_Registrar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Nombre", ruta.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)ruta.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Turno", ruta.Turno);
            cmd.Parameters.AddWithValue("@HoraInicio", ruta.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFin", ruta.HoraFin);
            cmd.Parameters.AddWithValue("@PuntoInicio", (object?)ruta.PuntoInicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PuntoFin", (object?)ruta.PuntoFin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoRuta", ruta.EstadoRuta);
            cmd.Parameters.AddWithValue("@Estado", ruta.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (RegistroResultadoDALC.EsRegistroExitoso(dr, out int filas, out string codigoGenerado, out string? mensaje))
            {
                ruta.CodigoRuta = codigoGenerado;
                    return true;
            }

            return false;
        }

        public bool Actualizar(RutaBE ruta)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_Actualizar", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", ruta.IdRuta);
            cmd.Parameters.AddWithValue("@Nombre", ruta.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)ruta.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Turno", ruta.Turno);
            cmd.Parameters.AddWithValue("@HoraInicio", ruta.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFin", ruta.HoraFin);
            cmd.Parameters.AddWithValue("@PuntoInicio", (object?)ruta.PuntoInicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PuntoFin", (object?)ruta.PuntoFin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoRuta", ruta.EstadoRuta);
            cmd.Parameters.AddWithValue("@Estado", ruta.Estado);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }

        public bool Eliminar(Guid idRuta)
        {
            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_EliminarLogico", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdRuta", idRuta);

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            return RegistroResultadoDALC.EsRegistroExitoso(dr, out _, out _, out _);
        }
        public List<RutaBE> ListarActivas()
        {
            List<RutaBE> lista = new();

            using SqlConnection cn = _bdConexion.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand("sp_Ruta_ListarActivas", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cn.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new RutaBE
                {
                    IdRuta = dr.GetGuid(dr.GetOrdinal("IdRuta")),
                    CodigoRuta = dr["CodigoRuta"]?.ToString() ?? string.Empty,
                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty
                });
            }

            return lista;
        }
    }
}

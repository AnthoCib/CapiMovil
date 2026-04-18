using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BC
{
    public interface ICrudBC<T>
    {
        List<T> Listar();
        T? ListarPorId(Guid id);
        bool Registrar(T entidad);
        bool Actualizar(T entidad);
        bool Eliminar(Guid id);
    }
}

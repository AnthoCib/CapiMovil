namespace CapiMovil.DL.DALC
{
    public interface ICrudDALC<T>
    {
        List<T> Listar();
        T? ListarPorId(Guid id);
        bool Registrar(T entidad);
        bool Actualizar(T entidad);
        bool Eliminar(Guid id);
    }
}

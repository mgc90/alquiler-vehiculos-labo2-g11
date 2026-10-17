namespace alquiler_vehiculos_g11.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<IReadOnlyList<TEntity>> ObtenerTodosAsync();

    Task<TEntity?> ObtenerPorIdAsync(int id);
}

public interface IWriteRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    Task AgregarAsync(TEntity entity);

    Task ActualizarAsync(TEntity entity);
}

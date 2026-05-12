using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services.Implementations;

namespace ClinicaMedicala.Services.Implementations;

public class GenericService<T> : IGenericService<T> where T : class
{
    protected readonly IGenericRepository<T> _repository;

    public GenericService(IGenericRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _repository.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _repository.Update(entity);
    }

    public void Delete(T entity)
    {
        _repository.Delete(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _repository.SaveChangesAsync();
    }
}

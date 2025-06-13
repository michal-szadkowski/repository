using System.Collections.Concurrent;

namespace Repository.WebApi.Data;

public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{

    private readonly ConcurrentDictionary<int, T> storage = new();
    private int nextId;

    private readonly ILogger<InMemoryRepository<T>> logger;

    public InMemoryRepository(ILogger<InMemoryRepository<T>> logger)
    {
        this.logger = logger;
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        logger.LogDebug("Retrieving all {TypeName} entities.", typeof(T).Name);
        return Task.FromResult(storage.Values.AsEnumerable());
    }

    public Task<T?> GetByIdAsync(int id)
    {
        logger.LogDebug("Retrieving {TypeName} with Id: {Id}", typeof(T).Name, id);
        storage.TryGetValue(id, out T? entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(T entity)
    {
        int newId = Interlocked.Increment(ref nextId);
        entity.Id = newId;
        storage.TryAdd(newId, entity);
        logger.LogInformation("Added new {TypeName} with Id: {Id}", typeof(T).Name, newId);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        if (storage.ContainsKey(entity.Id))
        {
            storage.AddOrUpdate(entity.Id, entity, (key, oldValue) => entity);
            logger.LogInformation("Updated {TypeName} with Id: {Id}", typeof(T).Name, entity.Id);
        }
        else
        {
            throw new KeyNotFoundException($"Entity with Id {entity.Id} not found.");
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        if (storage.TryRemove(id, out _))
        {
            logger.LogInformation("Deleted {TypeName} with Id: {Id}", typeof(T).Name, id);
        }
        else
        {
            throw new KeyNotFoundException($"Entity with Id {id} not found.");
        }
        return Task.CompletedTask;
    }
}

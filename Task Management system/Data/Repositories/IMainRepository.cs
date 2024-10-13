using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Task_Management_system.Data.Repositories
{
    public interface IMainRepository<T> where T : class
    {

        // Get all entites with include other entities
        IEnumerable<T> GetAll(params string[] entities);

        // Get all entites with include other entities [Another version]
        public IEnumerable<T> GetAll(
         Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

        // Get entity by id
        T GetBy(int id);

        // Get entity by any property
        T GetBy(Expression<Func<bool, T>> predicate);

        // Add a new entity
        void Add(T entity);

        // Update an existing entity
        void Update(T entity);

        // Delete an entity by id
        void Delete(int id);

        
    }
}

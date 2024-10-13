using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Task_Management_system.Models;

namespace Task_Management_system.Data.Repositories
{
    public class MainRepository<T> : IMainRepository<T> where T : class
    {
        private readonly AppDbContext _db;
        public MainRepository(AppDbContext db) {
            _db = db;
        }
        
        // Adding a new entity 
        public void Add(T entity)
        {
            _db.Add(entity);
        }

        // Updating an entity
        public void Update(T entity)
        {
            _db.Update(entity);
        }

        // Deleting an entity by id
        public void Delete(int id)
        {
            var entity = _db.Set<T>().Find(id);
            if (entity == null)
            {
                throw new Exception("The Item Not Found");
            }
            else
                _db.Remove(entity);
        }


        // Getting all records of entity type T, and eager loading another entities if exist
        public IEnumerable<T> GetAll(Func<  IQueryable<T>, IIncludableQueryable<T, object>  > include = null)
        {
            // getting all records from database of entity type T
            IQueryable<T> query = _db.Set<T>();

            // Include related entites if the include function is not null
            if (include != null)
            {
                query = include(query);  
            }

            return query.ToList();  
        }

        // Another version of Getting All entities but eager loading anotehr entities 
        public IEnumerable<T> GetAll(params string[] entities)
        {
            IQueryable<T> query = _db.Set<T>();
            
            // Include related entites if exist
            if (entities.Length > 0)
            {
                foreach (var entity in entities)
                {
                    query = query.Include(entity);
                    
                }
            }   
            return query;
        }

        // Getting an entity by id
        public virtual T GetBy(int id)
        {
            var entity = _db.Set<T>().Find(id);
            if (entity == null) 
            {
                throw new Exception("The Item Not Found");
            }
            else
            return(entity);
        }

        // Getting an entity by any property
        public T GetBy(Expression<Func<bool, T>> predicate)
        {
            var entity = _db.Set<T>().Find(predicate);
            if (entity == null)
            {
                throw new Exception("The Item Not Found");

            }
            else
            return entity;
        }

       
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Task_Management_system.Data.Repositories;
using Task_Management_system.Models;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;


        public IMainRepository<Project> projects { get; private set; }

        public IMainRepository<User> users { get; private set; }

        public IMainRepository<SubTask> subtasks { get; private set; }

        public ITaskRepository tasks { get; private set; }

        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            projects = new MainRepository<Project>(_db);
            tasks = new TaskRepository(_db);
            users = new MainRepository<User>(_db);
            subtasks = new MainRepository<SubTask>(_db);

        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public int SaveChanges()
        {
            
           return _db.SaveChanges();
        }
    }
}

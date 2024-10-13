using Task_Management_system.Data.Repositories;
using Task_Management_system.Models;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Data.UnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        IMainRepository<Project> projects { get; }
        IMainRepository<User> users { get; }
        IMainRepository<SubTask> subtasks { get; }
        ITaskRepository tasks { get; }  

        int SaveChanges();
    }
}

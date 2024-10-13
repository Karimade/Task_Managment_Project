using System.Data;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Data.Repositories
{
    public interface ITaskRepository:IMainRepository<Task>
    {
        IEnumerable<Task> GetFilteredTasks(int? projectId, int? userId, DateTime? dueDateFrom, DateTime? dueDateTo);
        IEnumerable<Task> GetOverdueTasks(int count);
        Task GetTaskById(int taskId);
    }
}

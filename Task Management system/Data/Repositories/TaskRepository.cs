
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Data.Repositories
{
   
    public class TaskRepository : MainRepository<Task>, ITaskRepository
    {

        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context) :base(context) {
            _context = context;
           
        }

        // Filter Tasks by (Project Id || userId || data or any compination of them)
        public IEnumerable<Task> GetFilteredTasks(int? projectId, int? userId, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            // Start with a base query with includes for related entities
            IQueryable<Task> query = _context.tasks
                                              .Include(t => t.UserTasks).ThenInclude(t=>t.User)
                                              .Include(t => t.SubTasks)
                                              .Include(t => t.Project);

            // Apply filter for projectId if it is not null
            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            // Apply filter for userId if it is not null
            if (userId.HasValue)
            {
                query = query.Where(t => t.UserTasks.Any(ut=>ut.UserId == userId) );
            }

            // Apply filter for dueDate range if both dates are provided
            if (dueDateFrom.HasValue && dueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate >= dueDateFrom.Value && t.DueDate <= dueDateTo.Value);
            }

            // Return the result as a list
            return query.ToList();
        }

        // Getting a task by id 
        public Task GetTaskById(int taskId)
        {
           var task =  _context.tasks
                .Where(t => t.TaskId == taskId)                                // Filter first by taskId
                    .Include(t => t.SubTasks).ThenInclude(st => st.Users)      // Include related entities after filtering
                    .Include(t => t.Project)
                    .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User).AsSplitQuery()      // Split Query to improve performance
                    .FirstOrDefault();
           
                return task;    
        }

        // Get the last [count] Over due tasks
        public IEnumerable<Task> GetOverdueTasks(int count)
        {
            var now = DateTime.Now;
            return _context.tasks.
                Where(t=>t.DueDate<now)
                .OrderByDescending(t=>t.DueDate) // Order by most recent due dates
                .Take(count) // Taking first {count} tasks
                .ToList();
        }

    }
}

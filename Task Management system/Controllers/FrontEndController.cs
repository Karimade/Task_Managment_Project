using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management_system.Data;
using Task_Management_system.Data.UnitOfWork;
using Task_Management_system.Models;
using Task = Task_Management_system.Models.Task;
namespace Task_Management_system.Controllers
{
    public class FrontEndController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IUnitOfWork _UOW;
        public FrontEndController(ApiService apiService, IUnitOfWork UOW)
        {
            _apiService = apiService;
            _UOW = UOW;
        }
        // Task List View

        public IActionResult TaskList()
        {
            var users = _UOW.users.GetAll();
            ViewBag.Users=users;
            var projects = _UOW.projects.GetAll();
            ViewBag.Projects = projects;
           var tasks =  _UOW.tasks.GetAll(query => query.Include(t => t.UserTasks).ThenInclude(su => su.User)
                                            .Include(t=>t.SubTasks).Include(t=>t.Project));

                                                   
            return View(tasks);
        }
        [HttpPost]
        public IActionResult TaskList(int? projectId ,int? userId, DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            // Sending the Select Values
            var users = _UOW.users.GetAll();
            ViewBag.Users = users;

            var projects = _UOW.projects.GetAll();
            ViewBag.Projects = projects;

            // Sending the last filter data to display it
            if (projectId != null)
            {
                var OldProjectName = _UOW.projects.GetBy((int)projectId);
                ViewBag.OldProjectName = OldProjectName.Name;
            }else
                ViewBag.OldProjectName = null;
            if (userId != null)
            {
                var OldUserName = _UOW.users.GetBy((int)userId);
                ViewBag.OldUserName = OldUserName.Name;
            }
            else
                ViewBag.OldUserName = null;
            
            ViewBag.OldDateFrom = dueDateFrom;
            ViewBag.OldDateTo = dueDateTo;
            
            IEnumerable<Task> tasks = _apiService.GetTasksFiltered(projectId, userId, dueDateFrom, dueDateTo);

            return View(tasks);
        }

        // Task Details View
        public IActionResult TaskDetails(int taskId)
        {
            
            var taskDetails = _apiService.GetTaskDetails(taskId);
            if (taskDetails == null)
            {
                return NotFound();
            }
            return View(taskDetails);
        }


        // Overdue Tasks JSON Feed
        public IActionResult OverdueTasksJson(int count)
        {
            var overdueTasks = _apiService.GetOverdueTasks(count);
            return Json(overdueTasks);
        }

        public IActionResult SubTaskDetails(int subTaskId) 
        {
            if (subTaskId == null)
            {
                return NotFound();
            }

            SubTask subtask =_UOW.subtasks.GetAll(query=>query.Include(s=>s.Users)).FirstOrDefault(s => s.SubTaskId == subTaskId);

            
            return View(subtask);
        }
    }
}

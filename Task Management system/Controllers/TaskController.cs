using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task_Management_system.Data.UnitOfWork;
using Task_Management_system.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _UOW;
        private readonly IHostingEnvironment _host;
        public TaskController(IUnitOfWork UOW, IHostingEnvironment host)
        {
            _host = host;
            _UOW = UOW;
        }
        public IActionResult Index()
        {

            var tasks = _UOW.tasks.GetAll(
             t => t.Include(ut => ut.UserTasks).ThenInclude(u => u.User)
             .Include(p => p.Project).Include(s => s.SubTasks)
                );
            return View(tasks);
        }

        public IActionResult Insert()
        {
            IEnumerable<Project> projects = _UOW.projects.GetAll();
            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name");
           
            IEnumerable<User> users = _UOW.users.GetAll();
            ViewBag.usersList = new SelectList(users, "UserId", "Name");
            return View();
        }


        [HttpPost]
        public IActionResult Insert(Task task, int[] AssignedUsers)
        {
            if (task != null && AssignedUsers != null)
            {
                foreach (var userId in AssignedUsers)
                {
                    task.UserTasks.Add(new UserTask() {
                        UserId = userId,
                        TaskId = task.TaskId
                    });
                }
                if (ModelState.IsValid)
                {
                    string fileName = string.Empty;
                    if(task.File != null)
                    {
                        string myUpload = Path.Combine(_host.WebRootPath, "Files");
                        fileName = task.File.FileName;
                        string fullPath = Path.Combine(myUpload, fileName);
                        task.File.CopyTo(new FileStream(fullPath, FileMode.Create));
                        task.Attachment = fileName;
                    }
                    _UOW.tasks.Add(task);
                    _UOW.SaveChanges();
                    return RedirectToAction("index");
                }
            }
            return View();
        }


        public IActionResult Edit(int TaskId)
        {
            IEnumerable<Project> projects = _UOW.projects.GetAll();
            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name");

            IEnumerable<User> users = _UOW.users.GetAll();
            ViewBag.Users = new SelectList(users, "UserId", "Name", new { });

            // This takes long time when displaying the edit view
            Task tbEditedTask = _UOW.tasks.GetTaskById(TaskId);

            // This takes less time , but it will not show the Old assignd users when editing 
            //Task tbEditedTask = _UOW.tasks.GetAll("SubTasks.Users").FirstOrDefault(t => t.TaskId == TaskId);

            if (tbEditedTask == null)
            {
                return NotFound();
            }
            return View(tbEditedTask);

            
        }



        [HttpPost]
        public IActionResult Edit(Task editedTask, int[] AssignedUsers)
        {
            // Retrieve the original task from the database with related UserTasks and SubTasks
            Task orgTask = _UOW.tasks.GetAll(query => query
                .Include(t => t.UserTasks)  
                .Include(t => t.SubTasks))  
                .FirstOrDefault(t => t.TaskId == editedTask.TaskId);

            if (orgTask == null)
            {
                return NotFound(); 
            }
            string fileName = string.Empty;
            if (editedTask.File != null)
            {
                string myUpload = Path.Combine(_host.WebRootPath, "Files");
                fileName = editedTask.File.FileName;
                string fullPath = Path.Combine(myUpload, fileName);
               editedTask.File.CopyTo(new FileStream(fullPath, FileMode.Create));
                orgTask.Attachment = editedTask.File.FileName;
            }
           
            orgTask.Description = editedTask.Description;
            orgTask.DueDate = editedTask.DueDate;
            orgTask.ProjectId = editedTask.ProjectId;

            // Handle Task assigned users
            if (AssignedUsers != null)
            {
                
                var existingUserTasks = orgTask.UserTasks.ToList();

                // Track which UserTasks to remove (users not in the new AssignedUsers list)
                var userTasksToRemove = existingUserTasks.Where(ut => !AssignedUsers.Contains(ut.UserId)).ToList();
                foreach (var userTask in userTasksToRemove)
                {
                    orgTask.UserTasks.Remove(userTask); // Remove users not in the new list
                }

                // Add new UserTasks if they do not already exist
                foreach (var userId in AssignedUsers)
                {
                    // Check if the UserTask for the given TaskId and UserId already exists
                    var userTaskExists = existingUserTasks.Any(ut => ut.UserId == userId);

                    if (!userTaskExists)
                    {
                        orgTask.UserTasks.Add(new UserTask
                        {
                            UserId = userId,
                            TaskId = editedTask.TaskId
                        });
                    }
                }
            }

            // Handle Subtasks
            if (editedTask.SubTasks != null)
            {
                // Clear existing subtasks to prevent duplicates
                orgTask.SubTasks.Clear(); 

                foreach (var editedSubTask in editedTask.SubTasks)
                {
                    var newSubTask = new SubTask
                    {
                        SubTaskId = editedSubTask.SubTaskId, // Ensure we set the ID if editing
                        Name = editedSubTask.Name,
                        Description = editedSubTask.Description,
                        DueDate = editedSubTask.DueDate,
                        Users = new List<User>() 
                    };

                 
                    var existingSubTask = _UOW.subtasks.GetAll(query => query
                        .Include(st => st.Users) 
                    ).FirstOrDefault(st => st.SubTaskId == editedSubTask.SubTaskId);
                    var existingUserIds = existingSubTask?.Users.Select(u => u.UserId).ToList() ?? new List<int>();

                    // Handle Assigned Users for each SubTask
                    if (editedSubTask.SelectedUserIds != null)
                    {
                        foreach (var subUserId in editedSubTask.SelectedUserIds)
                        {
                            // Only add the user if they are not already assigned to the subtask
                            if (!existingUserIds.Contains(subUserId))
                            {
                                var selectedUser = _UOW.users.GetBy(subUserId);
                                if (selectedUser != null) // Ensure the user exists
                                {
                                    newSubTask.Users.Add(selectedUser);
                                }
                            }
                        }
                    }

                    orgTask.SubTasks.Add(newSubTask);
                }
            }
            
            
                _UOW.SaveChanges(); 
                return RedirectToAction("Index"); 
            

            // If we got this far, something failed, redisplay the form
            return View(editedTask);
        }



        public IActionResult AddSubTask(int TaskId)
    {

        ViewBag.TaskId = TaskId;
        IEnumerable<User> users = _UOW.users.GetAll();
        ViewBag.usersList = new SelectList(users, "UserId", "Name");
        return View();
    }


    [HttpPost]
    public IActionResult AddSubTask(SubTask subtask, int[] AssignedUsers)
    {
        if (subtask != null && AssignedUsers != null)
        {
            foreach (var userId in AssignedUsers)
            {
                var user = _UOW.users.GetBy(userId);
                if (user != null)
                {
                    subtask.Users.Add(user);
                }
            }
                if (ModelState.IsValid)
                {
                    _UOW.subtasks.Add(subtask);
                _UOW.SaveChanges();
                return RedirectToAction("index");
                }
            }
        return View();
    }

        public ActionResult Delete(int id)
        {
            _UOW.tasks.Delete(id);
            _UOW.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

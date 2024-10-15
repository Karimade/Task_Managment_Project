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

        // List of tasks Action
        public IActionResult Index()
        {

            var tasks = _UOW.tasks.GetAll(
             t => t.Include(ut => ut.UserTasks).ThenInclude(u => u.User)
             .Include(p => p.Project).Include(s => s.SubTasks)
                );
            return View(tasks);
        }

        // Insert a new task Action
        public IActionResult Insert()
        {
            // Making select lists
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
                // Handling the Assigned users
                foreach (var userId in AssignedUsers)
                {
                    task.UserTasks.Add(new UserTask() {
                        UserId = userId,
                        TaskId = task.TaskId
                    });
                }
                if (ModelState.IsValid)
                {
                    // Handling the files
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

        // Edit a task Action
        public IActionResult Edit(int TaskId)
        {
            IEnumerable<Project> projects = _UOW.projects.GetAll();
            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name");

            IEnumerable<User> users = _UOW.users.GetAll();
            ViewBag.Users = new SelectList(users, "UserId", "Name", new { });

            // This takes long time when displaying the edit view
            //Task tbEditedTask = _UOW.tasks.GetTaskById(TaskId);

            // This takes less time , but it will not show the Old assigned users when editing 
            Task tbEditedTask = _UOW.tasks.GetAll("SubTasks.Users").FirstOrDefault(t => t.TaskId == TaskId);

            if (tbEditedTask == null) // if the task to be edited is null return not found 
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
                .Include(t => t.SubTasks).ThenInclude(st=>st.Users))  
                .FirstOrDefault(t => t.TaskId == editedTask.TaskId);

            if (orgTask == null)
            {
                return NotFound(); 
            }
            // -------------Handling the file upload----------------------
            string fileName = string.Empty;
            if (editedTask.File != null)
            {
                // combine the root Path with the Files folder
                string myUpload = Path.Combine(_host.WebRootPath, "Files");
                // getting the file name 
                fileName = editedTask.File.FileName;
                // combine the file name to the previous path to make the full path
                string fullPath = Path.Combine(myUpload, fileName);
                // uploading the file on the Path on the server
                editedTask.File.CopyTo(new FileStream(fullPath, FileMode.Create));
                // save the file name in the attachment field in the database
                orgTask.Attachment = editedTask.File.FileName;
            }
            // updating other fields
            orgTask.Description = editedTask.Description;
            orgTask.DueDate = editedTask.DueDate;
            orgTask.ProjectId = editedTask.ProjectId;

            // -------------Handling Task assigned users---------------------
            // First:  Remove the users in Original userTasks that doesn't exist in the new assigned users
            // Second: Add the userTasks that exist in the newAssigned users that doesn't exists in the Original userTasks.
            // by doing this we ensure that no conflict will happen in updating the Assigned users
            if (AssignedUsers != null)
            {   
                // getting the original Assigned users
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
                        // Add new userTask if it doesn't exist
                        orgTask.UserTasks.Add(new UserTask
                        {
                            UserId = userId,
                            TaskId = editedTask.TaskId
                        });
                      
                    }
                }
            }

            // -----------Handle Subtasks------------------------------------
            if (editedTask.SubTasks != null)
            {
                // Clear existing subtasks to prevent duplicates
                orgTask.SubTasks.Clear();

                foreach (var editedSubTask in editedTask.SubTasks)
                {
                    // add new subtask object with the the new edited fields
                    var newSubTask = new SubTask
                    {
                        SubTaskId = editedSubTask.SubTaskId, // Ensure we set the ID if editing
                        Name = editedSubTask.Name,
                        Description = editedSubTask.Description,
                        DueDate = editedSubTask.DueDate,
                        Users = new List<User>()
                    };
                    //---------------------------Handling subtask users----------------------------------------
                    // getting the current subtask to be edited
                    var existingSubTask = _UOW.subtasks.GetAll(query => query
                        .Include(st => st.Users)
                    ).FirstOrDefault(st => st.SubTaskId == editedSubTask.SubTaskId);

                    // getting the old users of the subtask to be edited, if no users exist return empty list
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
                    // finally adding the new subtask after making all the checking required
                    orgTask.SubTasks.Add(newSubTask);
                }
            }
            if (ModelState.IsValid) 
            {
                _UOW.tasks.Update(orgTask);
                _UOW.SaveChanges(); 
                return RedirectToAction("Index"); 
            
            }
            // If we got this far, something failed, redisplay the form
            return View(editedTask);
        }


        // Add new subTask Action
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

        // Delete Task Action
        public ActionResult Delete(int id)
        {
            _UOW.tasks.Delete(id);
            _UOW.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

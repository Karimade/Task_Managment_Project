using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Task_Management_system.Data.Repositories;
using Task_Management_system.Data.UnitOfWork;
using Task = Task_Management_system.Models.Task;
namespace Task_Management_system.Controllers
{
   //############################################## API ###########################################################
    [Route("api/[controller]")]
    [ApiController]
    public class TaskApiController : ControllerBase
    {
        private readonly IUnitOfWork _UOW;
        private readonly ILogger<TaskApiController> _logger;

        public TaskApiController(IUnitOfWork UOW, ILogger<TaskApiController> logger)
        {
            _UOW = UOW;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetTasks(int? projectId, int? userId, DateTime? dueDateFrom, DateTime? dueDateTo, int page = 1, int pageSize = 10)
        {
            try
            {
                var tasks = _UOW.tasks.GetFilteredTasks(projectId, userId, dueDateFrom, dueDateTo);
                //// Check if tasks are null and return an empty array if true
                if (tasks == null)
                {
                    return NotFound("No Such Data Exists!");
                }
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving tasks.");
            }
        }

        
        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            try
            {
                var task = _UOW.tasks.GetTaskById(id);
                if (task == null)
                {
                    return NotFound(new { Message = $"Task with ID {id} not found." });
                }
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the task.");
            }
        }


        // https://localhost:7240/api/taskapi/overdue/{Count}
        [HttpGet("overdue/{count}")]
        public IActionResult GetOverdueTasks(int count)
        {
            try
            {
                var overdueTasks = _UOW.tasks.GetOverdueTasks(count);
                return Ok(overdueTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue tasks.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving overdue tasks.");
            }
        }
    }
}

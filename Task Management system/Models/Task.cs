using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management_system.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "The Name field is Required!")]
        [StringLength(50, ErrorMessage = "Please Enter a Valid Task Name", MinimumLength = 3)]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "The DueDate field is Required!")]
        public DateTime DueDate {  get; set; }
        
        public string? Attachment {  get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }


        // ==============================Handling the relations==============================================
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual List<SubTask>? SubTasks {  get; set; }  = new List<SubTask>();
        
        // Navigation property for users assigned to this task
        public virtual ICollection<UserTask>? UserTasks { get; set; } = new List<UserTask>();

    }
}

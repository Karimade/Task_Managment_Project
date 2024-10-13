using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Task_Management_system.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "The Name field is Required!")]
        [StringLength(30, ErrorMessage = "Please Enter a Valid User Name", MinimumLength = 3)]
        public string Name { get; set; }
        public string? Email { get; set; }

        // Navigation property for tasks assigned to the user
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<UserTask> UserTasks { get; set; }= new List<UserTask>();
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<SubTask>? SubTasks { get; set; } = new HashSet<SubTask>();

    }
}

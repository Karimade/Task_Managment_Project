using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Task_Management_system.Models
{

    public class SubTask
    {
        [Key]
        public int SubTaskId { get; set; }

        [Required(ErrorMessage = "The Name field is Required!")]
        [StringLength(30, ErrorMessage = "Please Enter a Valid SubTask Name", MinimumLength = 3)]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "The DueDate field is Required!")]
        public DateTime DueDate { get; set; }

        [NotMapped]
        public List<int>? SelectedUserIds { get; set; } // for handling 

        [ForeignKey("Tasks")]
        public int TaskId { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Task? Tasks { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual List<User> Users { get; set; }= new List<User>();
    }
}

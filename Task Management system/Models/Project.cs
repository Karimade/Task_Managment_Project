using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Task_Management_system.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage ="The Name field is Required!")]
        [StringLength(30,ErrorMessage ="Please Enter a Valid Project Name",MinimumLength =3)]
        public string  Name  { get; set; }
        public string? Description { get; set; }

        // Navigation property for tasks
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}

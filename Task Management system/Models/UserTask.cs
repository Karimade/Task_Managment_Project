using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Task_Management_system.Models
{
    // For Many to Many relation [User <==> Task]
    public class UserTask
    {
        //public int UserTaskId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
       
        public virtual User User { get; set; }

        [ForeignKey("Task")]
        public int TaskId { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Task Task { get; set; }
    }
}

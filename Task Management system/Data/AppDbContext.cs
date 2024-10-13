using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task_Management_system.Models;
using Task = Task_Management_system.Models.Task;

namespace Task_Management_system.Data
{
    public class AppDbContext:IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {}

        public DbSet<Project> projects { get; set; }
        public DbSet<Task> tasks { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<SubTask> subTasks { get; set; }
        public DbSet<UserTask> userTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTask>()
                .HasKey(ut => new { ut.UserId, ut.TaskId }); // Composite Primary key

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.Task)
                .WithMany(t => t.UserTasks)
                .HasForeignKey(t => t.TaskId);

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u=>u.UserTasks)
                .HasForeignKey(u => u.UserId);

          base.OnModelCreating(modelBuilder); 

        
        }
    }
}

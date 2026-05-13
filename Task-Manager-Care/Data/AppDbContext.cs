using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        //Models
        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ClientCategory> ClientCategories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceCategory>().HasData(
                new ServiceCategory { Id = 1, Name = "Investment" },
                new ServiceCategory { Id = 2, Name = "Insurance" },
                new ServiceCategory { Id = 3, Name = "Tax" },
                new ServiceCategory { Id = 4, Name = "Real Estate" },
                new ServiceCategory { Id = 5, Name = "Morgage" },
                new ServiceCategory { Id = 6, Name = "Travel Insurance" },
                new ServiceCategory { Id = 7, Name = "Financial Planners" }
            );
            modelBuilder.Entity<ClientCategory>().HasData(
                new ClientCategory { Id = 1, client_type = "Existing Client" },
                new ClientCategory { Id = 2, client_type = "New Client" }
            );               
             modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "Pending" },
                new Status { Id = 2, Name = "In Progress" },
                new Status { Id = 3, Name = "Completed" }
             );
        }
    }
}


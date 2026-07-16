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
        public DbSet<Client> Clients { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ClientCategory> ClientCategories { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }
        public DbSet<CommentEntity> Comments => Set<CommentEntity>();
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ServiceCategory>().HasData(
                new ServiceCategory { Id = 1, Name = "Investments" },
                new ServiceCategory { Id = 2, Name = "Insurance" },
                new ServiceCategory { Id = 3, Name = "Tax" },
                new ServiceCategory { Id = 4, Name = "Real Estate" },
                new ServiceCategory { Id = 5, Name = "Morgage" },
                new ServiceCategory { Id = 6, Name = "Travel Insurance" },
                new ServiceCategory { Id = 7, Name = "Financial Planners" },
                new ServiceCategory { Id = 8, Name = "Others" }
            );

            modelBuilder.Entity<ClientCategory>().HasData(
                new ClientCategory { Id = 1, ClientType = "Existing Client" },
                new ClientCategory { Id = 2, ClientType = "New Client" }
            );

            modelBuilder.Entity<Status>().HasData(
               new Status { Id = 1, Name = "Assigned" },
               new Status { Id = 2, Name = "Pending" },
               new Status { Id = 3, Name = "In Progress" },
               new Status { Id = 4, Name = "Completed" },
               new Status { Id = 5, Name = "Cancelled" }
            );
            modelBuilder.Entity<Priority>().HasData( 
                new Priority { Id = 1, Name = "Low" },
                new Priority { Id = 2, Name = "Medium" },
                new Priority { Id = 3, Name = "High" },
                new Priority { Id = 4, Name = "Urgent" }

            );
            //To restrict someone if by mistake if they delete the employee and they have assigned too many
            //tasks so the data will be lost to stop that use the restrict behaviour
            modelBuilder.Entity<Client>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Client>()
                .HasOne(c => c.ClientCategory)
                .WithMany()
                .HasForeignKey(c => c.ClientCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaskItem>()
                .HasOne(t=>t.AssignedTo)
                .WithMany()
                .HasForeignKey(t=>t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId);
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TaskItem)
                .WithMany()
                .HasForeignKey(n => n.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Client)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
    //start
            await context.Database.MigrateAsync();
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("Database already contains data. Skipping seeding.");
                return;
            }
            // 1. USERS

            var passwordHasher = new PasswordHasher<User>();

            var users = new List<User>();

            // -------------------------
            // SUPER ADMIN
            // -------------------------

            var superAdmin = new User
            {
                Name = "Test Super Admin",
                Email = "medium.superadmin@careinsurance.ca",
                Role = "SuperAdmin",
                IsActive = true,
                DOB = new DateOnly(1985, 5, 15),
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            };

            superAdmin.Password = passwordHasher.HashPassword(
                superAdmin,
                "Password123!"
            );

            users.Add(superAdmin);

            // -------------------------
            // ADMIN
            // -------------------------

            var admin = new User
            {
                Name = "Test Admin",
                Email = "medium.admin@careinsurance.ca",
                Role = "Admin",
                IsActive = true,
                DOB = new DateOnly(1988, 8, 20),
                CreatedAt = DateTime.UtcNow.AddMonths(-10)
            };

            admin.Password = passwordHasher.HashPassword(
                admin,
                "Password123!"
            );

            users.Add(admin);

            // -------------------------
            // 20 EMPLOYEES
            // -------------------------

            string[] firstNames =
            {
                "Arjun",
                "Emma",
                "Liam",
                "Olivia",
                "Noah",
                "Ava",
                "Ethan",
                "Sophia",
                "Lucas",
                "Mia",
                "Mason",
                "Isabella",
                "Logan",
                "Charlotte",
                "James",
                "Amelia",
                "Benjamin",
                "Harper",
                "Henry",
                "Evelyn"
            };

            string[] lastNames =
            {
                "Patel",
                "Brown",
                "Smith",
                "Johnson",
                "Wilson",
                "Taylor",
                "Singh",
                "Martin",
                "Lee",
                "Sharma",
                "Davis",
                "Clark",
                "Thomas",
                "White",
                "Moore",
                "Anderson",
                "Jackson",
                "Walker",
                "Harris",
                "Miller"
            };

            for (int i = 0; i < 50; i++)
            {
                var name = $"{firstNames[i % firstNames.Length]} " +
                            $"{lastNames[i % lastNames.Length]}";

                var emailName = name
                    .ToLower()
                    .Replace(" ", ".");

                var employee = new User
                {
                    Name = name,
                    Email = $"medium.{emailName}@careinsurance.ca",
                    Role = "Employee",
                    IsActive = true,
                    DOB = new DateOnly(
                        1985 + (i % 12),
                        1 + (i % 12),
                        1 + (i % 25)
                    ),
                    CreatedAt = DateTime.UtcNow.AddMonths(-(i + 1))
                };

                employee.Password = passwordHasher.HashPassword(
                    employee,
                    "Password123!"
                );

                users.Add(employee);
            }

            context.Users.AddRange(users);

            await context.SaveChangesAsync();

            Console.WriteLine($"Users created: {users.Count}");

            // =========================================================
            // GET USERS
            // =========================================================

            var allUsers = await context.Users
                .OrderBy(u => u.Id)
                .ToListAsync();

            var employees = allUsers
                .Where(u => u.Role == "Employee" && u.IsActive)
                .ToList();

            var adminUsers = allUsers
                .Where(u =>
                    (u.Role == "Admin" || u.Role == "SuperAdmin")
                    && u.IsActive)
                .ToList();

            // =========================================================
            // 2. CLIENTS
            // =========================================================

            Console.WriteLine("Creating clients...");

            var clients = new List<Client>();

            string[] clientFirstNames =
            {
                "James",
                "Robert",
                "William",
                "Linda",
                "Patricia",
                "Jennifer",
                "Thomas",
                "Richard",
                "Barbara",
                "Susan",
                "Joseph",
                "Charles",
                "Daniel",
                "Nancy",
                "Karen",
                "Matthew",
                "Lisa",
                "Anthony",
                "Betty",
                "Mark"
            };

            string[] clientLastNames =
            {
                "Smith",
                "Johnson",
                "Williams",
                "Brown",
                "Davis",
                "Miller",
                "Wilson",
                "Moore",
                "Taylor",
                "Anderson"
            };

            for (int i = 1; i <= 1000; i++)
            {
                var firstName =
                    clientFirstNames[
                        (i - 1) % clientFirstNames.Length
                    ];

                var lastName =
                    clientLastNames[
                        (i - 1) % clientLastNames.Length
                    ];

                var client = new Client
                {
                    ClientName =
                        $"{firstName} {lastName} {i}",

                    //ses the ternary operator.The general format is:
                    //condition? valueIfTrue : valueIfFalse
                    //if i is even Is i even?If yes: ClientCategoryId = 2, If no:ClientCategoryId = 1
                    ClientCategoryId =
                        i % 2 == 0 ? 2 : 1,

                    //Every 4th client gets a company name.Everyone else gets null.
                    CompanyName =
                        i % 4 == 0
                            ? $"Test Company {i}"
                            : null,

                    PhoneNumber =
                        $"416-555-{i:0000}",

                    Email =
                        $"medium.client{i}@example.com",

                    //if i=1 then, 100+1=101;
                    //ex: 101 Main street, toronto,...,150 Main street, toronto
                    Address =
                        $"{100 + i} Main Street, Toronto",

                    CreatedOn =
                        DateTime.UtcNow.AddDays(-i),

                    CreatedById =
                        adminUsers[i % adminUsers.Count].Id,

                    IsDeleted = false
                };

                clients.Add(client);
            }

            context.Clients.AddRange(clients);

            await context.SaveChangesAsync();

            Console.WriteLine($"Clients created: {clients.Count}");

            // =========================================================
            // 3. TASKS
            // =========================================================

            Console.WriteLine("Creating tasks...");

            var savedClients = await context.Clients
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.ClientId)
                .ToListAsync();

            var tasks = new List<TaskItem>();

            string[] taskDescriptions =
            {
                "Follow up with client regarding insurance options.",
                "Review client's investment portfolio.",
                "Schedule a meeting with client.",
                "Prepare insurance quotation.",
                "Contact client for missing documents.",
                "Review life insurance requirements.",
                "Follow up regarding application.",
                "Prepare investment documentation.",
                "Call client regarding policy renewal.",
                "Complete client onboarding.",
                "Review mortgage application.",
                "Prepare financial planning documents.",
                "Contact client regarding investment strategy.",
                "Follow up on outstanding paperwork.",
                "Review client's existing insurance policy."
            };

            for (int i = 1; i <= 5000; i++)
            {
                var client =
                    savedClients[
                        (i - 1) % savedClients.Count
                    ];

                var assignedEmployee =
                    employees[
                        (i - 1) % employees.Count
                    ];

                var createdBy =
                    adminUsers[
                        (i - 1) % adminUsers.Count
                    ];

                var createdDate =
                    DateTime.UtcNow.AddDays(-(i % 180));

                // -------------------------
                // STATUS
                // -------------------------

                int statusId;

                if (i % 20 == 0)
                {
                    statusId = 5; // Cancelled
                }
                else if (i % 5 == 0)
                {
                    statusId = 4; // Completed
                }
                else if (i % 3 == 0)
                {
                    statusId = 3; // In Progress
                }
                else if (i % 7 == 0)
                {
                    statusId = 1; // Assigned
                }
                else
                {
                    statusId = 2; // Pending
                }

                // -------------------------
                // DUE DATE
                // -------------------------

                DateTime dueDate;

                if (statusId == 4)
                {
                    // Completed tasks
                    dueDate =
                        createdDate.AddDays(5 + (i % 10));
                }
                else if (i % 8 == 0)
                {
                    // Overdue
                    dueDate =
                        DateTime.UtcNow.AddDays(
                            -(1 + (i % 30))
                        );
                }
                else if (i % 10 == 0)
                {
                    // Due soon
                    dueDate =
                        DateTime.UtcNow.AddDays(
                            1 + (i % 3)
                        );
                }
                else
                {
                    // Future
                    dueDate =
                        DateTime.UtcNow.AddDays(
                            5 + (i % 30)
                        );
                }

                var task = new TaskItem
                {
                    ClientId =
                        client.ClientId,

                    ClientName =
                        client.ClientName,

                    ClientCategoryId =
                        client.ClientCategoryId,

                    PhoneNumber =
                        client.PhoneNumber,

                    Email =
                        client.Email,

                    AssignedToId =
                        assignedEmployee.Id,

                    CreatedById =
                        createdBy.Id,

                    Created_On =
                        createdDate,

                    Updated_On =
                        i % 4 == 0
                            ? createdDate.AddDays(1)
                            : null,

                    CompletedOn =
                        statusId == 4
                            ? createdDate.AddDays(2)
                            : null,

                    StatusId =
                        statusId,

                    task_Description =
                        taskDescriptions[
                            (i - 1) %
                            taskDescriptions.Length
                        ],

                    LongDescription =
                        $"Medium test dataset task #{i}. " +
                        $"This task was generated for CRM performance and functional testing.",

                    DueDate =
                        dueDate,

                    ServiceCategoryId =
                        ((i - 1) % 8) + 1,

                    PriorityId =
                        ((i - 1) % 4) + 1,

                    LastOverdueEmailSentAt =
                        null
                };

                tasks.Add(task);
            }

            context.Tasks.AddRange(tasks);

            await context.SaveChangesAsync();

            Console.WriteLine($"Tasks created: {tasks.Count}");

            // =========================================================
            // 4. ACTIVITIES
            // =========================================================

            Console.WriteLine("Creating activities...");

            var savedTasks = await context.Tasks
                .OrderBy(t => t.Id)
                .ToListAsync();

            var activities = new List<Activity>();

            foreach (var task in savedTasks)
            {
                var user =
                    allUsers[
                        task.Id % allUsers.Count
                    ];

                // Task Created
                activities.Add(new Activity
                {
                    TaskId = task.Id,

                    ChangedById = user.Id,

                    Action = "Task Created",

                    ChangedAt =
                        task.Created_On,

                    Description =
                        $"{user.Name} created the task."
                });

                // Task Updated
                if (task.Id % 2 == 0)
                {
                    activities.Add(new Activity
                    {
                        TaskId = task.Id,

                        ChangedById = user.Id,

                        Action = "Task Updated",

                        ChangedAt =
                            task.Created_On.AddHours(2),

                        Description =
                            $"{user.Name} updated the task."
                    });
                }

                // Task Assigned
                if (task.Id % 3 == 0)
                {
                    activities.Add(new Activity
                    {
                        TaskId = task.Id,

                        ChangedById = user.Id,

                        Action = "Task Assigned",

                        ChangedAt =
                            task.Created_On.AddHours(1),

                        Description =
                            $"{user.Name} assigned the task."
                    });
                }

                // Completed
                if (task.StatusId == 4)
                {
                    activities.Add(new Activity
                    {
                        TaskId = task.Id,

                        ChangedById = user.Id,

                        Action = "Task Completed",

                        ChangedAt =
                            task.CompletedOn
                            ?? task.Created_On.AddDays(2),

                        Description =
                            $"{user.Name} completed the task."
                    });
                }

                // Cancelled
                if (task.StatusId == 5)
                {
                    activities.Add(new Activity
                    {
                        TaskId = task.Id,

                        ChangedById = user.Id,

                        Action = "Task Cancelled",

                        ChangedAt =
                            task.Created_On.AddDays(1),

                        Description =
                            $"{user.Name} cancelled the task."
                    });
                }
            }

            context.TaskHistories.AddRange(activities);

            await context.SaveChangesAsync();

            Console.WriteLine(
                $"Activities created: {activities.Count}"
            );

            // =========================================================
            // 5. COMMENTS
            // =========================================================

            Console.WriteLine("Creating comments...");

            var comments = new List<Remarks>();

            string[] commentTexts =
            {
                "Called the client and discussed the next steps.",
                "Client requested additional information.",
                "Documents have been received.",
                "Follow-up required next week.",
                "Client is reviewing the proposal.",
                "Meeting scheduled with the client.",
                "Waiting for client confirmation.",
                "Information sent to the client.",
                "Client confirmed the requirements.",
                "Follow-up completed successfully."
            };

            // Approximately 1,500 comments
            for (int i = 1; i <= 1500; i++)
            {
                var task =
                    savedTasks[
                        (i - 1) % savedTasks.Count
                    ];

                var user =
                    allUsers[
                        (i - 1) % allUsers.Count
                    ];

                comments.Add(new Remarks
                {
                    TaskId = task.Id,

                    UserId = user.Id,

                    Text =
                        commentTexts[
                            (i - 1) %
                            commentTexts.Length
                        ],

                    CreatedAt =
                        task.Created_On.AddHours(
                            1 + (i % 48)
                        )
                });
            }

            context.Comments.AddRange(comments);

            await context.SaveChangesAsync();

            Console.WriteLine(
                $"Comments created: {comments.Count}"
            );

            // =========================================================
            // 6. NOTIFICATIONS
            // =========================================================

            Console.WriteLine("Creating notifications...");

            var notifications = new List<Notification>();

            for (int i = 1; i <= 750; i++)
            {
                var task =
                    savedTasks[
                        (i - 1) % savedTasks.Count
                    ];

                var user =
                    employees[
                        (i - 1) % employees.Count
                    ];

                notifications.Add(new Notification
                {
                    UserId =
                        user.Id,

                    CreatedById =
                        task.CreatedById,

                    TaskId =
                        task.Id,

                    Type =
                        i % 4 == 0
                            ? "TaskUpdated"
                            : "TaskAssigned",

                    Title =
                        i % 4 == 0
                            ? "Task Updated"
                            : "New Task Assigned",

                    Message =
                        i % 4 == 0
                            ? $"Task for {task.ClientName} has been updated."
                            : $"You have been assigned a task for {task.ClientName}.",

                    IsRead =
                        i % 3 == 0,

                    CreatedOn =
                        DateTime.UtcNow.AddDays(
                            -(i % 30)
                        )
                });
            }

            context.Notifications.AddRange(notifications);

            await context.SaveChangesAsync();

            Console.WriteLine(
                $"Notifications created: {notifications.Count}"
            );

            // =========================================================
            // FINAL COUNTS
            // =========================================================

            Console.WriteLine("");
            Console.WriteLine("==========================================");
            Console.WriteLine("       FINAL DATABASE COUNTS");
            Console.WriteLine("==========================================");

            Console.WriteLine(
                $"Users          : {await context.Users.CountAsync()}"
            );

            Console.WriteLine(
                $"Clients        : {await context.Clients.CountAsync()}"
            );

            Console.WriteLine(
                $"Tasks          : {await context.Tasks.CountAsync()}"
            );

            Console.WriteLine(
                $"Activities     : {await context.TaskHistories.CountAsync()}"
            );

            Console.WriteLine(
                $"Comments       : {await context.Comments.CountAsync()}"
            );

            Console.WriteLine(
                $"Notifications  : {await context.Notifications.CountAsync()}"
            );

            Console.WriteLine("==========================================");
            Console.WriteLine("       CRM MEDIUM DATA SEEDING COMPLETE");
            Console.WriteLine("==========================================");
        }
    }
}
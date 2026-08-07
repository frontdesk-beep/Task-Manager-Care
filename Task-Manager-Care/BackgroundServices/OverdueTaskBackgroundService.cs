using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Services;

namespace Task_Manager_Care.BackgroundServices
{
    public class OverdueTaskBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueTaskBackgroundService> _logger;


        public OverdueTaskBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OverdueTaskBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private async Task DelayUntilNineAM(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;

            var nextRun = now.Date.AddHours(9); // Today 9:00 AM

            // If 9 AM already passed, schedule tomorrow 9 AM
            if (now >= nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);
        }
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //WAIT UNTILL 9 AM FIRST
                //in every 24 hours - calculates according to the secrver time 
                //await Task.Delay(
                //    TimeSpan.FromHours(24),
                //    stoppingToken);
                //for automatic everyday morning 9 am and second email next day morning 9 am - if the task is still not completed.
                //await DelayUntilNineAM(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                    var emailService = scope.ServiceProvider
                        .GetRequiredService<IEmailService>();


                    var overdueTasks = await context.Tasks
                        .Include(t => t.AssignedTo)
                        .Include(t => t.Status)
                        .Include(t => t.ServiceCategory)
                        .Where(t =>
                            t.DueDate.Date < DateTime.Today &&
                            t.Status.Name != "Completed" &&
                            (
                                t.LastOverdueEmailSentAt == null ||
                                t.LastOverdueEmailSentAt.Value.Date < DateTime.Today
                            )
                        )
                        .ToListAsync();


                    foreach (var task in overdueTasks)
                    {
                        if (task.AssignedTo != null)
                        {
                            await emailService.SendEmailAsync(
                                task.AssignedTo.Email,
                                "Overdue Task Alert",
                                $@"
                                <h2>Overdue Task</h2>

                                <p>Hello {task.AssignedTo.Name}</p>

                                <p>
                                Task <b>{task.ClientName}</b> is overdue.
                                </p>
                                
                                <p>
                                Task Name: {task.task_Description}
                                </p>

                                <p>
                                Task Name: {task.ServiceCategory?.Name}
                                </p>

                                <p>
                                Due Date: {task.DueDate}
                                </p>
                                "
                            );
                            task.LastOverdueEmailSentAt = DateTime.Now;
                        }
                    }
                    await context.SaveChangesAsync();

                }


            }
        }
    }
}
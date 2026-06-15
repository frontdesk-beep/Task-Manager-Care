using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Manager_Care.Domain.Models;

public class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ChangedById { get; set; }
    public int? OldStatusId { get; set; }
    public int? NewStatusId { get; set; }
    public string Note { get; set; }
    public DateTime ChangedAt { get; set; }
}

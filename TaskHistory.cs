using System;

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

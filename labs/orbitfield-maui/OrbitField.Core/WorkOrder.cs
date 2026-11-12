namespace OrbitField.Core;

public enum WorkStatus { Open, InProgress, Completed }

public sealed class WorkOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string CustomerAlias { get; set; } = "";
    public WorkStatus Status { get; set; } = WorkStatus.Open;
    public bool PendingSync { get; set; } = true;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool Advance()
    {
        if (Status == WorkStatus.Completed) return false;
        Status = Status == WorkStatus.Open ? WorkStatus.InProgress : WorkStatus.Completed;
        PendingSync = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return true;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title) || Title.Trim().Length > 160)
            throw new ArgumentException("Title must have 1-160 characters.");
        if (string.IsNullOrWhiteSpace(CustomerAlias) || CustomerAlias.Length > 80)
            throw new ArgumentException("Customer alias must have 1-80 characters.");
    }
}

namespace OrbitField.Core;

public static class SyncLogic
{
    public static IReadOnlyList<WorkOrder> Pending(IEnumerable<WorkOrder> orders)
        => orders.Where(x => x.PendingSync).OrderBy(x => x.UpdatedAtUtc).ToArray();

    public static bool ShouldAcceptIncoming(DateTimeOffset local, DateTimeOffset incoming)
        => incoming >= local;
}

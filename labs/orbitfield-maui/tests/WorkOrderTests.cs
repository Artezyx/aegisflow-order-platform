using OrbitField.Core;
using Xunit;

public class WorkOrderTests
{
    [Fact] public void AdvancesOnlyThroughValidStages()
    {
        var work = new WorkOrder{Title="Inspect vehicle",CustomerAlias="Customer A"};
        Assert.True(work.Advance());
        Assert.Equal(WorkStatus.InProgress,work.Status);
        Assert.True(work.Advance());
        Assert.Equal(WorkStatus.Completed,work.Status);
        Assert.False(work.Advance());
    }

    [Fact] public void RequiresValidTitle()
        => Assert.Throws<ArgumentException>(() => new WorkOrder{Title="",CustomerAlias="A"}.Validate());

    [Fact] public void OrdersPendingSyncChronologically()
    {
        var older=new WorkOrder{UpdatedAtUtc=DateTimeOffset.UtcNow.AddDays(-1)};
        var newer=new WorkOrder{UpdatedAtUtc=DateTimeOffset.UtcNow};
        var synced=new WorkOrder{PendingSync=false};
        var pending=SyncLogic.Pending(new[]{newer,synced,older});
        Assert.Equal(new[]{older,newer},pending);
    }

    [Fact] public void RejectsStaleServerTimestamp()
        => Assert.False(SyncLogic.ShouldAcceptIncoming(DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddDays(-1)));
}

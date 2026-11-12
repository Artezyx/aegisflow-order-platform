using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrbitField.Core;

namespace OrbitField.App.ViewModels;

public sealed partial class OrderRow : ObservableObject
{
    private readonly Func<WorkOrder,Task> _advance;
    public WorkOrder Order {get;}
    public OrderRow(WorkOrder order,Func<WorkOrder,Task> advance)
    {Order=order;_advance=advance;}

    public string Title=>Order.Title;
    public string CustomerAlias=>Order.CustomerAlias;
    public string Status=>Order.Status.ToString();
    public string SyncDisplay=>Order.PendingSync?"Pending offline sync":"Synced";

    [RelayCommand]
    private async Task Advance()=>await _advance(Order);
}

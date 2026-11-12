using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrbitField.Core;
using OrbitField.App.Services;

namespace OrbitField.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly LocalStore _store;
    private readonly SyncService _sync;
    private readonly SemaphoreSlim _gate=new(1,1);

    [ObservableProperty] private string title="";
    [ObservableProperty] private string customerAlias="";
    [ObservableProperty] private string feedback="Ready for offline work.";
    public ObservableCollection<OrderRow> Orders {get;}=new();

    public MainViewModel(LocalStore store,SyncService sync)
    {
        _store=store;_sync=sync;
        _=Refresh();
    }

    [RelayCommand]
    private async Task Create()
    {
        var order=new WorkOrder{Title=Title.Trim(),CustomerAlias=CustomerAlias.Trim()};
        try{order.Validate();}
        catch(ArgumentException e){Feedback=e.Message;return;}
        await _store.Save(order);
        Title="";CustomerAlias="";
        Feedback="Order saved locally.";
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await _gate.WaitAsync();
        try
        {
            var saved=await _store.All();
            Orders.Clear();
            foreach(var item in saved)Orders.Add(new OrderRow(item,AdvanceOrder));
        }
        catch(Exception exc){Feedback="Local storage unavailable: "+exc.Message;}
        finally{_gate.Release();}
    }

    private async Task AdvanceOrder(WorkOrder order)
    {
        if(!order.Advance()){Feedback="Order already completed.";return;}
        await _store.Save(order);
        await Refresh();
    }

    [RelayCommand]
    private async Task Sync()
    {
        Feedback="Synchronizing pending orders…";
        try
        {
            var (sent,failed)=await _sync.SyncAsync();
            Feedback=$"Synced {sent} orders; {failed} pending or conflicted.";
            await Refresh();
        }
        catch(Exception exc){Feedback="Sync failed: "+exc.Message;}
    }
}

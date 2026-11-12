using OrbitField.Core;
using SQLite;

namespace OrbitField.App.Services;

public sealed class LocalStore
{
    private readonly SQLiteAsyncConnection _db;
    private readonly SemaphoreSlim _init = new(1,1);
    private bool _ready;
    public LocalStore()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory,"orbitfield.db3");
        _db = new SQLiteAsyncConnection(path);
    }
    private async Task Initialize()
    {
        if (_ready) return;
        await _init.WaitAsync();
        try
        {
            if (_ready) return;
            await _db.CreateTableAsync<LocalOrder>();
            _ready = true;
        }
        finally { _init.Release(); }
    }

    public async Task<IReadOnlyList<WorkOrder>> All()
    {
        await Initialize();
        var rows = await _db.Table<LocalOrder>().ToListAsync();
        return rows.Select(x=>x.ToDomain()).OrderByDescending(x=>x.UpdatedAtUtc).ToList();
    }

    public async Task Save(WorkOrder order)
    {
        await Initialize();
        await _db.InsertOrReplaceAsync(LocalOrder.FromDomain(order));
    }

    public async Task MarkSynced(Guid id,DateTimeOffset version)
    {
        await Initialize();
        var row = await _db.FindAsync<LocalOrder>(id.ToString());
        if(row is null || row.UpdatedAtTicks != version.UtcDateTime.Ticks) return;
        row.PendingSync=false;
        await _db.UpdateAsync(row);
    }
}
public sealed class LocalOrder
{
    [PrimaryKey] public string Id {get;set;} = "";
    public string Title {get;set;} = "";
    public string CustomerAlias {get;set;} = "";
    public int Status {get;set;}
    public bool PendingSync {get;set;}
    public long UpdatedAtTicks {get;set;}
    public WorkOrder ToDomain()=>new(){
        Id=Guid.Parse(Id),Title=Title,CustomerAlias=CustomerAlias,
        Status=(WorkStatus)Status,PendingSync=PendingSync,
        UpdatedAtUtc=new DateTimeOffset(new DateTime(UpdatedAtTicks,DateTimeKind.Utc))
    };
    public static LocalOrder FromDomain(WorkOrder w)=>new(){
        Id=w.Id.ToString(),Title=w.Title,CustomerAlias=w.CustomerAlias,
        Status=(int)w.Status,PendingSync=w.PendingSync,UpdatedAtTicks=w.UpdatedAtUtc.UtcDateTime.Ticks
    };
}

using System.Collections.Concurrent;
using OrbitField.Core;

var builder=WebApplication.CreateBuilder(args);
var app=builder.Build();
var records=new ConcurrentDictionary<Guid, WorkOrder>();
app.MapGet("/health",()=>Results.Ok(new{status="ready",mode="local-demo"}));
app.MapGet("/api/orders",()=>Results.Ok(records.Values.OrderBy(x=>x.UpdatedAtUtc).ToArray()));
app.MapPut("/api/orders/{id:guid}",(Guid id,WorkOrder order)=>{
    if(id!=order.Id)return Results.BadRequest(new{error="ID mismatch"});
    try{order.Validate();}catch(ArgumentException exc){return Results.BadRequest(new{error=exc.Message});}
    var safe=new WorkOrder{
        Id=order.Id,Title=order.Title.Trim(),CustomerAlias=order.CustomerAlias.Trim(),
        Status=order.Status,UpdatedAtUtc=order.UpdatedAtUtc,PendingSync=false
    };
    records.AddOrUpdate(id,safe,(_,old)=>SyncLogic.ShouldAcceptIncoming(old.UpdatedAtUtc,safe.UpdatedAtUtc)?safe:old);
    var current=records[id];
    return current.UpdatedAtUtc>safe.UpdatedAtUtc
        ?Results.Conflict(new{error="Stale update",server=current})
        :Results.Ok(current);
});
app.Run();

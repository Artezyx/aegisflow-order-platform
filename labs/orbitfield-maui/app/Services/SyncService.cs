using System.Net;
using System.Net.Http.Json;
using OrbitField.Core;

namespace OrbitField.App.Services;

public sealed class SyncService
{
    private readonly LocalStore _store;
    private readonly HttpClient _http;

    // Android emulator uses 10.0.2.2 to reach the host. Production must use HTTPS + auth.
    private static string ApiUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5099" : "http://localhost:5099";

    public SyncService(LocalStore store,HttpClient http)
    {
        _store=store;_http=http;
    }

    public async Task<(int synced,int failed)> SyncAsync(CancellationToken ct=default)
    {
        int synced=0,failed=0;
        var pending=SyncLogic.Pending(await _store.All());
        foreach(var order in pending)
        {
            try
            {
                using var result=await _http.PutAsJsonAsync(
                    ApiUrl + "/api/orders/" + order.Id,order,ct);
                if(result.IsSuccessStatusCode)
                {
                    await _store.MarkSynced(order.Id,order.UpdatedAtUtc);
                    synced++;
                }
                else
                {
                    // HTTP 409 means a conflict that needs manual resolution; do not overwrite.
                    failed++;
                }
            }
            catch(HttpRequestException){failed++;}
            catch(TaskCanceledException) when(!ct.IsCancellationRequested){failed++;}
        }
        return (synced,failed);
    }
}

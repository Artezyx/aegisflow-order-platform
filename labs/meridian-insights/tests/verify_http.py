import json
import urllib.error
import urllib.request

BASE = "http://127.0.0.1:5098"

def fetch(path):
    with urllib.request.urlopen(BASE + path, timeout=8) as response:
        assert response.status == 200, (path, response.status)
        return json.loads(response.read().decode())

metrics = fetch("/api/metrics")
assert metrics["orders"] == 8, metrics
assert metrics["revenue"] == 539000, metrics
assert metrics["cost"] == 348000, metrics
assert metrics["openPipeline"] == 66000, metrics
assert abs(metrics["marginPercent"] - 35.44) < 0.01, metrics

series = fetch("/api/revenue/daily?days=30")
assert len(series) == 6, series
assert sum(x["revenue"] for x in series) == 539000, series

try:
    fetch("/api/revenue/daily?days=91")
    raise AssertionError("Invalid 91-day window incorrectly returned HTTP 200")
except urllib.error.HTTPError as error:
    assert error.code == 400, error.code

print("PASS: SQL Server fact table -> ASP.NET API -> KPI totals and daily series")

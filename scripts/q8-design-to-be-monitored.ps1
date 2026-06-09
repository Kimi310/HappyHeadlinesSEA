<#
.SYNOPSIS
  Q8 - Design to be Monitored demonstration (metrics vs logging vs tracing).

  Generates some traffic, then observes that SAME activity three different ways
  over HTTP:
    - METRICS  : aggregate numbers from /api/cache/stats
    - LOGGING  : discrete events queried from Loki
    - TRACING  : end-to-end request flow queried from Tempo

.EXAMPLE
  ./scripts/q8-design-to-be-monitored.ps1
#>
param(
    [string]$CacheBase = "http://localhost:8086",
    [string]$Publisher = "http://localhost:5203",
    [string]$Loki = "http://localhost:3100",
    [string]$Tempo = "http://localhost:3200",
    [string]$Region = "Europe",
    [int]$N = 15
)

$ErrorActionPreference = "Stop"

Write-Host "Q8 - Design to be Monitored (metrics / logs / traces)" -ForegroundColor Cyan

Write-Host "`n[0] Generating traffic ($N cache reads + 1 publish)..." -ForegroundColor Yellow
for ($i = 0; $i -lt $N; $i++) {
    try { Invoke-WebRequest "$CacheBase/api/cache/articles/$Region" -UseBasicParsing | Out-Null } catch {}
}
$body = @{ title = "Monitored demo"; content = ("generating telemetry " * 5); continent = $Region; isGlobal = $false } | ConvertTo-Json
try { Invoke-RestMethod "$Publisher/publishArticle" -Method Post -ContentType "application/json" -Body $body | Out-Null } catch {}
Start-Sleep -Seconds 4

# ---------- METRICS ----------
Write-Host "`n[1] METRICS - aggregate numbers over time (GET /api/cache/stats)" -ForegroundColor Green
try {
    $s = Invoke-RestMethod "$CacheBase/api/cache/stats"
    Write-Host ("    L1 hit={0} miss={1} ratio={2}  dbQueries={3}  avgLatencyMs={4}" -f `
            $s.layers.l1.hit, $s.layers.l1.miss, [math]::Round([double]$s.layers.l1.ratio, 2), $s.dbQueries, [math]::Round([double]$s.avgLatencyMs, 2))
}
catch { Write-Host "    metrics query failed: $($_.Exception.Message)" -ForegroundColor Red }
Write-Host "    -> good for dashboards / alerts / trends; no per-request detail" -ForegroundColor DarkGray

# ---------- LOGGING (Loki) ----------
Write-Host "`n[2] LOGGING - discrete events (query Loki)" -ForegroundColor Green
try {
    $end = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $start = $end - 900
    $q = [uri]::EscapeDataString('{app="publisherService"}')
    $logs = Invoke-RestMethod "$Loki/loki/api/v1/query_range?query=$q&start=${start}000000000&end=${end}000000000&limit=5&direction=backward"
    $lines = @($logs.data.result | ForEach-Object { $_.values } | ForEach-Object { $_[1] })
    Write-Host ("    {0} recent publisherService log line(s), showing up to 3:" -f $lines.Count)
    $lines | Select-Object -First 3 | ForEach-Object { Write-Host ("      {0}" -f $_) }
}
catch { Write-Host "    Loki query failed: $($_.Exception.Message)" -ForegroundColor Red }
Write-Host "    -> good for debugging specific events; hard to aggregate across services" -ForegroundColor DarkGray

# ---------- TRACING (Tempo) ----------
Write-Host "`n[3] TRACING - request flow across services (query Tempo)" -ForegroundColor Green
try {
    $tr = Invoke-RestMethod "$Tempo/api/search?limit=5"
    $traces = @($tr.traces)
    Write-Host ("    {0} recent trace(s), showing up to 3:" -f $traces.Count)
    $traces | Select-Object -First 3 | ForEach-Object {
        Write-Host ("      traceID={0}  root={1}  durationMs={2}" -f $_.traceID, $_.rootServiceName, $_.durationMs)
    }
}
catch { Write-Host "    Tempo query failed: $($_.Exception.Message)" -ForegroundColor Red }
Write-Host "    -> good for cross-service latency / dependencies; large data volume" -ForegroundColor DarkGray

Write-Host "`n================ Q8 RESULT ================" -ForegroundColor Cyan
Write-Host "The same traffic was observed three ways:"
Write-Host " - METRICS  : aggregate counters (what is happening, how much)"
Write-Host " - LOGGING  : individual events    (what happened, exactly)"
Write-Host " - TRACING  : end-to-end paths     (where time went, across services)"

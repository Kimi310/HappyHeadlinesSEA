<#
.SYNOPSIS
  Q6 - Recovery demonstration: "Design to be disabled".

  Shows a subsystem (the article cache) can be switched OFF at runtime with NO
  redeploy, while the service keeps serving (degraded to direct-DB reads) -
  i.e. disabled, not down. This is the feature-level recovery lever.

  Contrast (not shown here): "design for rollback" works at the DEPLOYMENT level
  - revert the whole release by redeploying a previous SHA-tagged image. That is
  infra-level, not a runtime HTTP action.

.EXAMPLE
  ./scripts/q6-recovery.ps1
#>
param(
    [string]$CacheBase = "http://localhost:8086",
    [string]$Region = "Europe",
    [int]$N = 10
)

$ErrorActionPreference = "Stop"
$articlesUrl = "$CacheBase/api/cache/articles/$Region"

function Set-Mode([bool]$enabled) {
    $v = $enabled.ToString().ToLower()
    Invoke-RestMethod "$CacheBase/api/cache/mode?enabled=$v" -Method Post | Out-Null
}
function Reset-Stats { Invoke-RestMethod "$CacheBase/api/cache/stats/reset" -Method Post | Out-Null }
function Warm-Up { Invoke-RestMethod "$CacheBase/api/cache/warmup" -Method Post | Out-Null }
function Get-Stats { Invoke-RestMethod "$CacheBase/api/cache/stats" }
function Invoke-Reads([int]$count) {
    $ok = 0
    for ($i = 0; $i -lt $count; $i++) {
        try { Invoke-WebRequest $articlesUrl -UseBasicParsing -TimeoutSec 5 | Out-Null; $ok++ } catch {}
    }
    return $ok
}

Write-Host "Q6 - Recovery: Design to be disabled" -ForegroundColor Cyan
try { Invoke-WebRequest $articlesUrl -UseBasicParsing | Out-Null } catch {
    Write-Host "Cannot reach $articlesUrl - is the stack running?" -ForegroundColor Red; throw
}

try {
    Write-Host "`n[1] Feature ENABLED (normal operation)" -ForegroundColor Green
    Set-Mode $true; Warm-Up; Reset-Stats
    $ok1 = Invoke-Reads $N
    $s1 = Get-Stats
    Write-Host ("    cacheEnabled={0}  reads ok={1}/{2}  dbQueries={3}  avgLatencyMs={4}" -f `
            $s1.cacheEnabled, $ok1, $N, $s1.dbQueries, [math]::Round([double]$s1.avgLatencyMs, 2))

    Write-Host "`n[2] DISABLING the cache subsystem at runtime (no redeploy)..." -ForegroundColor Yellow
    Set-Mode $false

    Write-Host "`n[3] Same reads with the feature DISABLED" -ForegroundColor Green
    Reset-Stats
    $ok2 = Invoke-Reads $N
    $s2 = Get-Stats
    Write-Host ("    cacheEnabled={0}  reads ok={1}/{2}  dbQueries={3}  avgLatencyMs={4}" -f `
            $s2.cacheEnabled, $ok2, $N, $s2.dbQueries, [math]::Round([double]$s2.avgLatencyMs, 2))
    Write-Host ("    -> service stayed AVAILABLE ({0}/{1} ok), degraded to direct-DB reads" -f $ok2, $N) -ForegroundColor Green

    Write-Host "`n[4] Re-enabling the cache..." -ForegroundColor Yellow
    Set-Mode $true
    Write-Host ("    cacheEnabled={0}" -f (Get-Stats).cacheEnabled)

    Write-Host "`n================ Q6 RESULT ================" -ForegroundColor Cyan
    Write-Host "Design to be DISABLED (feature level):"
    Write-Host " - the cache was switched off at runtime with NO redeploy"
    Write-Host (" - reads kept succeeding ({0}/{1}) - disabled, not down (graceful degradation)" -f $ok2, $N)
    Write-Host (" - cost of degrading: dbQueries {0} -> {1}, latency {2} -> {3} ms" -f `
            $s1.dbQueries, $s2.dbQueries, [math]::Round([double]$s1.avgLatencyMs, 2), [math]::Round([double]$s2.avgLatencyMs, 2))
    Write-Host "Design for ROLLBACK (deployment level): revert the whole release by"
    Write-Host "redeploying a previous SHA-tagged image - infra-level, not shown here." -ForegroundColor DarkGray
}
finally {
    # Always leave the cache enabled (default) so this run does not affect later runs.
    try { Set-Mode $true } catch {}
}

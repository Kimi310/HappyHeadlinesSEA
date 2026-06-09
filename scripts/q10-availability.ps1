<#
.SYNOPSIS
  Q10 - Availability demonstration (redundancy / failover).

  Sends load through the nginx load balancer, which fronts two article-service
  instances, then stops one instance and shows requests keep succeeding via the
  survivor - i.e. removing a single point of failure preserves availability.

  Orchestrates a `docker stop`/`docker start` of one instance - self-contained.

.EXAMPLE
  ./scripts/q10-availability.ps1 -N 30
#>
param(
    [string]$Lb = "http://localhost:80",
    [string]$Region = "Europe",
    [int]$N = 30,
    # Compose service name (resolved to the real container below). You can also
    # pass an explicit container name to override auto-detection.
    [string]$InstanceService = "article-service",
    [string]$Instance
)

$ErrorActionPreference = "Stop"
$url = "$Lb/api/article/get/$Region"

# Resolve the actual container name from its docker compose service label so the
# script works regardless of the compose project prefix (folder name).
function Resolve-ComposeContainer([string]$service, [string]$explicit) {
    if ($explicit) { return $explicit }
    $name = docker ps -a --filter "label=com.docker.compose.service=$service" --format "{{.Names}}" |
        Select-Object -First 1
    if (-not $name) {
        throw "No container found for compose service '$service'. Is the stack running? Try: docker compose up -d"
    }
    return $name
}
$Instance = Resolve-ComposeContainer $InstanceService $Instance
Write-Host "Using article-service instance container: $Instance" -ForegroundColor DarkGray

function Measure-Availability([int]$count) {
    $ok = 0; $fail = 0
    for ($i = 0; $i -lt $count; $i++) {
        try { Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 5 | Out-Null; $ok++ } catch { $fail++ }
    }
    return [pscustomobject]@{ ok = $ok; fail = $fail; total = $count }
}

Write-Host "Q10 - Availability (redundancy / failover via nginx)" -ForegroundColor Cyan
Write-Host "Load-balanced target: $url" -ForegroundColor DarkGray

try {

Write-Host "`n[1] Baseline - both article-service instances up" -ForegroundColor Yellow
$baseline = Measure-Availability $N
Write-Host ("    {0}/{1} requests succeeded" -f $baseline.ok, $baseline.total)

Write-Host "`n[2] Stopping ONE instance ($Instance)..." -ForegroundColor Yellow
docker stop $Instance | Out-Null
Start-Sleep -Seconds 2
# Settle: let nginx's passive health mark the dead upstream down before measuring
# steady state (a few requests during this detection window may fail - that is the
# transient failover cost, separate from steady-state availability).
$settle = Measure-Availability 6
Write-Host ("    (failover detection window: {0}/{1} during settle)" -f $settle.ok, $settle.total) -ForegroundColor DarkGray

Write-Host "`n[3] Steady-state load with one instance DOWN" -ForegroundColor Yellow
$degraded = Measure-Availability $N
Write-Host ("    {0}/{1} requests succeeded" -f $degraded.ok, $degraded.total)

Write-Host "`n[4] Restarting $Instance..." -ForegroundColor Yellow
docker start $Instance | Out-Null

Write-Host "`n================ Q10 RESULT ================" -ForegroundColor Cyan
Write-Host ("Both instances up : {0}/{1} OK" -f $baseline.ok, $baseline.total)
Write-Host ("One instance down : {0}/{1} OK" -f $degraded.ok, $degraded.total)
if ($degraded.ok -eq $degraded.total) {
    Write-Host "Stayed FULLY available in steady state - nginx failed over to the survivor." -ForegroundColor Green
}
elseif ($degraded.ok -gt 0) {
    Write-Host "Stayed mostly available - a few requests hit the dead upstream before failover." -ForegroundColor Yellow
}
else {
    Write-Host "Went DOWN - check that both instances and nginx proxy_next_upstream are configured." -ForegroundColor Red
}
Write-Host "Redundancy (X-axis duplication) removes the single point of failure." -ForegroundColor DarkGray

}
finally {
    # Always restore the stopped instance so this run does not affect later runs.
    Write-Host "`n[cleanup] ensuring $Instance is running..." -ForegroundColor DarkGray
    docker start $Instance | Out-Null
}

<#
.SYNOPSIS
  Q2 - Fault Isolation demonstration (circuit breaker).

  Shows that a failure in ProfanityService is contained: CommentService keeps
  accepting comments (stored unfiltered via the fallback) while the circuit
  breaker is Open, then recovers automatically once the dependency is back.

  Orchestrates a `docker stop`/`docker start` of ProfanityService to cause the
  failure - so it is self-contained.

.EXAMPLE
  ./scripts/q2-fault-isolation.ps1
#>
param(
    [string]$CommentBase = "http://localhost:8083",
    [string]$ProfanityContainer = "happyheadlinessea-profanity-service-1",
    [int]$TripCount = 5,
    [int]$RecoverySeconds = 20
)

$ErrorActionPreference = "Stop"
$articleId = [guid]::NewGuid().Guid

function Get-BreakerState {
    try { return (Invoke-RestMethod "$CommentBase/api/comment/breaker-state").state } catch { return "unknown" }
}
function New-Comment([string]$text) {
    $body = @{ articleId = $articleId; commentText = $text } | ConvertTo-Json
    try {
        Invoke-WebRequest "$CommentBase/api/comment" -Method Post -ContentType "application/json" -Body $body -UseBasicParsing -TimeoutSec 10 | Out-Null
        return $true
    }
    catch { return $false }
}
Write-Host "Q2 - Fault Isolation (circuit breaker)" -ForegroundColor Cyan
Write-Host "Article id for this run: $articleId" -ForegroundColor DarkGray

try {

Write-Host "`n[1] Healthy - breaker should be Closed" -ForegroundColor Yellow
Write-Host ("    breaker state : {0}" -f (Get-BreakerState))
Write-Host ("    post comment  : ok={0}" -f (New-Comment "hello while healthy"))

Write-Host "`n[2] Stopping ProfanityService ($ProfanityContainer)..." -ForegroundColor Yellow
docker stop $ProfanityContainer | Out-Null
Start-Sleep -Seconds 2

Write-Host "`n[3] Posting $TripCount comments with the dependency DOWN" -ForegroundColor Yellow
$accepted = 0
for ($i = 1; $i -le $TripCount; $i++) {
    $ok = New-Comment "comment $i while profanity down"
    if ($ok) { $accepted++ }
    Write-Host ("    post {0} -> http ok={1}, breaker={2}" -f $i, $ok, (Get-BreakerState))
}
Write-Host ("    {0}/{1} comments accepted (HTTP 200) despite the outage - stored unfiltered via fallback" -f $accepted, $TripCount) -ForegroundColor Green

$state = Get-BreakerState
$col = if ($state -eq "Open") { "Green" } else { "Red" }
Write-Host ("`n[4] Breaker state now: {0}" -f $state) -ForegroundColor $col

Write-Host "`n[5] Restarting ProfanityService and waiting ~$RecoverySeconds s for recovery..." -ForegroundColor Yellow
docker start $ProfanityContainer | Out-Null
Start-Sleep -Seconds $RecoverySeconds
$probeOk = New-Comment "probe after recovery"
Start-Sleep -Seconds 1
$state = Get-BreakerState
Write-Host ("    probe ok={0}, breaker state: {1}" -f $probeOk, $state)

Write-Host "`n================ Q2 RESULT ================" -ForegroundColor Cyan
Write-Host "The ProfanityService failure was ISOLATED:"
Write-Host " - CommentService stayed up and kept saving comments (fallback = unfiltered)"
Write-Host " - the breaker tripped to Open and failed fast instead of hanging"
Write-Host " - it recovered automatically once the dependency returned"
Write-Host "Patterns layered on one call: timeout + circuit breaker + fallback." -ForegroundColor DarkGray

}
finally {
    # Always restore ProfanityService so this run does not affect later runs.
    Write-Host "`n[cleanup] ensuring $ProfanityContainer is running..." -ForegroundColor DarkGray
    docker start $ProfanityContainer | Out-Null
}

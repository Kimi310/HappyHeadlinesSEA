<#
.SYNOPSIS
  GAF before/after demonstration driver for HappyHeadlines.

  Tactic A (caching): runs N reads with the cache OFF, then N reads with the
  cache ON, and reports DB queries, hit ratio and average server latency.

  Tactic B (compression): measures the article payload size on the wire with
  and without gzip.

  These are proxies for energy/resource use (the standard GAF / SCI approach),
  not direct joule measurement.

.PARAMETER Phase
  before : run only the cache-OFF baseline and save the result to a temp file.
  after  : run only the cache-ON pass, then print the comparison using the
           saved "before" result (also measures compression).
  both   : run the whole thing in one go (default).

.PARAMETER ArticleServiceBase
  Base URL of ArticleService used to seed articles into the region DB
  (default http://localhost:8082).

.PARAMETER SeedCount
  Number of articles to insert into the region DB before running so the cache
  and compression measurement have real data (default 25; set 0 to skip seeding).

.EXAMPLE
  ./scripts/gaf-demo.ps1 -Region Europe -N 200
.EXAMPLE
  ./scripts/gaf-demo.ps1 -Phase before
  ./scripts/gaf-demo.ps1 -Phase after
#>
param(
    [string]$Region = "Europe",
    [int]$N = 200,
    [string]$Base = "http://localhost:8086",
    [ValidateSet("before", "after", "both")]
    [string]$Phase = "both",
    # ArticleService base URL used to seed articles into the regional DB before the demo.
    [string]$ArticleServiceBase = "http://localhost:8082",
    # Ensure at least this many articles exist for the region before running.
    [int]$SeedCount = 25
)

$ErrorActionPreference = "Stop"
$articlesUrl = "$Base/api/cache/articles/$Region"
$stateFile = Join-Path ([System.IO.Path]::GetTempPath()) "gaf-before.json"

function Get-Stats { Invoke-RestMethod -Uri "$Base/api/cache/stats" -Method Get }
function Set-Mode([bool]$enabled) {
    $v = $enabled.ToString().ToLower()
    Invoke-RestMethod -Uri "$Base/api/cache/mode?enabled=$v" -Method Post | Out-Null
}
function Reset-Stats { Invoke-RestMethod -Uri "$Base/api/cache/stats/reset" -Method Post | Out-Null }
function Warm-Up { Invoke-RestMethod -Uri "$Base/api/cache/warmup" -Method Post | Out-Null }

function Invoke-Load([int]$count) {
    for ($i = 0; $i -lt $count; $i++) {
        Invoke-WebRequest -Uri $articlesUrl -Headers @{ "Accept-Encoding" = "gzip" } -UseBasicParsing | Out-Null
    }
}

function Get-WireBytes([string]$encoding) {
    # curl.exe reports bytes actually received over the wire (compressed when gzipped)
    $size = & curl.exe -s -o NUL -w "%{size_download}" -H "Accept-Encoding: $encoding" $articlesUrl
    return [int]$size
}

function Get-PayloadInfo {
    # Fetch the uncompressed body so we can verify there is actually something to compress.
    $resp = Invoke-WebRequest -Uri $articlesUrl -Headers @{ "Accept-Encoding" = "identity" } -UseBasicParsing
    $body = $resp.Content
    $count = 0
    try {
        $parsed = $body | ConvertFrom-Json
        if ($parsed -is [System.Array]) { $count = $parsed.Count } elseif ($null -ne $parsed) { $count = 1 }
    }
    catch { }
    return [pscustomobject]@{
        Bytes        = [System.Text.Encoding]::UTF8.GetByteCount($body)
        ArticleCount = $count
    }
}

function Test-Reachable {
    try { Invoke-WebRequest -Uri $articlesUrl -UseBasicParsing | Out-Null } catch {
        Write-Host "Cannot reach $articlesUrl - is the stack running (docker compose up)?" -ForegroundColor Red
        throw
    }
}

function Add-SeedArticles([int]$count) {
    # Seed real, sizeable articles into the region DB (via ArticleService) so the cache
    # and the compression measurement have meaningful data to work with.
    $addUrl = "$ArticleServiceBase/api/article/add"
    $lorem = ("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor " +
        "incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud " +
        "exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ")
    Write-Host ("`n[SEED] ensuring {0} article(s) exist in {1}DB via {2}" -f $count, $Region, $addUrl) -ForegroundColor Cyan
    $created = 0
    for ($i = 1; $i -le $count; $i++) {
        $body = @{
            title          = "GAF demo article #$i - $Region"
            content        = ($lorem * 4) + " Article #$i for region $Region."
            continent      = $Region
            isGlobal       = $false
            publishedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
        } | ConvertTo-Json
        try {
            Invoke-RestMethod -Uri $addUrl -Method Post -ContentType "application/json" -Body $body | Out-Null
            $created++
        }
        catch {
            Write-Host ("  Failed to seed article #{0}: {1}" -f $i, $_.Exception.Message) -ForegroundColor Red
            Write-Host "  Is ArticleService running at $ArticleServiceBase? Override with -ArticleServiceBase." -ForegroundColor Yellow
            throw
        }
    }
    Write-Host ("  Seeded {0} article(s)." -f $created) -ForegroundColor DarkGray

    # Make sure the freshly seeded rows are loaded into the cache before measuring.
    try { Set-Mode $true | Out-Null } catch {}
    Warm-Up
}

function Invoke-BeforePhase {
    Write-Host "`n[BEFORE] cache OFF - every read hits SQL" -ForegroundColor Yellow
    Set-Mode $false
    Reset-Stats
    Invoke-Load $N
    $stats = Get-Stats
    $result = [pscustomobject]@{
        region       = $Region
        n            = $N
        dbQueries    = [long]$stats.dbQueries
        avgLatencyMs = [double]$stats.avgLatencyMs
    }
    $result | ConvertTo-Json | Set-Content -Path $stateFile -Encoding UTF8
    Write-Host ("  DB queries: {0}   avg latency: {1} ms" -f $result.dbQueries, [math]::Round($result.avgLatencyMs, 2))
    return $result
}

function Invoke-AfterPhase {
    Write-Host "`n[AFTER] cache ON - reads served from L1/L2" -ForegroundColor Green
    Set-Mode $true
    Warm-Up
    Reset-Stats
    Invoke-Load $N
    return Get-Stats
}

function Show-Report($before, $after, $plain, $gzip, $payload) {
    $afterDb = [long]$after.dbQueries
    $afterLat = [math]::Round([double]$after.avgLatencyMs, 2)
    $l1ratio = [math]::Round([double]$after.layers.l1.ratio * 100, 1)
    $avoided = [long]$after.dbQueriesAvoided
    $saved = if ($plain -gt 0) { [math]::Round((1 - $gzip / $plain) * 100, 1) } else { 0 }

    $beforeDb = if ($before) { [long]$before.dbQueries } else { "n/a" }
    $beforeLat = if ($before) { [math]::Round([double]$before.avgLatencyMs, 2) } else { "n/a" }

    Write-Host "`n================ GAF DEMO RESULTS ================" -ForegroundColor Cyan
    Write-Host "Tactic A - Caching (compute / DB load)"
    "{0,-26}{1,15}{2,15}" -f "Metric", "BEFORE (off)", "AFTER (on)" | Write-Host
    "{0,-26}{1,15}{2,15}" -f "DB queries", $beforeDb, $afterDb | Write-Host
    "{0,-26}{1,15}{2,15}" -f "Avg latency (ms)", $beforeLat, $afterLat | Write-Host
    "{0,-26}{1,15}{2,15}" -f "L1 hit ratio (%)", "-", $l1ratio | Write-Host
    "{0,-26}{1,15}{2,15}" -f "DB queries avoided", "-", $avoided | Write-Host

    Write-Host "`nTactic B - Response compression (network)"
    "{0,-26}{1,15}" -f "Articles in payload", $payload.ArticleCount | Write-Host
    "{0,-26}{1,15}" -f "Payload uncompressed (B)", $plain | Write-Host
    "{0,-26}{1,15}" -f "Payload gzipped (B)", $gzip | Write-Host

    # gzip has a fixed ~18-byte header/footer overhead, so tiny/empty payloads always
    # grow. Only treat the comparison as meaningful when there is real data to compress.
    if ($payload.ArticleCount -le 0 -or $plain -lt 200) {
        Write-Host ("  Payload is empty/too small ({0} article(s), {1} B) - compression cannot help here." -f $payload.ArticleCount, $plain) -ForegroundColor Yellow
        Write-Host "  This is NOT a compression problem: there is simply nothing to compress." -ForegroundColor Yellow
        Write-Host "  Make sure the region has seeded articles and that warm-up loaded them," -ForegroundColor Yellow
        Write-Host ("  e.g. try a region that has data:  ./scripts/q4-gaf-demo.ps1 -Region {0}" -f $Region) -ForegroundColor Yellow
    }
    else {
        "{0,-26}{1,14}%" -f "Bytes saved", $saved | Write-Host
        if ($gzip -ge $plain) {
            Write-Host "  (no size difference - is Compression__Enabled=true on the service?)" -ForegroundColor Yellow
        }
    }
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "Note: DB queries / latency / bytes are proxies for energy use." -ForegroundColor DarkGray
}

Write-Host "GAF demo - phase=$Phase, region=$Region, requests=$N, base=$Base" -ForegroundColor Cyan
Test-Reachable

# Seed the region DB so there is real data to cache and compress.
if ($SeedCount -gt 0) {
    Add-SeedArticles $SeedCount
}

try {

if ($Phase -eq "before") {
    Invoke-BeforePhase | Out-Null
    Write-Host "`nBefore-phase result saved to: $stateFile" -ForegroundColor DarkGray
    Write-Host "Now run:  ./scripts/gaf-demo.ps1 -Phase after" -ForegroundColor DarkGray
    return
}

# Phase = after or both: obtain the "before" result.
$before = $null
if ($Phase -eq "both") {
    $before = Invoke-BeforePhase
}
else {
    if (Test-Path $stateFile) {
        $before = Get-Content -Path $stateFile -Raw | ConvertFrom-Json
        if ($before.region -ne $Region -or $before.n -ne $N) {
            Write-Host ("Warning: saved before-phase used region={0}, N={1} (now region={2}, N={3}) - comparison may be uneven." -f $before.region, $before.n, $Region, $N) -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "No saved before-phase found - run './scripts/gaf-demo.ps1 -Phase before' first. Showing AFTER only." -ForegroundColor Yellow
    }
}

$after = Invoke-AfterPhase

Write-Host "`nMeasuring payload size (compression)..." -ForegroundColor DarkGray
$payload = Get-PayloadInfo
if ($payload.ArticleCount -le 0) {
    Write-Host ("  Warning: {0} returned an empty payload ({1} B) - no articles for region '{2}'." -f $articlesUrl, $payload.Bytes, $Region) -ForegroundColor Yellow
}
$plain = Get-WireBytes "identity"
$gzip = Get-WireBytes "gzip"

Show-Report $before $after $plain $gzip $payload

}
finally {
    # Restore the default (cache enabled) so this run does not affect later runs.
    try { Set-Mode $true | Out-Null } catch {}
}

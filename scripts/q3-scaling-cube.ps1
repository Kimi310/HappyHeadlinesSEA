<#
.SYNOPSIS
  Q3 - Scaling Cube demonstration: Z-axis (data partitioning).

  Publishes articles to several continents, then shows that:
    - reading one continent returns only that continent's articles, and
    - each continent's data physically lives in its own database
  i.e. the data is partitioned along the Z-axis (split by key = continent).

.EXAMPLE
  ./scripts/q3-scaling-cube.ps1
#>
param(
    [string]$Publisher = "http://localhost:5203",
    [string]$CacheBase = "http://localhost:8086",
    [string[]]$Regions = @("Europe", "Asia", "Africa"),
    [int]$PerRegion = 2,
    [string]$SqlContainer = "article-sqlserver"
)

$ErrorActionPreference = "Stop"

Write-Host "Q3 - Scaling Cube: Z-axis data partitioning" -ForegroundColor Cyan

Write-Host "`n[0] Publishing $PerRegion article(s) to each of: $($Regions -join ', ')" -ForegroundColor Yellow
foreach ($r in $Regions) {
    for ($i = 1; $i -le $PerRegion; $i++) {
        $b = @{ title = "$r headline $i"; content = "$r regional content $i"; continent = $r; isGlobal = $false } | ConvertTo-Json
        try { Invoke-RestMethod "$Publisher/publishArticle" -Method Post -ContentType "application/json" -Body $b | Out-Null }
        catch { Write-Host "    publish to $r failed: $($_.Exception.Message)" -ForegroundColor Red }
    }
    Write-Host "    published to $r"
}
Start-Sleep -Seconds 6
# refresh the cache so reads reflect the freshly published data
try { Invoke-RestMethod "$CacheBase/api/cache/warmup" -Method Post | Out-Null } catch {}

Write-Host "`n[1] Reading each continent via the cache (partitioned by continent)" -ForegroundColor Green
foreach ($r in $Regions) {
    $items = Invoke-RestMethod "$CacheBase/api/cache/articles/$r"
    $count = ($items | Measure-Object).Count
    $titles = (($items | Select-Object -First 3).title -join "; ")
    Write-Host ("    {0,-10} {1,3} article(s)  e.g. {2}" -f $r, $count, $titles)
}

Write-Host "`n[2] Proof of physical partitioning - row count in each continent DB" -ForegroundColor Green
foreach ($r in $Regions) {
    $db = "$($r)DB"
    $cnt = docker exec $SqlContainer /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -No -h -1 -d $db -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.Articles;" 2>&1
    Write-Host ("    {0,-16} rows = {1}" -f $db, (($cnt -join '').Trim()))
}

Write-Host "`n================ Q3 RESULT ================" -ForegroundColor Cyan
Write-Host "Z-axis scaling = split DATA by key (here, continent):"
Write-Host " - each continent's articles live in their OWN database (EuropeDB, AsiaDB, ...)"
Write-Host " - a read for one continent only touches that shard: smaller, faster, isolated"
Write-Host " - blast radius is contained: one continent DB failing doesn't take the others down"
Write-Host "Combine with X-axis (cloned article-service behind nginx) to scale app + data together." -ForegroundColor DarkGray

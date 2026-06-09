# Green Architecture Framework — Demonstration Runbook

**Question 4:** *Demonstrate selected tactics from GAF with before-and-after examples.*

This demo shows two GAF tactics already living in HappyHeadlines and measures their
impact with **before/after** numbers. Because energy cannot be measured directly in
this setup, we use the standard GAF / Software Carbon Intensity (SCI) approach:
**proxies for energy** — database queries executed, request latency, and bytes
transferred over the network.

| Tactic | GAF principle | Proxy measured |
|--------|---------------|----------------|
| A — Caching (L1 memory + L2 Redis) | Efficiency | DB queries, latency |
| B — Response compression (gzip) | Efficiency (network) | bytes on the wire |
| Metrics endpoint + Grafana | Observability | makes waste visible |

## Prerequisites

```bash
docker compose up --build
```

Wait until `article-cache-service` (host port **8086**) and `redis` are healthy.

## What was added (all additive, flag-gated)

- `ArticleCache__Enabled` (default `true`) — when `false`, the cache is bypassed and
  every read goes to SQL. Used to produce the "before" baseline.
- `POST /api/cache/mode?enabled=false|true` — flip the cache at runtime.
- `POST /api/cache/stats/reset` — zero the counters for a clean run.
- New metrics on `GET /api/cache/stats`: `dbQueries`, `dbQueriesAvoided`,
  `avgLatencyMs`, `cacheEnabled` (in addition to the existing hit/miss stats).
- `Compression__Enabled` (default `true`) — gzip/brotli response compression on the
  article read paths.

## Run the demonstration

```powershell
./scripts/gaf-demo.ps1 -Region Europe -N 200
```

The script:
1. **Before** — disables the cache, resets stats, fires N reads, captures stats.
2. **After** — enables the cache, warms it, resets stats, fires N reads, captures stats.
3. **Compression** — requests the same payload with `Accept-Encoding: identity` vs `gzip`.

### Manual equivalent (curl)

```bash
# BEFORE
curl -X POST "http://localhost:8086/api/cache/mode?enabled=false"
curl -X POST  http://localhost:8086/api/cache/stats/reset
#  ...fire reads against /api/cache/articles/Europe...
curl http://localhost:8086/api/cache/stats

# AFTER
curl -X POST "http://localhost:8086/api/cache/mode?enabled=true"
curl -X POST  http://localhost:8086/api/cache/warmup
curl -X POST  http://localhost:8086/api/cache/stats/reset
#  ...fire reads again...
curl http://localhost:8086/api/cache/stats

# COMPRESSION (bytes on the wire)
curl -s -o NUL -w "%{size_download}\n" -H "Accept-Encoding: identity" http://localhost:8086/api/cache/articles/Europe
curl -s -o NUL -w "%{size_download}\n" -H "Accept-Encoding: gzip"     http://localhost:8086/api/cache/articles/Europe
```

## Results (fill in after running)

**Tactic A — Caching** (N = ___ reads, region = ___)

| Metric | Before (cache off) | After (cache on) |
|--------|-------------------|------------------|
| DB queries | | |
| Avg latency (ms) | | |
| L1 hit ratio (%) | n/a | |
| DB queries avoided | n/a | |

**Tactic B — Compression**

| Payload | Bytes |
|---------|-------|
| Uncompressed | |
| Gzipped | |
| Saved (%) | |

## Visualise

Grafana → **Article Cache Overview** (`http://localhost:3000`) now includes
*DB Queries*, *DB Queries Avoided*, *Avg Read Latency* and *Cache Enabled* panels,
which update live while the script runs.

## How this maps back to GAF

- **Efficiency:** caching removes repeated SQL work; compression removes repeated bytes.
- **Observability:** the stats endpoint + dashboard make resource use measurable.
- **Elasticity (discussion):** the same efficiency creates headroom, so fewer/smaller
  replicas are needed for the same load.

## Honest caveats

- DB queries, latency and bytes are **proxies** for energy, not joule measurements.
- All changes are flag-gated; with defaults the system behaves exactly as before.

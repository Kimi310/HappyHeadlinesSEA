# Q4 — Green Architecture Framework (GAF) · .http demonstration

A click-through, before/after demonstration of two GAF efficiency tactics in
HappyHeadlines, driven entirely by HTTP requests:

- **Tactic A — Caching:** run reads with the cache OFF (every read hits SQL),
  then with the cache ON (served from L1/L2), and compare the DB-query count,
  hit ratio and average latency.
- **Tactic B — Response compression:** fetch the same payload without and with
  gzip and compare the size on the wire.

DB queries, latency and bytes are **proxies** for energy/resource use — the
standard GAF / Software Carbon Intensity (SCI) approach.

## How to run

1. Start the stack: `docker compose up --build` (run from the repo root).
2. Open these files in an editor that runs `.http` files:
   - VS Code with the **REST Client** extension (`humao.rest-client`)
   - JetBrains Rider / IntelliJ (built-in HTTP client)
   - Visual Studio 2022 (`.http` support)
3. Run the files **in numbered order**, clicking *Send Request* on each block.

Services used:
- `article-cache-service` → http://localhost:8086
- `publisher-service` → http://localhost:5203 (only for the optional seed step)

## Step order

| File | Service | What it does |
|------|---------|--------------|
| `00-publisher-service-seed-articles.http` | publisher | (optional) publish a few Europe articles so reads return real data |
| `01-cache-service-disable-cache.http` | cache | turn the cache OFF (start the "before") |
| `02-cache-service-reset-stats.http` | cache | zero the counters |
| `03-cache-service-read-articles-before.http` | cache | fire several reads (each hits SQL) |
| `04-cache-service-get-stats-before.http` | cache | capture BEFORE stats |
| `05-cache-service-enable-cache.http` | cache | turn the cache ON (start the "after") |
| `06-cache-service-warmup.http` | cache | preload the cache |
| `07-cache-service-reset-stats.http` | cache | zero the counters again |
| `08-cache-service-read-articles-after.http` | cache | fire the same reads (served from cache) |
| `09-cache-service-get-stats-after.http` | cache | capture AFTER stats |
| `10-cache-service-read-uncompressed.http` | cache | payload size without gzip |
| `11-cache-service-read-gzip.http` | cache | payload size with gzip |

## What to look for

- **Step 04 (before):** `dbQueries` ≈ number of reads, `dbQueriesAvoided` = 0,
  `cacheEnabled` = false.
- **Step 09 (after):** `dbQueries` ≈ 0, `dbQueriesAvoided` ≈ number of reads,
  `layers.l1.ratio` near 1.0, lower `avgLatencyMs`, `cacheEnabled` = true.
- **Steps 10 vs 11:** step 11's response has a `Content-Encoding: gzip` header
  and a smaller transfer size than step 10.

For an automated run with a printed results table, use `scripts/gaf-demo.ps1`;
full notes are in `docs/GAF-DEMO.md`.

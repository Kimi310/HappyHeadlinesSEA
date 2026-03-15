# ArticleCacheService

Separate microservice responsible for article caching.

## Responsibilities

- L1 cache: `IMemoryCache`
- L2 cache: Redis
- Warm preload from SQL for last 14 days
- Cache hit/miss counters in Redis exposed as JSON (`/api/cache/stats`)
- Setup in Grafana by adding new Stat view and adding row: `[{"ratio": $number($.layers.l1.ratio)}]` and `[{"ratio": $number($.layers.l2.ratio)}]` plus column with selector: `ratio` format as: `Number`

## API

- `GET /api/cache/articles/{region}`
- `DELETE /api/cache/region/{region}`
- `POST /api/cache/warmup`
- `GET /api/cache/stats`
- `GET /health`


# How to run

```
docker compose build

docker compose up

```

or


```
docker build -t happyheaders-api:latest

docker compose up --build

```

- If **FIRST** build include --build in docker compose like this:
```
docker compose up --build
```

## RabbitMQ article flow

- RabbitMQ broker is exposed on `5672` and management UI on `15672`.
- `ArticleQueue` initializes exchange/queue topology for article events.
- `PublisherService` publishes `/publishArticle` messages to RabbitMQ.
- `ArticleService` consumes from queue and stores articles in SQL.

## Article cache and monitoring

- `ArticleCacheService` is a separate microservice and Docker container.
- `ArticleService` calls `ArticleCacheService` over HTTP (no project references between services).
- `ArticleCacheService` uses two cache layers: L1 (`IMemoryCache`) and L2 (`Redis`).
- Cache is preloaded periodically with articles from the last 14 days.
- Cache hit/miss counters are stored in Redis and exposed by `ArticleCacheService` on `GET /api/cache/stats`.
- Grafana includes an auto-provisioned dashboard: `Article Cache Overview` (datasource: Infinity JSON API).

Default local URLs:

- Grafana: `http://localhost:3000`
- ArticleCacheService stats: `http://localhost:8086/api/cache/stats`


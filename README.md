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

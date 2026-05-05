# Payhas Binyat API

HTTP API над PostgreSQL слоем (`Payhas.Binyat.Data.Postgres`).
Тонкая обёртка для подключения существующего WPF клиента (через адаптер
на .NET Framework 4.6.1) и любого другого внешнего интегратора.

## Запуск локально

1. PostgreSQL запущен на `127.0.0.1:5432`, база `payhas`,
   пользователь `payhas_app`, пароль в `appsettings.json`.
2. Из корня репозитория:
   ```bash
   dotnet run --project Payhas.Binyat.Api
   ```
3. Swagger: <http://localhost:5050/swagger>

## Эндпоинты

| Группа | URL | Назначение |
|--------|-----|-----------|
| Health    | `GET /api/health`               | Жив ли API |
| Health    | `GET /api/health/db`            | Достижим ли PostgreSQL |
| Stocks    | `GET /api/stocks/search?q=…`    | Fuzzy-поиск (pg_trgm + tsvector) |
| Stocks    | `GET /api/stocks`               | Список с проекцией для грида |
| Stocks    | `GET /api/stocks/{id}`          | Полная карточка товара |
| Stocks    | `GET /api/stocks/facets?fields=type,group` | Фасеты для фильтра UI |
| Stocks    | `POST/PUT/DELETE /api/stocks`   | CRUD |
| Invoices  | `GET /api/invoices?from=…&till=…&displayCurrencyId=…` | Накладные с верными суммами |
| Invoices  | `GET /api/invoices/payment-info?…` | Партнёрский ledger |
| Invoices  | `GET /api/invoices/revenue?…`   | Отчёт прибыли с возвратами (фикс «100%») |
| Invoices  | `GET /api/invoices/{id}`        | Полная накладная |
| Invoices  | `POST/PUT/DELETE /api/invoices` | CRUD |
| Balances  | `GET /api/balances/by-date?date=…&priceGroup=…&displayCurrencyId=…` | Балансы на дату |
| Balances  | `GET /api/balances/stock/{stockId}` | Балансы по конкретному товару |
| Balances  | `GET /api/balances/warehouse/{warehouseId}` | Балансы по складу |
| Balances  | `GET /api/balances/by-type?…`   | Время-серия по типам накладных |

## Деплой

См. [`Deploy/DEPLOY.md`](Deploy/DEPLOY.md).

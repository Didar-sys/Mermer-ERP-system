# Payhas Binyat API — деплой на VDS

Сервер: **AwtoCom** (Ubuntu 24.04, IP `216.250.14.108`).
PostgreSQL уже установлен и накачена схема (`payhas` / `payhas_app`).
Сайт `awtofon.com.tm` работает на той же машине через nginx.

## 1. Установить .NET 8 Runtime (один раз)

В веб-консоли сервера:

```bash
sudo apt install -y aspnetcore-runtime-8.0
```

Если пакет не найден:

```bash
sudo apt install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0
```

Если репозиторий `dotnet` ещё не подключён, перед этим:

```bash
sudo apt install -y wget
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/ms.deb
sudo dpkg -i /tmp/ms.deb
sudo apt install -y aspnetcore-runtime-8.0
```

Проверка:

```bash
dotnet --info
```

## 2. Опубликовать API локально (на ПК разработчика)

В корне репозитория:

```powershell
dotnet publish Payhas.Binyat.Api/Payhas.Binyat.Api.csproj `
    -c Release `
    -r linux-x64 `
    --self-contained false `
    -o ./publish/payhas-api
```

Получится папка `publish/payhas-api/` с `Payhas.Binyat.Api`,
`Payhas.Binyat.Data.Postgres.dll`, `appsettings.json` и зависимостями.

Залить её на сервер (через FileZilla / scp) в `/home/ubuntu/payhas-api/`.

## 3. Развернуть на сервере

```bash
sudo mkdir -p /opt/payhas-api
sudo cp -r /home/ubuntu/payhas-api/* /opt/payhas-api/
sudo chown -R ubuntu:ubuntu /opt/payhas-api
sudo chmod +x /opt/payhas-api/Payhas.Binyat.Api
```

## 4. Настроить строку подключения

Файл `/opt/payhas-api/appsettings.Production.json` уже содержит
строку подключения к локальному PostgreSQL — поменяйте пароль на
актуальный:

```bash
sudo nano /opt/payhas-api/appsettings.Production.json
```

## 5. Установить systemd-сервис

```bash
sudo cp /opt/payhas-api/Deploy/payhas-api.service /etc/systemd/system/payhas-api.service
sudo systemctl daemon-reload
sudo systemctl enable --now payhas-api
sudo systemctl status payhas-api --no-pager
```

API теперь слушает `http://127.0.0.1:5050`.

Лог-стрим:

```bash
sudo journalctl -u payhas-api -f
```

## 6. Проверить из консоли сервера

```bash
curl http://127.0.0.1:5050/api/health
curl http://127.0.0.1:5050/api/health/db
```

Должно вернуть JSON со статусом `ok`.

## 7. Подключить через nginx (опционально)

Если хотите выставить API наружу как `api.awtofon.com.tm`:

```bash
sudo cp /opt/payhas-api/Deploy/nginx-payhas-api.conf /etc/nginx/sites-available/payhas-api
sudo ln -sf /etc/nginx/sites-available/payhas-api /etc/nginx/sites-enabled/payhas-api
sudo nginx -t
sudo systemctl reload nginx
```

DNS-запись `api.awtofon.com.tm` должна указывать на IP сервера.

Для HTTPS поверх — `sudo certbot --nginx -d api.awtofon.com.tm`
(если порт 80 открыт наружу).

## 8. Проверка из браузера

- Локально на сервере: `http://127.0.0.1:5050/swagger`
- Снаружи (если nginx настроен): `http://api.awtofon.com.tm/swagger`

Swagger UI даёт интерактивную проверку всех эндпоинтов.

## Обновление API

```bash
sudo systemctl stop payhas-api
sudo cp -r /home/ubuntu/payhas-api/* /opt/payhas-api/
sudo systemctl start payhas-api
```

## Откат

```bash
sudo systemctl stop payhas-api
# восстановить предыдущую версию из бэкапа
sudo systemctl start payhas-api
```

# Mermer ERP API — деплой на VDS

Сервер: Ubuntu 24.04.  
PostgreSQL уже установлен, база `mermer` / пользователь `mermer_app`.

## 1. Установить .NET 8 Runtime (один раз)

```bash
sudo apt install -y aspnetcore-runtime-8.0
```

Если пакет не найден:

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/ms.deb
sudo dpkg -i /tmp/ms.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
```

Проверка:

```bash
dotnet --info
```

## 2. Собрать API на ПК разработчика

```powershell
dotnet publish Mermer.Api/Mermer.Api.csproj `
    -c Release `
    -r linux-x64 `
    --self-contained false `
    -o ./publish/mermer-api
```

Папка `publish/mermer-api/` — всё что нужно залить на сервер.

## 3. Залить файлы на сервер (FileZilla)

Источник: `publish/mermer-api/`  
Назначение: `/home/ubuntu/mermer-api/`

## 4. Переместить в /opt и выдать права

```bash
sudo mkdir -p /opt/mermer-api
sudo cp -r /home/ubuntu/mermer-api/* /opt/mermer-api/
sudo chown -R ubuntu:ubuntu /opt/mermer-api
sudo chmod +x /opt/mermer-api/Mermer.Api
```

## 5. Создать файл конфигурации подключения

Вместо правки `appsettings.json` используем отдельный `.env` —
при переезде на другой сервер меняется только этот файл:

```bash
sudo nano /opt/mermer-api/.env
```

Содержимое (подставьте свои значения):

```
ConnectionStrings__Postgres=Host=127.0.0.1;Port=5432;Database=mermer;Username=mermer_app;Password=ВАШ_ПАРОЛЬ
```

Права только для владельца:

```bash
sudo chmod 600 /opt/mermer-api/.env
```

## 6. Установить systemd-сервис

```bash
sudo cp /opt/mermer-api/Deploy/mermer-api.service /etc/systemd/system/mermer-api.service
sudo systemctl daemon-reload
sudo systemctl enable --now mermer-api
sudo systemctl status mermer-api --no-pager
```

Лог-стрим:

```bash
sudo journalctl -u mermer-api -f
```

## 7. Проверка

```bash
curl http://127.0.0.1:5050/api/health
curl http://127.0.0.1:5050/api/health/db
```

Должно вернуть `{"status":"ok",...}`.

## 8. Подключить через nginx (опционально)

```bash
sudo cp /opt/mermer-api/Deploy/nginx-mermer-api.conf /etc/nginx/sites-available/mermer-api
sudo ln -sf /etc/nginx/sites-available/mermer-api /etc/nginx/sites-enabled/mermer-api
sudo nginx -t && sudo systemctl reload nginx
```

## Обновление API

```bash
sudo systemctl stop mermer-api
sudo cp -r /home/ubuntu/mermer-api/* /opt/mermer-api/
sudo systemctl start mermer-api
```

## Переезд на другой сервер

1. Повторить шаги 1–4 на новом сервере.
2. Создать `/opt/mermer-api/.env` с новыми данными подключения.
3. Запустить сервис. Старый сервер остаётся нетронутым.

## Swagger UI

- Локально на сервере: `http://127.0.0.1:5050/swagger`
- Через nginx: `http://api.awtofon.com.tm/swagger`

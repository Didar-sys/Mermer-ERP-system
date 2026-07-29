# 1. Сборка приложения (.NET 8 / 9 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов для восстановления зависимостей
COPY ["Mermer.Api/Mermer.Api.csproj", "Mermer.Api/"]
COPY ["Mermer.Data.Postgres/Mermer.Data.Postgres.csproj", "Mermer.Data.Postgres/"]

RUN dotnet restore "Mermer.Api/Mermer.Api.csproj"

# Копируем весь исходный код и собираем
COPY . .
WORKDIR "/src/Mermer.Api"
RUN dotnet build "Mermer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mermer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Финальный образ для запуска (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mermer.Api.dll"]
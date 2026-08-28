# 1. Сборка приложения (.NET 8 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем проекты для restore
COPY ["Mermer.Api/Mermer.Api.csproj", "Mermer.Api/"]
COPY ["Mermer.Data.Postgres/Mermer.Data.Postgres.csproj", "Mermer.Data.Postgres/"]

RUN dotnet restore "Mermer.Api/Mermer.Api.csproj"

# Копируем остальные исходники
COPY . .
WORKDIR "/src/Mermer.Api"
RUN dotnet publish "Mermer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Явно задаем порт 8080 для .NET 8 Kestrel
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mermer.Api.dll"]
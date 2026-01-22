# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj(s) first for better layer caching
COPY ["books_app.api.csproj", "./"]

RUN dotnet restore "./books_app.api.csproj"

# Copy the rest
COPY . .

# Build
RUN dotnet build "./books_app.api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "./books_app.api.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Test stage (runs dotnet test)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS test
WORKDIR /src
COPY . .

# Run all tests in repo:
RUN dotnet test -c Release --logger "trx;LogFileName=test_results.trx"

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=10 \
  CMD curl -fsS http://localhost:8080/swagger/v1/swagger.json >/dev/null || exit 1

ENTRYPOINT ["dotnet", "books_app.api.dll"]
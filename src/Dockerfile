# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "API/API.csproj"

COPY . .
WORKDIR /src/API
RUN dotnet publish "API.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Stage 2: Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Instala a dependência do Linux exigida pelo Npgsql
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# Roda com usuário sem privilégios
USER $APP_UID

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
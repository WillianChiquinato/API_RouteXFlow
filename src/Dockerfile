# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia primeiro os arquivos de projeto para aproveitar o cache do Docker (substitua "Api" pelo nome exato da sua pasta se for diferente)
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "API/API.csproj"

# Copia o restante do código fonte
COPY . .
WORKDIR "/src/API"

# Publica a aplicação
RUN dotnet publish "API.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Stage 2: Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Roda com usuário sem privilégios administrativos (Segurança)
USER $APP_UID

# Porta padrão para o .NET moderno
EXPOSE 8080

# Copia os arquivos publicados do Stage 1
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
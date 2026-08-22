FROM node:24-alpine AS frontend-build
WORKDIR /frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY ["src/StockFlow.Api/StockFlow.Api.csproj", "src/StockFlow.Api/"]
RUN dotnet restore "src/StockFlow.Api/StockFlow.Api.csproj"

COPY . .
WORKDIR "/src/src/StockFlow.Api"
RUN dotnet publish "StockFlow.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=api-build /app/publish .
COPY --from=frontend-build /frontend/dist/stockflow-web/browser ./wwwroot

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "StockFlow.Api.dll"]

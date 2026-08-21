FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StockFlow.Api.dll"]

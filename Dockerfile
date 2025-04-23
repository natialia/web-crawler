# Build Stage mit SDK (für EF Core Migration)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./

# Tools installieren für EF Core CLI
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet restore
RUN dotnet build -c Release -o /out
RUN dotnet publish -c Release -o /out

# Laufzeit-Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "WebCrawler.dll"]

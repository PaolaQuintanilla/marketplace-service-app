# syntax=docker/dockerfile:1

# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Honor the pinned SDK / rollForward from global.json
COPY global.json ./
COPY ServiceApp.sln ./

# Copy csproj files first so `restore` is a cached layer (only re-runs when a csproj changes)
COPY ServiceApp.Domain/ServiceApp.Domain.csproj                 ServiceApp.Domain/
COPY ServiceApp.Application/ServiceApp.Application.csproj        ServiceApp.Application/
COPY ServiceApp.Infrastructure/ServiceApp.Infrastructure.csproj ServiceApp.Infrastructure/
COPY ServiceApp.API/ServiceApp.API.csproj                       ServiceApp.API/
RUN dotnet restore ServiceApp.API/ServiceApp.API.csproj

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish ServiceApp.API/ServiceApp.API.csproj -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# The aspnet image ships a non-root 'app' user; run as it for safety
USER app

# Listen on 8080 (the .NET default; no root needed). App Service forwards here via WEBSITES_PORT.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ServiceApp.API.dll"]

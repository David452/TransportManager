# syntax=docker/dockerfile:1

# 1) Build Tailwind CSS
FROM node:20-alpine AS css
WORKDIR /src/Web
COPY Web/package.json Web/package-lock.json* ./
RUN npm install
COPY Web/ ./
RUN npx tailwindcss -i wwwroot/css/input.css -o wwwroot/css/output.css --minify

# 2) Build & publish .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TransportManager.sln ./
COPY Core/Core.csproj Core/
COPY Web/Web.csproj Web/
COPY CLI/CLI.csproj CLI/
RUN dotnet restore Web/Web.csproj

COPY Core/ Core/
COPY Web/ Web/
# Overwrite with the Tailwind-built CSS
COPY --from=css /src/Web/wwwroot/css/output.css Web/wwwroot/css/output.css

RUN dotnet publish Web/Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# 3) Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# JSON data store lives here — mount a volume to persist it
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Web.dll"]

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /source
COPY SolarChargerFE/. ./src/

# restore packages.
RUN dotnet restore "./src/SolarChargerFE.csproj"

# build and publish (UseAppHost=false creates platform independent binaries).
WORKDIR /source/src
RUN dotnet build "SolarChargerFE.csproj" -c Release -o /app/build/SolarChargerFE
RUN dotnet publish "SolarChargerFE.csproj" -c Release -o /app/publish/SolarChargerFE /p:UseAppHost=false --no-restore -f net8.0

# move binaries into smaller base image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080/tcp
WORKDIR /app/SolarChargerFE
ENV ApiAddress "http://server:8080"
ENTRYPOINT ["dotnet", "SolarChargerFE.dll"]
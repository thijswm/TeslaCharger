FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /source
COPY SolarCharger/. ./src/

# restore packages.
RUN dotnet restore "./src/SolarCharger.csproj"

# build and publish (UseAppHost=false creates platform independent binaries).
WORKDIR /source/src
RUN dotnet build "SolarCharger.csproj" -c Release -o /app/build/SolarCharger
RUN dotnet publish "SolarCharger.csproj" -c Release -o /app/publish/SolarCharger /p:UseAppHost=false --no-restore -f net8.0

# move binaries into smaller base image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080/tcp
WORKDIR /app/SolarCharger
ENV ConfigFolder /config
ENTRYPOINT ["dotnet", "SolarCharger.dll"]
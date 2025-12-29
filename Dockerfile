# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY *.csproj .
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Listen on port 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .

EXPOSE 5000

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY ./bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "TaxiApi.dll"]


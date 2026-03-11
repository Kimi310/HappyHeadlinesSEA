# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /

# Copy project files
COPY ProfanityService/ProfanityService.csproj ProfanityService/
WORKDIR /ProfanityService
RUN dotnet restore

# Copy source code
COPY ProfanityService/ .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /ProfanityService
RUN dotnet publish ProfanityService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ProfanityService.dll"]

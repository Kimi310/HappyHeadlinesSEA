# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["SubscriberCache/SubscriberCache.csproj", "SubscriberCache/"]
RUN dotnet restore "SubscriberCache/SubscriberCache.csproj"

# Copy everything else and build
COPY SubscriberCache/ SubscriberCache/
WORKDIR "/src/SubscriberCache"
RUN dotnet build "SubscriberCache.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SubscriberCache.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubscriberCache.dll"]
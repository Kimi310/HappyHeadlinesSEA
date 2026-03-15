# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["CommentCache/CommentCache.csproj", "CommentCache/"]
RUN dotnet restore "CommentCache/CommentCache.csproj"

# Copy everything else and build
COPY CommentCache/ CommentCache/
WORKDIR "/src/CommentCache"
RUN dotnet build "CommentCache.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "CommentCache.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommentCache.dll"]
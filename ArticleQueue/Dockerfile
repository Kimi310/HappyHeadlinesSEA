FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ArticleQueue/ArticleQueue.csproj", "ArticleQueue/"]
RUN dotnet restore "ArticleQueue/ArticleQueue.csproj"

COPY . .

WORKDIR /src/ArticleQueue
RUN dotnet build "ArticleQueue.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArticleQueue.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ArticleQueue.dll"]


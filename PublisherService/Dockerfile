FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PublisherService/PublisherService.csproj", "PublisherService/"]
RUN dotnet restore "PublisherService/PublisherService.csproj"

COPY . .

WORKDIR /src/PublisherService
RUN dotnet build "PublisherService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PublisherService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PublisherService.dll"]
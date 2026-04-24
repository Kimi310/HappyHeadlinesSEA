FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SubscriberService/SubscriberService.csproj", "SubscriberService/"]
RUN dotnet restore "SubscriberService/SubscriberService.csproj"

COPY . .

WORKDIR /src/SubscriberService
RUN dotnet build "SubscriberService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SubscriberService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SubscriberService.dll"]

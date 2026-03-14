FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NewsletterService/NewsletterService.csproj", "NewsletterService/"]
RUN dotnet restore "NewsletterService/NewsletterService.csproj"

COPY . .

WORKDIR /src/NewsletterService
RUN dotnet build "NewsletterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewsletterService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "NewsletterService.dll"]
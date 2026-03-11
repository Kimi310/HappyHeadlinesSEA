FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DraftService/DraftService.csproj", "DraftService/"]
RUN dotnet restore "DraftService/DraftService.csproj"

COPY . .

WORKDIR /src/DraftService
RUN dotnet build "DraftService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DraftService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 5201

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5201

ENTRYPOINT ["dotnet", "DraftService.dll"]
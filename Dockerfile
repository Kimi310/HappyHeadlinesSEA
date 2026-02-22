FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["API/API.csproj", "API/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Service/Service.csproj", "Service/"]

RUN dotnet restore "API/API.csproj"

COPY . .

WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5199
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5199

ENTRYPOINT ["dotnet", "API.dll"]
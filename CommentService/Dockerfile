FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CommentService/CommentService.csproj", "CommentService/"]
RUN dotnet restore "CommentService/CommentService.csproj"

COPY . .

WORKDIR /src/CommentService
RUN dotnet build "CommentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommentService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CommentService.dll"]
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TaskManager.Api.csproj
RUN dotnet publish TaskManager.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/data/tasks.db"
ENV FrontendOrigin="http://localhost:5173"

# Volume para persistência do SQLite
VOLUME ["/data"]

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
# Use Linux-based images for better compatibility unless you require Windows containers
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY TrainingTask.Server/TrainingTask.Server.csproj TrainingTask.Server/
COPY TrainingTask.Server/*.cs TrainingTask.Server/
COPY TrainingTask.Server/Controllers/ TrainingTask.Server/Controllers/
COPY TrainingTask.Server/Data/ TrainingTask.Server/Data/
COPY TrainingTask.Server/Models/ TrainingTask.Server/Models/
COPY TrainingTask.Server/Properties/ TrainingTask.Server/Properties/
COPY TrainingTask.Server/Services/ TrainingTask.Server/Services/
COPY TrainingTask.Server/wwwroot/ TrainingTask.Server/wwwroot/
RUN dotnet restore TrainingTask.Server/TrainingTask.Server.csproj
WORKDIR /src/TrainingTask.Server
# Only build the ASP.NET Core project, not the whole solution
RUN dotnet build TrainingTask.Server.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TrainingTask.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure ASP.NET Core listens on port 80 in the container
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "TrainingTask.Server.dll"]

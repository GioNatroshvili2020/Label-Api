# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["label-api/label-api.csproj", "label-api/"]
RUN dotnet restore "./label-api/label-api.csproj"
COPY . .
WORKDIR "/src/label-api"
RUN dotnet build "./label-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./label-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

# Create uploads directory and set permissions
RUN mkdir -p /app/uploads/coverart /app/uploads/audio
RUN chown -R $APP_UID:$APP_UID /app/uploads

COPY --from=publish /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "label-api.dll"]
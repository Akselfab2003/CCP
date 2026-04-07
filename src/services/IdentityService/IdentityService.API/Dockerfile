# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.Props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["src/services/IdentityService/IdentityService.API/IdentityService.API.csproj", "src/services/IdentityService/IdentityService.API/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/IdentityService/IdentityService.Application/IdentityService.Application.csproj", "src/services/IdentityService/IdentityService.Application/"]
COPY ["src/services/IdentityService/Keycloak.Sdk/Keycloak.Sdk.csproj", "src/services/IdentityService/Keycloak.Sdk/"]
COPY ["src/shared/CCP.Sdk.utils/CCP.Sdk.utils.csproj", "src/shared/CCP.Sdk.utils/"]
RUN dotnet restore "./src/services/IdentityService/IdentityService.API/IdentityService.API.csproj"
COPY . .
WORKDIR "/src/src/services/IdentityService/IdentityService.API"
RUN dotnet build "./IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
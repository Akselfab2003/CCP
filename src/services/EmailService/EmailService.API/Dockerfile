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
COPY ["src/services/EmailService/EmailService.API/EmailService.API.csproj", "src/services/EmailService/EmailService.API/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/EmailService/EmailService.Domain/EmailService.Domain.csproj", "src/services/EmailService/EmailService.Domain/"]
COPY ["src/services/EmailService/EmailService.Infrastructure/EmailService.Infrastructure.csproj", "src/services/EmailService/EmailService.Infrastructure/"]
COPY ["src/services/EmailService/EmailService.Application/EmailService.Application.csproj", "src/services/EmailService/EmailService.Application/"]
COPY ["src/services/EmailService/EmailTemplates/EmailTemplates.csproj", "src/services/EmailService/EmailTemplates/"]
RUN dotnet restore "./src/services/EmailService/EmailService.API/EmailService.API.csproj"
COPY . .
WORKDIR "/src/src/services/EmailService/EmailService.API"
RUN dotnet build "./EmailService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EmailService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailService.API.dll"]
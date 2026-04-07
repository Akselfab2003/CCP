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
COPY ["src/services/MessagingService/MessagingService.Api/MessagingService.Api.csproj", "src/services/MessagingService/MessagingService.Api/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/MessagingService/MessagingService.Application/MessagingService.Application.csproj", "src/services/MessagingService/MessagingService.Application/"]
COPY ["src/services/MessagingService/MessagingService.Infrastructure/MessagingService.Infrastructure.csproj", "src/services/MessagingService/MessagingService.Infrastructure/"]
COPY ["src/services/MessagingService/MessagingService.Domain/MessagingService.Domain.csproj", "src/services/MessagingService/MessagingService.Domain/"]
RUN dotnet restore "./src/services/MessagingService/MessagingService.Api/MessagingService.Api.csproj"
COPY . .
WORKDIR "/src/src/services/MessagingService/MessagingService.Api"
RUN dotnet build "./MessagingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MessagingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessagingService.Api.dll"]
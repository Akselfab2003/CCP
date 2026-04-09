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
COPY ["src/services/CustomerService/CustomerService.Api/CustomerService.Api.csproj", "src/services/CustomerService/CustomerService.Api/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/CustomerService/CustomerService.Application/CustomerService.Application.csproj", "src/services/CustomerService/CustomerService.Application/"]
COPY ["src/services/CustomerService/CustomerService.Domain/CustomerService.Domain.csproj", "src/services/CustomerService/CustomerService.Domain/"]
COPY ["src/services/CustomerService/CustomerService.Infrastructure/CustomerService.Infrastructure.csproj", "src/services/CustomerService/CustomerService.Infrastructure/"]
COPY ["src/shared/CCP.Encryption/CCP.Encryption.csproj", "src/shared/CCP.Encryption/"]
RUN dotnet restore "./src/services/CustomerService/CustomerService.Api/CustomerService.Api.csproj"
COPY . .
WORKDIR "/src/src/services/CustomerService/CustomerService.Api"
RUN dotnet build "./CustomerService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CustomerService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]
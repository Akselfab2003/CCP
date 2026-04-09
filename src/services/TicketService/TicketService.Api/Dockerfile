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
COPY ["src/services/TicketService/TicketService.Api/TicketService.Api.csproj", "src/services/TicketService/TicketService.Api/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/TicketService/TicketService.Application/TicketService.Application.csproj", "src/services/TicketService/TicketService.Application/"]
COPY ["src/services/TicketService/TicketService.Domain/TicketService.Domain.csproj", "src/services/TicketService/TicketService.Domain/"]
COPY ["src/services/TicketService/TicketService.Infrastructure/TicketService.Infrastructure.csproj", "src/services/TicketService/TicketService.Infrastructure/"]
RUN dotnet restore "./src/services/TicketService/TicketService.Api/TicketService.Api.csproj"
COPY . .
WORKDIR "/src/src/services/TicketService/TicketService.Api"
RUN dotnet build "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketService.Api.dll"]
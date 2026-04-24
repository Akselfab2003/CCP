# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project

# --- Node.js builder stage for JS bundling ---
FROM node:lts AS node-builder
WORKDIR /chatbot
COPY src/ui/CCP.UI/wwwroot/chatbot/package.json src/ui/CCP.UI/wwwroot/chatbot/webpack.config.js ./
COPY src/ui/CCP.UI/wwwroot/chatbot/ChatBot.js ./
COPY src/ui/CCP.UI/wwwroot/chatbot/ChatBot.css ./
RUN npm install && npm run build

# --- .NET build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.Props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["src/ui/CCP.UI/CCP.UI.csproj", "src/ui/CCP.UI/"]
COPY ["src/apphost/CCP.ServiceDefaults/CCP.ServiceDefaults.csproj", "src/apphost/CCP.ServiceDefaults/"]
COPY ["src/shared/CCP.Shared/CCP.Shared.csproj", "src/shared/CCP.Shared/"]
COPY ["src/services/IdentityService/IdentityService.Sdk/IdentityService.Sdk.csproj", "src/services/IdentityService/IdentityService.Sdk/"]
COPY ["src/shared/CCP.Sdk.utils/CCP.Sdk.utils.csproj", "src/shared/CCP.Sdk.utils/"]
COPY ["src/services/MessagingService/MessagingService.Sdk/MessagingService.Sdk.csproj", "src/services/MessagingService/MessagingService.Sdk/"]
COPY ["src/services/TicketService/TicketService.Sdk/TicketService.Sdk.csproj", "src/services/TicketService/TicketService.Sdk/"]
RUN dotnet restore "./src/ui/CCP.UI/CCP.UI.csproj"
COPY . .
# Copy only the bundled JS from node-builder, not node_modules
COPY --from=node-builder /chatbot/chatbot.bundle.js src/ui/CCP.UI/wwwroot/chatbot.bundle.js
COPY --from=node-builder /chatbot/ChatBot.css  src/ui/CCP.UI/wwwroot/ChatBot.css 
WORKDIR "/src/src/ui/CCP.UI"
RUN dotnet tool install --global Microsoft.OpenApi.Kiota && \
	export PATH="$PATH:/root/.dotnet/tools" && \
	dotnet build "./CCP.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CCP.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CCP.UI.dll"]
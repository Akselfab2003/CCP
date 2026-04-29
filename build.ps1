if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error "The .NET SDK is not installed or not in the system PATH. Please install it from https://dotnet.microsoft.com/download and ensure it's added to your PATH."
    exit 1
}

# Install kiota if not already installed
if (-not (Get-Command "kiota" -ErrorAction SilentlyContinue)) {
    Write-Host "Kiota is not installed. Installing Kiota CLI tool..."
    dotnet tool install --global Microsoft.Kiota.Cli
} else {
    Write-Host "Kiota is already installed."
}


$bytes = New-Object byte[] 32; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes); 
write-host  ([Convert]::ToBase64String($bytes))


dotnet user-secrets set "SERVICE_ACCOUNT_SECRET" ([System.Guid]::NewGuid()).ToString() --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "ROUNDCUBE_DEFAULT_USER_PASSWORD" "passwd123" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "ROUNDCUBE_DEFAULT_USER_EMAIL" "admin@example.com" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "REALM_IMPORT_PATH" (Join-Path $PSScriptRoot -ChildPath ".\config\CCP_Realm.json") --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "MAILCOW_API_KEY" "<# REDACTED #>" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "MAILCOW_API_URL" "<# REDACTED #>" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "KeycloakAdminApiClientSecret" ([System.Guid]::NewGuid()).ToString() --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "Encryption_Key" ([Convert]::ToBase64String($bytes)) --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "emailWorkerServiceUsername" "<# REDACTED #>" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "emailWorkerServicePassword" "<# REDACTED #>" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj
dotnet user-secrets set "emailHostUrl" "<# REDACTED #>" --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj


dotnet run --project .\src\apphost\CCP.AppHost\CCP.AppHost.csproj --configuration Release -p:OpenApiGenerateDocuments=false
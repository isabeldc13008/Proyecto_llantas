$ErrorActionPreference = 'Stop'
$apiProject = Join-Path $PSScriptRoot '..\src\SistemaLlantas.Api\SistemaLlantas.Api.csproj'
$sqlCredential = Get-Credential -Message 'Credenciales SQL para GDLLSQLDLLO (no las del formulario GLLD)'
if ($null -eq $sqlCredential) { return }
$previousConnection = $env:ConnectionStrings__SqlServer
$previousUsername = $env:SqlCredentials__Username
$previousPassword = $env:SqlCredentials__Password
try {
    $env:ConnectionStrings__SqlServer = 'Server=tcp:srvsqlgdlldllo.database.windows.net,1433;Database=GDLLSQLDLLO;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30'
    $env:SqlCredentials__Username = $sqlCredential.UserName
    $env:SqlCredentials__Password = $sqlCredential.GetNetworkCredential().Password
    dotnet run --project $apiProject --launch-profile local
}
finally {
    $env:ConnectionStrings__SqlServer = $previousConnection
    $env:SqlCredentials__Username = $previousUsername
    $env:SqlCredentials__Password = $previousPassword
    $sqlCredential = $null
}

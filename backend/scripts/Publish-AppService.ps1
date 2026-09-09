param([switch]$Deploy)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$frontend = Join-Path $repo 'frontend\sistema-llantas'
$buildRoot = Join-Path $repo ('artifacts\appservice-' + [Guid]::NewGuid().ToString('N'))
$package = Join-Path $buildRoot 'package'
$angular = Join-Path $buildRoot 'angular'
$zip = Join-Path $buildRoot 'GLLDAPWBDLLO.zip'
$frontendBuild = Join-Path $buildRoot 'frontend'
New-Item -ItemType Directory -Path $package -Force | Out-Null
New-Item -ItemType Directory -Path $frontendBuild -Force | Out-Null
# Build from a clean copy so npm ci never has to delete a locked esbuild.exe in the working tree.
robocopy $frontend $frontendBuild /E /XD node_modules dist /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -gt 7) { throw 'No fue posible copiar el frontend al directorio temporal.' }
Push-Location $frontendBuild
try {
    npm.cmd ci
    if ($LASTEXITCODE -ne 0) { throw 'Falló npm ci.' }
    npm.cmd run build -- --configuration production --output-path $angular
    if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación Angular.' }
}
finally { Pop-Location }
dotnet publish (Join-Path $repo 'backend\src\SistemaLlantas.Api\SistemaLlantas.Api.csproj') -c Release -o $package -p:UseAppHost=false
if ($LASTEXITCODE -ne 0) { throw 'Falló dotnet publish.' }
$browser = Join-Path $angular 'browser'
if (!(Test-Path (Join-Path $browser 'index.html'))) { throw 'Falta index.html de Angular.' }
$wwwroot = Join-Path $package 'wwwroot'
New-Item -ItemType Directory -Path $wwwroot -Force | Out-Null
Copy-Item -Path (Join-Path $browser '*') -Destination $wwwroot -Recurse -Force
$development = Join-Path $package 'appsettings.Development.json'
if (Test-Path -LiteralPath $development) { Remove-Item -LiteralPath $development }
if (!(Test-Path (Join-Path $package 'web.config'))) { throw 'Falta web.config para Windows App Service.' }
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $zip
Write-Host "Paquete preparado: $zip"
if ($Deploy) {
    $subscription = '39ffc6d4-3091-4c4e-9696-2096badeef4f'
    $group = 'RG_DLLO_GDLL'
    $name = 'GLLDAPWBDLLO'
    $settingNames = az webapp config appsettings list --subscription $subscription -g $group -n $name --query '[].name' -o json
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo consultar App Service. Revisa az login y tus permisos.' }
    $settingNames = $settingNames | ConvertFrom-Json
    foreach ($required in @('ASPNETCORE_ENVIRONMENT','Jwt__Key','ConnectionStrings__SqlServer')) {
        if ($required -notin $settingNames) { throw "Configura $required en App Service antes de desplegar." }
    }
    az webapp deploy --subscription $subscription -g $group -n $name --src-path $zip --type zip
    if ($LASTEXITCODE -ne 0) { throw 'El despliegue falló. El paquete queda disponible para revisar.' }
    Write-Host 'Despliegue enviado. Verifica https://glldapwbdllo-gpbke2cwgzakdhfg.eastus2-01.azurewebsites.net/acceso'
}

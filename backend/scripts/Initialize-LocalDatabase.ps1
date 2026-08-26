param(
    [string]$Server = 'localhost\SQLEXPRESS',
    [string]$Database = 'SistemaLlantas',
    [switch]$IncludeOperationalDemo
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

$env:Jwt__Key = 'GLLD-LOCAL-MIGRATION-ONLY-CHANGE-BEFORE-RUNNING'
dotnet ef database update `
  --project (Join-Path $repoRoot 'backend\src\SistemaLlantas.Infrastructure\SistemaLlantas.Infrastructure.csproj') `
  --startup-project (Join-Path $repoRoot 'backend\src\SistemaLlantas.Api\SistemaLlantas.Api.csproj') `
  --connection "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
if ($LASTEXITCODE -ne 0) { throw 'No fue posible aplicar las migraciones.' }

if ($IncludeOperationalDemo) {
    $centersFile = Join-Path $repoRoot 'frontend\sistema-llantas\src\app\core\data\centers.ts'
    $seedFile = Join-Path $PSScriptRoot 'seed-operational-data.sql'
    $tempFile = Join-Path ([System.IO.Path]::GetTempPath()) ("glld-centers-{0}.sql" -f [Guid]::NewGuid())
    if (-not (Test-Path -LiteralPath $centersFile)) { throw "No existe el catálogo fuente requerido para cargar la demo: $centersFile" }

    $source = Get-Content -Raw -LiteralPath $centersFile
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRANSACTION;')
    foreach ($match in [regex]::Matches($source, "(?m)^(R[1-4]):'([^']+)'")) {
        $relevance = $match.Groups[1].Value
        foreach ($entry in $match.Groups[2].Value.Split(';')) {
            $parts = $entry.Split(':', 2)
            $code = $parts[0].Replace("'", "''")
            $name = $parts[1].Replace("'", "''")
            $lines.Add("IF NOT EXISTS (SELECT 1 FROM TBL_Centro WHERE Codigo=N'$code') INSERT TBL_Centro (Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre,Relevancia) VALUES (NEWID(),SYSDATETIMEOFFSET(),N'seed-local',1,N'$code',N'$name',N'$relevance');")
        }
    }
    $lines.Add('COMMIT TRANSACTION;')
    [System.IO.File]::WriteAllLines($tempFile, $lines, [System.Text.UTF8Encoding]::new($false))
    try {
        sqlcmd -S $Server -d $Database -E -C -b -i $tempFile
        if ($LASTEXITCODE -ne 0) { throw 'No fue posible cargar los centros.' }
        sqlcmd -S $Server -d $Database -E -C -b -i $seedFile
        if ($LASTEXITCODE -ne 0) { throw 'No fue posible cargar los datos operativos.' }
    }
    finally {
        if (Test-Path -LiteralPath $tempFile) { Remove-Item -LiteralPath $tempFile -Force }
    }
}
else {
    Write-Host 'Migraciones aplicadas. La carga de datos operativos de demostración está deshabilitada.'
}

sqlcmd -S $Server -d $Database -E -C -Q "SET NOCOUNT ON; SELECT (SELECT COUNT(*) FROM TBL_Centro) Centros,(SELECT COUNT(*) FROM TBL_Vehiculo) Vehiculos,(SELECT COUNT(*) FROM TBL_PosicionVehiculo) Posiciones,(SELECT COUNT(*) FROM TBL_Llanta) Llantas;"

param([Parameter(Mandatory)][string]$PackagePath)
$ErrorActionPreference = 'Stop'
$package = (Resolve-Path -LiteralPath $PackagePath).Path
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$start = [Diagnostics.ProcessStartInfo]::new('dotnet')
$start.ArgumentList.Add((Join-Path $package 'SistemaLlantas.Api.dll'))
$start.WorkingDirectory = $package
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
# Child process only: never inherit a real database connection or signing key.
foreach ($name in @($start.Environment.Keys)) {
    if ($name -match '^(ConnectionStrings|SQLCONNSTR_|SQLAZURECONNSTR_|CUSTOMCONNSTR_|SqlCredentials|Jwt|ASPNETCORE_|DOTNET_ENVIRONMENT)') {
        $start.Environment.Remove($name) | Out-Null
    }
}
$start.Environment['ASPNETCORE_ENVIRONMENT'] = 'QA'
$start.Environment['DOTNET_ENVIRONMENT'] = 'QA'
$start.Environment['ASPNETCORE_URLS'] = "http://127.0.0.1:$port"
$start.Environment['ConnectionStrings__SqlServer'] = 'Server=127.0.0.1,1;Database=QaSmokeNeverConnect;Integrated Security=True;Connect Timeout=1;Encrypt=True;TrustServerCertificate=True'
$start.Environment['Jwt__Key'] = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
$start.Environment['FrontendUrl'] = "http://127.0.0.1:$port"
$start.Environment['Authentication__SeedDevelopmentUsers'] = 'false'
$process = [Diagnostics.Process]::new()
$process.StartInfo = $start
try {
    $process.Start() | Out-Null
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        if ($process.HasExited) { throw "QA process exited with code $($process.ExitCode). $($stderr.GetAwaiter().GetResult())" }
        try {
            $response = Invoke-WebRequest "http://127.0.0.1:$port/" -TimeoutSec 2
            if ($response.StatusCode -eq 200 -and $response.Content -match '<app-root') { $ready = $true; break }
        } catch { }
        Start-Sleep -Milliseconds 500
    }
    if (!$ready) { throw 'QA package did not serve Angular within the startup timeout.' }
    $missing = Invoke-WebRequest "http://127.0.0.1:$port/api/qa-smoke-missing" -SkipHttpErrorCheck
    if ($missing.StatusCode -ne 404) { throw 'Unknown API route must return 404.' }
    Write-Output 'PASS: QA startup, Angular index and API routing; no SQL access required.'
} finally {
    if (!$process.HasExited) { $process.Kill($true); $process.WaitForExit() }
    $process.Dispose()
}

param(
  [int]$Port = 5090,
  [string]$ProjectPath = '../../Api/OrderManagement.Api.csproj',
  [switch]$NoBuild,
  [int]$StartupTimeoutSeconds = 40,
  [switch]$VerboseLog
)

$ErrorActionPreference = 'Stop'

function Write-Log($msg,[ConsoleColor]$c=[ConsoleColor]::Cyan){
  $old = $Host.UI.RawUI.ForegroundColor; $Host.UI.RawUI.ForegroundColor=$c; Write-Host $msg; $Host.UI.RawUI.ForegroundColor=$old;
}

# Target (preferred) port, but will auto-detect if launchSettings overrides
$preferredBase = "http://127.0.0.1:$Port"
Write-Log "==> Smoke test starting (preferred=$preferredBase)"

# Resolve project
$projFull = Resolve-Path (Join-Path $PSScriptRoot $ProjectPath)
$projDir  = Split-Path $projFull -Parent

if (-not $NoBuild){
  Write-Log '==> Building API'
  dotnet build $projFull -nologo | Out-Null
  if ($LASTEXITCODE -ne 0){ throw 'Build failed' }
}

# Prepare log redirection
$stdout = Join-Path $env:TEMP 'om-smoke-api-out.log'
$stderr = Join-Path $env:TEMP 'om-smoke-api-err.log'
Remove-Item $stdout,$stderr -ErrorAction SilentlyContinue

# Environment for lightweight run (attempt to force port, may be overridden by launchSettings)
$env:USE_INMEMORY_DB = '1'
$env:ASPNETCORE_URLS = "http://0.0.0.0:$Port"
$env:DOTNET_ENVIRONMENT = 'Development'
$env:DOTNET_SKIP_LAUNCH_PROFILE = '1'
$env:DISABLE_LAUNCH_SETTINGS = '1'

Write-Log "==> Launching API (logs: $stdout / $stderr)"
$apiProcess = Start-Process dotnet -ArgumentList @('run','--no-build','--project',$projFull) -WorkingDirectory $projDir -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr

# Detect actual listening URL by parsing stdout
$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
$publicUrl = $null
$httpPattern = 'Now listening on:\s*(http://[^\s]+)'
while (-not $publicUrl -and (Get-Date) -lt $deadline){
  Start-Sleep -Milliseconds 400
  if (Test-Path $stdout){
    $lines = Get-Content $stdout -Tail 40
    foreach($l in $lines){
      if ($l -match $httpPattern){
        $publicUrl = $Matches[1]
        break
      }
    }
  }
}

if (-not $publicUrl){
  Write-Log 'Could not detect listening URL; dumping logs:' Yellow
  Get-Content $stderr -ErrorAction SilentlyContinue | Select-Object -Last 40
  Get-Content $stdout -ErrorAction SilentlyContinue | Select-Object -Last 80
  if ($apiProcess -and -not $apiProcess.HasExited){ Stop-Process -Id $apiProcess.Id -Force }
  exit 1
}
Write-Log "Detected URL: $publicUrl" Green

# Probe health (optional)
$healthOk = $false
try { $r = Invoke-WebRequest -Uri "$publicUrl/health" -UseBasicParsing -TimeoutSec 5; if ($r.StatusCode -eq 200){ $healthOk=$true } } catch {}
if (-not $healthOk){ Write-Log 'Health endpoint not reachable, continuing anyway...' Yellow }

function Invoke-JsonPost($url,$obj){ Invoke-RestMethod -Method Post -Uri $url -Body ($obj | ConvertTo-Json -Depth 8) -ContentType 'application/json' }

try {
  Write-Log '==> Placing order'
  $placeBody = @{ customerName = 'SMOKE_PowerShell'; lines = @(@{ product='SMOKE_PS_PRODUCT'; quantity=1; price=3.5; currency='USD' }) }
  $placeResp = Invoke-JsonPost "$publicUrl/api/orders" $placeBody
  if (-not $placeResp.orderId){ throw 'PlaceOrder: orderId missing' }
  Write-Log "   OrderId: $($placeResp.orderId)" Green

  Write-Log '==> Updating status'
  Invoke-RestMethod -Method Put -Uri "$publicUrl/api/orders/$($placeResp.orderId)/status" -Body (@{ status='Completed'} | ConvertTo-Json) -ContentType 'application/json'

  Write-Log '==> Getting order'
  $getResp = Invoke-RestMethod -Method Get -Uri "$publicUrl/api/orders/$($placeResp.orderId)"
  if ($getResp.status -ne 'Completed'){ throw 'GetOrder: status not updated' }
  if (-not ($getResp.lines | Where-Object { $_.product -eq 'SMOKE_PS_PRODUCT' })){ throw 'GetOrder: product not found in lines' }
  Write-Log '   GetOrder OK' Green

  Write-Log '==> Creating extra orders for sort check'
  Invoke-JsonPost "$publicUrl/api/orders" @{ customerName = 'SMOKE_SORT_B'; lines = @(@{ product='SMOKE_SORT_P'; quantity=1; price=1; currency='USD' }) } | Out-Null
  Start-Sleep -Milliseconds 50
  Invoke-JsonPost "$publicUrl/api/orders" @{ customerName = 'SMOKE_SORT_A'; lines = @(@{ product='SMOKE_SORT_P'; quantity=1; price=1; currency='USD' }) } | Out-Null

  Write-Log '==> Listing orders with sortBy=customerName asc'
  $listResp = Invoke-RestMethod -Method Get -Uri "$publicUrl/api/orders?page=1&pageSize=10&sortBy=customerName&desc=false"
  if (-not $listResp.orders){ throw 'ListOrders: orders field missing' }
  $names = $listResp.orders | Select-Object -ExpandProperty customerName
  $alphaIndex = [Array]::IndexOf($names, 'SMOKE_SORT_A')
  $betaIndex  = [Array]::IndexOf($names, 'SMOKE_SORT_B')
  if ($alphaIndex -lt 0 -or $betaIndex -lt 0 -or $alphaIndex -ge $betaIndex){ throw 'Sorting verification failed' }
  Write-Log '   Sorting OK' Green

  Write-Log 'Smoke test PASSED' Green
  exit 0
}
catch {
  Write-Log "Smoke test FAILED: $($_.Exception.Message)" Red
  if ($VerboseLog){
    Write-Log '--- STDOUT (tail) ---' Yellow; Get-Content $stdout -ErrorAction SilentlyContinue | Select-Object -Last 120
    Write-Log '--- STDERR (tail) ---' Yellow; Get-Content $stderr -ErrorAction SilentlyContinue | Select-Object -Last 120
  }
  exit 1
}
finally {
  if ($apiProcess -and -not $apiProcess.HasExited){ Stop-Process -Id $apiProcess.Id -Force }
  Remove-Item Env:USE_INMEMORY_DB -ErrorAction SilentlyContinue | Out-Null
  Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue | Out-Null
  Remove-Item Env:DOTNET_ENVIRONMENT -ErrorAction SilentlyContinue | Out-Null
  Remove-Item Env:DOTNET_SKIP_LAUNCH_PROFILE -ErrorAction SilentlyContinue | Out-Null
  Remove-Item Env:DISABLE_LAUNCH_SETTINGS -ErrorAction SilentlyContinue | Out-Null
}

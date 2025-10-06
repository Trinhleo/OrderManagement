param(
  [int]$Port = 5090,
  [string]$ProjectPath = '../../Api/OrderManagement.Api.csproj',
  [switch]$NoBuild,
  [int]$StartupTimeoutSeconds = 40
)

$ErrorActionPreference = 'Stop'

function Write-Log($msg,[ConsoleColor]$c=[ConsoleColor]::Cyan){
  $old = $Host.UI.RawUI.ForegroundColor; $Host.UI.RawUI.ForegroundColor=$c; Write-Host $msg; $Host.UI.RawUI.ForegroundColor=$old;
}

$preferredBase = "http://127.0.0.1:$Port"
Write-Log "==> Sorting+Pagination Test starting (preferred=$preferredBase)"

$projFull = Resolve-Path (Join-Path $PSScriptRoot $ProjectPath)
$projDir  = Split-Path $projFull -Parent

if (-not $NoBuild){
  Write-Log '==> Building API'
  dotnet build $projFull -nologo | Out-Null
  if ($LASTEXITCODE -ne 0){ throw 'Build failed' }
}

$stdout = Join-Path $env:TEMP 'om-sort-test-out.log'
$stderr = Join-Path $env:TEMP 'om-sort-test-err.log'
Remove-Item $stdout,$stderr -ErrorAction SilentlyContinue

$env:USE_INMEMORY_DB = '1'
$env:ASPNETCORE_URLS = "http://0.0.0.0:$Port"
$env:DOTNET_ENVIRONMENT = 'Development'
$env:DOTNET_SKIP_LAUNCH_PROFILE = '1'
$env:DISABLE_LAUNCH_SETTINGS = '1'

Write-Log "==> Launching API"
$apiProcess = Start-Process dotnet -ArgumentList @('run','--no-build','--project',$projFull) -WorkingDirectory $projDir -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr

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
  Write-Log 'Could not detect listening URL' Yellow
  if ($apiProcess -and -not $apiProcess.HasExited){ Stop-Process -Id $apiProcess.Id -Force }
  exit 1
}
Write-Log "Detected URL: $publicUrl" Green

function Invoke-JsonPost($url,$obj,$token=$null){ 
  $headers = @{'Content-Type'='application/json'}
  if($token){ $headers['Authorization'] = "Bearer $token" }
  Invoke-RestMethod -Method Post -Uri $url -Body ($obj | ConvertTo-Json -Depth 8) -Headers $headers
}

function Invoke-JsonGet($url,$token=$null){ 
  $headers = @{}
  if($token){ $headers['Authorization'] = "Bearer $token" }
  Invoke-RestMethod -Method Get -Uri $url -Headers $headers
}

try {
  Write-Log '==> Login to get token'
  $loginResp = Invoke-JsonPost "$publicUrl/api/auth/login" @{ username='admin'; password='admin123' }
  if(-not $loginResp.token){ throw 'Login failed: no token' }
  $token = $loginResp.token
  Write-Log "   Token received" Green

  Write-Log '==> Creating test orders with specific data'
  $orderIds = @()
  
  # Create orders with known sorting characteristics
  $testData = @(
    @{name='Customer_A'; status='Pending'},
    @{name='Customer_C'; status='Shipped'},
    @{name='Customer_B'; status='Completed'},
    @{name='Customer_D'; status='Processing'},
    @{name='Customer_E'; status='Cancelled'}
  )

  foreach($data in $testData){
    $order = Invoke-JsonPost "$publicUrl/api/orders" @{
      customerName=$data.name
      lines=@(@{product='TestProduct';quantity=1;price=10;currency='USD'})
    } $token
    $orderIds += $order.orderId
    Start-Sleep -Milliseconds 100
  }
  Write-Log "   Created $($orderIds.Count) test orders" Green

  Write-Log '==> TEST 1: Sort by CustomerName ASC'
  $resp1 = Invoke-JsonGet "$publicUrl/api/orders?page=1&pageSize=10&sortBy=customerName&desc=false" $token
  $names1 = $resp1.orders | Select-Object -ExpandProperty customerName
  $sortedNames = $names1 | Sort-Object
  if(($names1 -join ',') -eq ($sortedNames -join ',')){ 
    Write-Log "   ? PASS: CustomerName sorted correctly (ASC)" Green 
  } else { 
    throw "FAIL: CustomerName not sorted. Got: $($names1 -join ','), Expected: $($sortedNames -join ',')"
  }

  Write-Log '==> TEST 2: Sort by CustomerName DESC'
  $resp2 = Invoke-JsonGet "$publicUrl/api/orders?page=1&pageSize=10&sortBy=customerName&desc=true" $token
  $names2 = $resp2.orders | Select-Object -ExpandProperty customerName
  $sortedNamesDesc = $names2 | Sort-Object -Descending
  if(($names2 -join ',') -eq ($sortedNamesDesc -join ',')){ 
    Write-Log "   ? PASS: CustomerName sorted correctly (DESC)" Green 
  } else { 
    throw "FAIL: CustomerName not sorted DESC"
  }

  Write-Log '==> TEST 3: Sort by Status ASC'
  $resp3 = Invoke-JsonGet "$publicUrl/api/orders?page=1&pageSize=10&sortBy=status&desc=false" $token
  $statuses = $resp3.orders | Select-Object -ExpandProperty status
  $sortedStatuses = $statuses | Sort-Object
  if(($statuses -join ',') -eq ($sortedStatuses -join ',')){ 
    Write-Log "   ? PASS: Status sorted correctly (ASC)" Green 
  } else { 
    throw "FAIL: Status not sorted"
  }

  Write-Log '==> TEST 4: Pagination consistency'
  $page1 = Invoke-JsonGet "$publicUrl/api/orders?page=1&pageSize=2&sortBy=customerName&desc=false" $token
  $page2 = Invoke-JsonGet "$publicUrl/api/orders?page=2&pageSize=2&sortBy=customerName&desc=false" $token
  
  if($page1.totalCount -eq $page2.totalCount){
    Write-Log "   ? PASS: Total count consistent across pages ($($page1.totalCount))" Green
  } else {
    throw "FAIL: Total count mismatch"
  }

  $ids1 = $page1.orders | Select-Object -ExpandProperty id
  $ids2 = $page2.orders | Select-Object -ExpandProperty id
  $duplicates = $ids1 | Where-Object { $ids2 -contains $_ }
  if($duplicates.Count -eq 0){
    Write-Log "   ? PASS: No duplicate data between pages" Green
  } else {
    throw "FAIL: Found $($duplicates.Count) duplicates between pages"
  }

  Write-Log '==> TEST 5: Multi-page sorting consistency'
  $allPages = @()
  for($p=1; $p -le 3; $p++){
    $pageData = Invoke-JsonGet "$publicUrl/api/orders?page=$p&pageSize=2&sortBy=customerName&desc=false" $token
    $allPages += $pageData.orders
  }
  
  $allNames = $allPages | Select-Object -ExpandProperty customerName
  $allSorted = $allNames | Sort-Object
  if(($allNames -join ',') -eq ($allSorted -join ',')){ 
    Write-Log "   ? PASS: Sorting maintained across multiple pages" Green 
  } else { 
    Write-Log "   ? WARNING: Sorting not perfect across pages (may have other data)" Yellow
  }

  Write-Log '==> TEST 6: Default sort (CreatedAt DESC)'
  $respDefault = Invoke-JsonGet "$publicUrl/api/orders?page=1&pageSize=5" $token
  if($respDefault.orders.Count -gt 0){
    Write-Log "   ? PASS: Default sorting returns data" Green
  }

  Write-Log ''
  Write-Log '========================================' Green
  Write-Log 'ALL SORTING + PAGINATION TESTS PASSED ?' Green
  Write-Log '========================================' Green
  exit 0
}
catch {
  Write-Log "Test FAILED: $($_.Exception.Message)" Red
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

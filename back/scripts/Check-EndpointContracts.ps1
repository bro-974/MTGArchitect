param(
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$ReadmeRelativePath = "README"
)

$ErrorActionPreference = "Stop"

function Normalize-EndpointPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $Path
    }

    $normalized = $Path.Split('?')[0]
    $normalized = [regex]::Replace($normalized, '\{([^}:]+):[^}]+\}', '{$1}')

    if ($normalized.Length -gt 1) {
        $normalized = $normalized.TrimEnd('/')
    }

    return $normalized
}

function Get-SourceEndpoints {
    param(
        [string]$FilePath,
        [string]$DefaultPrefix,
        [hashtable]$ReceiverPrefixes
    )

    $content = Get-Content -Path $FilePath -Raw
    $matches = [regex]::Matches($content, '(?<receiver>\w+)\.Map(?<verb>Get|Post|Put|Delete|Patch)\(\s*"(?<route>[^"]+)"')

    $results = New-Object System.Collections.Generic.List[string]

    foreach ($match in $matches) {
        $receiver = $match.Groups["receiver"].Value
        $verb = $match.Groups["verb"].Value.ToUpperInvariant()
        $route = $match.Groups["route"].Value

        $prefix = $DefaultPrefix
        if ($ReceiverPrefixes.ContainsKey($receiver)) {
            $prefix = $ReceiverPrefixes[$receiver]
        }

        $fullPath = if ($route.StartsWith("/api/") -or $route -eq "/") {
            $route
        }
        elseif ([string]::IsNullOrWhiteSpace($prefix)) {
            if ($route.StartsWith('/')) { $route } else { "/$route" }
        }
        else {
            if ($route.StartsWith('/')) { "$prefix$route" } else { "$prefix/$route" }
        }

        $normalizedPath = Normalize-EndpointPath $fullPath
        $results.Add("$verb $normalizedPath")
    }

    return $results
}

function Get-DocumentedEndpoints {
    param([string]$ReadmePath)

    $content = Get-Content -Path $ReadmePath -Raw
    $matches = [regex]::Matches($content, '####\s+`(?<method>GET|POST|PUT|DELETE|PATCH)\s+(?<path>[^`]+)`')

    $results = New-Object System.Collections.Generic.List[string]

    foreach ($match in $matches) {
        $method = $match.Groups["method"].Value.ToUpperInvariant()
        $path = Normalize-EndpointPath $match.Groups["path"].Value
        $results.Add("$method $path")
    }

    return $results
}

$apiEndpointFile = Join-Path $WorkspaceRoot "MTGArchitectServices.ApiService/Endpoint.cs"
$authEndpointFile = Join-Path $WorkspaceRoot "MTGArchitectServices.AuthApiService/Endpoint.cs"
$readmePath = Join-Path $WorkspaceRoot $ReadmeRelativePath

if (-not (Test-Path $apiEndpointFile)) {
    throw "API endpoint file not found: $apiEndpointFile"
}

if (-not (Test-Path $authEndpointFile)) {
    throw "Auth endpoint file not found: $authEndpointFile"
}

if (-not (Test-Path $readmePath)) {
    throw "README file not found: $readmePath"
}

$sourceEndpoints = New-Object System.Collections.Generic.List[string]
$sourceEndpoints.AddRange((Get-SourceEndpoints -FilePath $apiEndpointFile -DefaultPrefix "/api" -ReceiverPrefixes @{
    secured = "/api"
    apiRoot = "/api"
    cards = "/api/cards"
    app = ""
}))
$sourceEndpoints.AddRange((Get-SourceEndpoints -FilePath $authEndpointFile -DefaultPrefix "" -ReceiverPrefixes @{ app = "" }))

$sourceUnique = $sourceEndpoints | Sort-Object -Unique
$docUnique = (Get-DocumentedEndpoints -ReadmePath $readmePath) | Sort-Object -Unique

$missingInReadme = $sourceUnique | Where-Object { $_ -notin $docUnique }
$extraInReadme = $docUnique | Where-Object { $_ -notin $sourceUnique }

if (($missingInReadme.Count -eq 0) -and ($extraInReadme.Count -eq 0)) {
    Write-Host "Endpoint documentation is up to date."
    exit 0
}

Write-Host "Endpoint documentation is out of date."

if ($missingInReadme.Count -gt 0) {
    Write-Host "\nMissing from README:"
    $missingInReadme | ForEach-Object { Write-Host " - $_" }
}

if ($extraInReadme.Count -gt 0) {
    Write-Host "\nDocumented but not found in endpoints:"
    $extraInReadme | ForEach-Object { Write-Host " - $_" }
}

exit 1

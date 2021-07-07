[CmdletBinding()]
Param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir,
    [Parameter(Mandatory = $true)]
    [string]$TargetDir,
    [Parameter(Mandatory = $true)]
    [string]$SevenZipDir
)

try {
    $modJsonPath = [System.IO.Path]::Combine($ProjectDir, "mod.json")
    if (Test-Path -Path $modJsonPath) {
        $path = [System.IO.Path]::Combine($TargetDir, "mod.json")
        Copy-Item $modJsonPath -Destination $path
        Write-Output "Copied mod.json -> $($path)"
    }
    else {
        throw New-Object System.IO.FileNotFoundException "mod.json not found at $($modJsonPath)"
    }

    $7zaPath = [System.IO.Path]::Combine($SevenZipDir, "tools", "7za.exe")
    if (Test-Path -Path $7zaPath) {
        $zipPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($TargetDir, "..\ForceResolution.zip"))
        Invoke-Expression -Command "$($7zaPath) a `"$($zipPath)`" `"$($TargetDir)`""
    }
    else {
        throw New-Object System.IO.FileNotFoundException "7za.exe not found at $($7zaPath)"
    }
}
catch {
    Write-Output "Error: $($PSItem.ToString())" 
    exit 1
}
param(
    [string]$config = "Release"
)

$scripts = $PSScriptRoot
$root = Split-Path $scripts -Parent
$bin_dir = "$root\src\bin"
$build_files = "$bin_dir\$config\*.*"

Compress-Archive -Path $build_files -DestinationPath "$bin_dir\Crash.Server.zip" -Force -CompressionLevel Optimal
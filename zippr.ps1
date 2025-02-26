$inputFile = "sample.zip"
$outputDir = "split_parts"
$chunkSize = 3GB  # 3 gigabyte chunks

if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$baseName = [System.IO.Path]::GetFileNameWithoutExtension($inputFile)
$fileBytes = [System.IO.File]::ReadAllBytes($inputFile)
$totalSize = $fileBytes.Length
$part = 0

for ($i = 0; $i -lt $totalSize; $i += $chunkSize) {
    $chunk = $fileBytes[$i..[Math]::Min($i + $chunkSize - 1, $totalSize - 1)]
    $partFile = "$outputDir\$baseName.part$part"
    [System.IO.File]::WriteAllBytes($partFile, $chunk)
    Write-Output "Created $partFile, Size: $(($chunk.Length/1GB).ToString("F2")) GB"
    $part++
}

# Merge script
function Merge-Zip {
    param (
        [string]$outputFile,
        [string]$inputDir
    )
    
    $partFiles = Get-ChildItem -Path $inputDir -Filter "$baseName.part*" | Sort-Object Name
    $outputStream = [System.IO.File]::OpenWrite($outputFile)
    
    foreach ($partFile in $partFiles) {
        $bytes = [System.IO.File]::ReadAllBytes($partFile.FullName)
        $outputStream.Write($bytes, 0, $bytes.Length)
        Write-Output "Merged $($partFile.FullName), Size: $(($bytes.Length/1GB).ToString("F2")) GB"
    }
    
    $outputStream.Close()
    Write-Output "Successfully merged into $outputFile"
}

# Example usage
# Merge-Zip -outputFile "merged.zip" -inputDir "split_parts"

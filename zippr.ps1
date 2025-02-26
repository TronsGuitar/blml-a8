$inputFile = "sample.zip"
$outputDir = "split_parts"
$chunkSize = 3GB  # 3 gigabytes

if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$baseName = [System.IO.Path]::GetFileNameWithoutExtension($inputFile)
$inputStream = [System.IO.File]::OpenRead($inputFile)
$buffer = New-Object byte[] $chunkSize
$part = 0

while (($bytesRead = $inputStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
    $partFile = "$outputDir\$baseName.part$part"
    $outputStream = [System.IO.File]::OpenWrite($partFile)
    $outputStream.Write($buffer, 0, $bytesRead)
    $outputStream.Close()
    Write-Output "Created $partFile, Size: $(($bytesRead / 1GB).ToString('F2')) GB"
    $part++
}

$inputStream.Close()


function Merge-Zip {
    param (
        [string]$outputFile,
        [string]$inputDir
    )

    $partFiles = Get-ChildItem -Path $inputDir -Filter "*.part*" | Sort-Object Name
    $outputStream = [System.IO.File]::OpenWrite($outputFile)

    foreach ($partFile in $partFiles) {
        $inputStream = [System.IO.File]::OpenRead($partFile.FullName)
        $buffer = New-Object byte[] 1048576  # 1MB buffer size for efficiency

        while (($bytesRead = $inputStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $outputStream.Write($buffer, 0, $bytesRead)
        }
        
        $inputStream.Close()
        Write-Output "Merged $($partFile.FullName), Size: $(($partFile.Length / 1GB).ToString('F2')) GB"
    }

    $outputStream.Close()
    Write-Output "Successfully merged into $outputFile"
}

# Example usage:
# Merge-Zip -outputFile "merged.zip" -inputDir "split_parts"

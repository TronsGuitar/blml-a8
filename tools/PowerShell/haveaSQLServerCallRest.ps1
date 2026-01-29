# Set up Nexus API credentials
$NexusApiUrl = "https://nexus.example.com/service/rest/v1/components"
$ApiToken = "your-api-token"  # Replace with your actual Nexus API Token
$RepositoryName = "repository-name"  # Replace with your repository name

# Set up the output file location
$OutputPath = "C:\path\to\your\output\sbom.json"  # Adjust path and file name as needed

# Define the HTTP request headers
$headers = @{
    "Authorization" = "Bearer $ApiToken"
    "Accept" = "application/json"
}

# Create the URI for the SBOM request
$SbomUri = "$NexusApiUrl/$RepositoryName/sbom"

try {
    # Make the API request to get the SBOM
    $response = Invoke-RestMethod -Uri $SbomUri -Headers $headers -Method Get

    # Check if the response has content and save it to the specified output file
    if ($response -ne $null) {
        $response | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutputPath -Encoding utf8
        Write-Output "SBOM file downloaded successfully and saved to $OutputPath."
    } else {
        Write-Output "No content found in response. Check the API or repository settings."
    }
} catch {
    Write-Output "Error encountered: $_"
}


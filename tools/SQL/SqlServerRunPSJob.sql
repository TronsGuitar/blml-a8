USE msdb;
GO

-- 1. Drop the job if it already exists
IF EXISTS (SELECT * FROM msdb.dbo.sysjobs WHERE name = 'Download and Insert SBOM')
BEGIN
    EXEC sp_delete_job @job_name = 'Download and Insert SBOM';
END
GO

-- 2. Create the job
EXEC sp_add_job 
    @job_name = 'Download and Insert SBOM',
    @enabled = 1, 
    @description = 'Fetches SBOM from Nexus API and inserts into a database table';

-- 3. Add a job step to execute the PowerShell script
EXEC sp_add_jobstep
    @job_name = 'Download and Insert SBOM',
    @step_name = 'Fetch SBOM and Insert',
    @subsystem = 'PowerShell',
    @command = N'
# PowerShell script to fetch SBOM and insert into database
$NexusApiUrl = "https://nexus.example.com/service/rest/v1/components"
$ApiToken = "your-api-token"
$RepositoryName = "repository-name"

$SqlServer = "YourSqlServerName"
$Database = "YourDatabaseName"
$TableName = "YourTableName"

$headers = @{
    "Authorization" = "Bearer $ApiToken"
    "Accept" = "application/json"
}

$SbomUri = "$NexusApiUrl/$RepositoryName/sbom"

try {
    $response = Invoke-RestMethod -Uri $SbomUri -Headers $headers -Method Get
    if ($response -ne $null) {
        $sbomJson = $response | ConvertTo-Json -Depth 10
        $repositoryId = $RepositoryName

        $connectionString = "Server=$SqlServer;Database=$Database;Integrated Security=True;"
        $query = @"
        INSERT INTO $TableName (RepositoryID, SBOMData)
        VALUES (''$repositoryId'', ''$sbomJson'')
"@

        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $connection.Open()
        $command.ExecuteNonQuery()
        $connection.Close()

        Write-Output "SBOM data inserted into database successfully."
    } else {
        Write-Output "No content found in response."
    }
} catch {
    Write-Output "Error encountered: $_"
}',
    @on_success_action = 1,
    @on_fail_action = 2; -- Go to the next step on success; quit on failure

-- 4. Schedule the job (optional - replace with desired schedule)
EXEC sp_add_jobschedule
    @job_name = 'Download and Insert SBOM',
    @name = 'Daily at 1AM', 
    @freq_type = 4,  -- Daily
    @freq_interval = 1, 
    @active_start_time = 010000;  -- Start at 1:00 AM

-- 5. Set notifications (optional)
EXEC sp_add_notification 
    @job_name = 'Download and Insert SBOM',
    @level = 2,  -- Notify on failure
    @operator_name = 'DBA_Operator';  -- Make sure this operator exists in SQL Agent

-- 6. Start the job manually (optional)
-- EXEC sp_start_job @job_name = 'Download and Insert SBOM';
GO

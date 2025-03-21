param(
    [string] [Parameter(Mandatory=$true)] $searchServiceName,
    [string] [Parameter(Mandatory=$true)] $indexName,
    [string] [Parameter(Mandatory=$true)] $indexFilename,
    [string] [Parameter(Mandatory=$true)] $indexerName,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $indexerScheduleInterval,
    [string] [Parameter(Mandatory=$true)] $dataSourceName,
    [string] [Parameter(Mandatory=$true)] $dataSourceType,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceConnectionString,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceContainerName,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceContainerQuery,
    [string] $indexerDisabled
)

# Change the ErrorActionPreference to 'Stop'
$ErrorActionPreference = 'Stop'

$apiVersion = '2024-07-01'
$token = ConvertFrom-SecureString (Get-AzAccessToken -ResourceUrl https://search.azure.com -AsSecureString).Token -AsPlainText
$headers = @{ 'Authorization' = "Bearer $token"; 'Content-Type' = 'application/json'; }
$uri = "https://$searchServiceName.search.windows.net"
$dataSourceDefinition = $null
$indexDefinition = $null
$indexerDefinition = $null
$DeploymentScriptOutputs = @{}

# Create data source, indexer definitions
switch ($dataSourceType)
{
    "azureblob" {
        $indexDefinition = @{
            'name' = $indexName;
            'fields' = @(
                @{ 'name' = 'rid'; 'type' = 'Edm.String'; 'key' = $true },
                @{ 'name' = 'description'; 'type' = 'Edm.String'; 'retrievable' = $true; 'searchable' = $true }
            );
        }
        $dataSourceDefinition = @{
            'name' = $dataSourceName;
            'type' = $dataSourceType;
            'container' = @{
                'name' = $dataSourceContainerName;
                'query' = $dataSourceContainerQuery;
            };
            'credentials' = @{
                'connectionString' = $dataSourceConnectionString;
            };
            'dataDeletionDetectionPolicy' = @{
                '@odata.type' = '#Microsoft.Azure.Search.NativeBlobSoftDeleteDeletionDetectionPolicy';
            };
        }
        $indexerDefinition = @{
            'name' = $indexerName;
            'disabled' = $indexerDisabled -eq 'true';
            'targetIndexName' = $indexName;
            'dataSourceName' = $dataSourceName;
            'schedule' = $indexerScheduleInterval.Length -gt 0 ? @{ 'interval' = $indexerScheduleInterval } : $null;
        }
        $DeploymentScriptOutputs['text'] = $indexName
    }
    default {
        throw "Unsupported data source type $dataSourceType"
    }
}

try {
    # https://learn.microsoft.com/rest/api/searchservice/create-index
    Invoke-WebRequest `
        -Method 'PUT' `
        -Uri "$uri/indexes/$($indexDefinition['name'])?api-version=$apiVersion" `
        -Headers  $headers `
        -Body (ConvertTo-Json $indexDefinition)

    if ($dataSourceContainerName.Length -gt 0 -and $dataSourceConnectionString.Length -gt 0)
    {
        # https://learn.microsoft.com/rest/api/searchservice/create-data-source
        Invoke-WebRequest `
            -Method 'PUT' `
            -Uri "$uri/datasources/$($dataSourceDefinition['name'])?api-version=$apiVersion" `
            -Headers $headers `
            -Body (ConvertTo-Json $dataSourceDefinition)

        # https://learn.microsoft.com/rest/api/searchservice/create-indexer
        Invoke-WebRequest `
            -Method 'PUT' `
            -Uri "$uri/indexers/$($indexerDefinition['name'])?api-version=$apiVersion" `
            -Headers $headers `
            -Body (ConvertTo-Json $indexerDefinition)
    }
} catch {
    Write-Error $_.ErrorDetails.Message
    throw
}
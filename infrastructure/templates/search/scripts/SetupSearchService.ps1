<#
.SYNOPSIS
Configures Azure AI Search index, indexer and data source resources using the Azure AI Search Management REST API.

.DESCRIPTION
Reads the JSON definition of an index from file, injects CORS settings into it, builds indexer and data source definitions 
based on the provided parameters, and sends REST API requests to create or update the Azure AI Search service resources.
#>
param(
    [string[]] [Parameter(Mandatory=$true)] $indexCorsAllowedOrigins,
    [string] [Parameter(Mandatory=$true)] $indexDefinitionFilePath,
    [string] [Parameter(Mandatory=$true)] $indexerName,
    [string] [Parameter(Mandatory=$false)] [AllowEmptyString()] $indexerOutputFieldMappingsFilePath = '',
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $indexerParsingMode,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $indexerScheduleInterval,
    [string] [Parameter(Mandatory=$true)] $dataSourceName,
    [string] [Parameter(Mandatory=$true)] $dataSourceType,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceConnectionString,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceContainerName,
    [string] [Parameter(Mandatory=$true)] [AllowEmptyString()] $dataSourceContainerQuery,
    [string] [Parameter(Mandatory=$true)] $searchServiceEndpoint,
    [switch] $indexerDisabled
)

# Change the ErrorActionPreference to 'Stop'
$ErrorActionPreference = 'Stop'

Write-Host "Running script from working directory: $(Get-Location)"

# Read the index definition from file
$indexDefinition = Get-Content -Path $indexDefinitionFilePath -Raw | ConvertFrom-Json

# Add CORS options to the index definition
if ($indexDefinition.PSObject.Properties.Name -contains 'corsOptions') {
    Write-Error "CORS options were found in the index definition file. Expecting CORS options to only be set by this script."
    throw "CORS options were found in the index definition file. Expecting CORS options to only be set by this script."
}
Write-Host "Adding $($indexCorsAllowedOrigins.Length) CORS allowed origins to the index definition."
$indexDefinition | Add-Member -MemberType NoteProperty -Name 'corsOptions' -Value @{
    'allowedOrigins' = $indexCorsAllowedOrigins
    'maxAgeInSeconds' = 300 # The duration in seconds that browsers should cache CORS preflight responses for.
}

# Create the data source and indexer definitions
$dataSourceDefinition = $null
$indexerDefinition = $null
switch ($dataSourceType)
{
    "azureblob" {
        $dataSourceDefinition = @{
            'name' = $dataSourceName
            'type' = $dataSourceType
            'container' = @{
                'name' = $dataSourceContainerName
                'query' = $dataSourceContainerQuery
            }
            'credentials' = @{
                'connectionString' = $dataSourceConnectionString
            }
            'dataDeletionDetectionPolicy' = @{
                '@odata.type' = '#Microsoft.Azure.Search.NativeBlobSoftDeleteDeletionDetectionPolicy'
            }
        }

        $indexerParameters = $null
        if (-not [string]::IsNullOrWhiteSpace($indexerParsingMode)) {
            $indexerParameters = @{
                'configuration' = @{
                    'parsingMode' = $indexerParsingMode
                }
            }
        }

        $indexerSchedule = $null
        if (-not [string]::IsNullOrWhiteSpace($indexerScheduleInterval)) {
            $indexerSchedule = @{
                'interval' = $indexerScheduleInterval
                'startTime' = '2025-01-01T00:00:00Z'
            }
        }

        $indexerOutputFieldMappings = @()
        if (-not [string]::IsNullOrWhiteSpace($indexerOutputFieldMappingsFilePath)) {
            $indexerOutputFieldMappingsConfig = Get-Content -Path $indexerOutputFieldMappingsFilePath -Raw | ConvertFrom-Json

            if ($indexerOutputFieldMappingsConfig -is [System.Array]) {
                $indexerOutputFieldMappings = @($indexerOutputFieldMappingsConfig)
            }
            else {
                throw "Output field mappings config must be a JSON array. File: $indexerOutputFieldMappingsFilePath"
            }
        }

        $indexerDefinition = @{
            'name' = $indexerName
            'disabled' = $indexerDisabled.IsPresent
            'targetIndexName' = $indexDefinition.name
            'dataSourceName' = $dataSourceName
            'parameters' = $indexerParameters
            'schedule' = $indexerSchedule
            'outputFieldMappings' = $indexerOutputFieldMappings
        }
    }
    default {
        throw "Unsupported data source type $dataSourceType"
    }
}

# Create/update the index, data source, and indexer using the Azure AI Search REST API
$apiVersion = '2024-07-01'
$token = (ConvertFrom-SecureString (Get-AzAccessToken -ResourceUrl https://search.azure.com -AsSecureString).Token -AsPlainText)
$headers = @{ 'Authorization' = "Bearer $token"; 'Content-Type' = 'application/json'; }
try {
    # https://learn.microsoft.com/rest/api/searchservice/create-index
    # Note: This will fail to update an index if attempting to remove fields, as they are locked for the index's lifetime
    $indexResponse = Invoke-WebRequest `
        -Method 'PUT' `
        -Uri "$searchServiceEndpoint/indexes/$($indexDefinition.name)?api-version=$apiVersion" `
        -Headers  $headers `
        -Body (ConvertTo-Json -Compress -Depth 100 $indexDefinition)
    Write-Host "Create/update request for '$($indexDefinition.name)' succeeded with status code: $($indexResponse.StatusCode)."

    if (-not [string]::IsNullOrWhiteSpace($dataSourceContainerName) -and -not [string]::IsNullOrWhiteSpace($dataSourceConnectionString))
    {
        # https://learn.microsoft.com/rest/api/searchservice/create-data-source
        $dataSourceResponse = Invoke-WebRequest `
            -Method 'PUT' `
            -Uri "$searchServiceEndpoint/datasources/$($dataSourceDefinition.name)?api-version=$apiVersion" `
            -Headers $headers `
            -Body (ConvertTo-Json -Compress -Depth 100 $dataSourceDefinition)
        Write-Host "Create/update request for '$($dataSourceDefinition.name)' succeeded with status code: $($dataSourceResponse.StatusCode)."

        # https://learn.microsoft.com/rest/api/searchservice/create-indexer
        $indexerResponse = Invoke-WebRequest `
            -Method 'PUT' `
            -Uri "$searchServiceEndpoint/indexers/$($indexerDefinition.name)?api-version=$apiVersion" `
            -Headers $headers `
            -Body (ConvertTo-Json -Compress -Depth 100 $indexerDefinition)
        Write-Host "Create/update request for '$($indexerDefinition.name)' succeeded with status code: $($indexerResponse.StatusCode)."
    }
    else {
        Write-Host "No data source name or connection string provided, skipping data source and indexer requests."
    }
} catch {
    Write-Error $_.ErrorDetails.Message
    throw
}
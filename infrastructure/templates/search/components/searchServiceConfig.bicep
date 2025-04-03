@description('Specifies the Search Service name.')
param searchServiceName string

@description('Specifies the name of the data source to create/update.')
param dataSourceName string

@description('Specifies the connection string to use to connect to the data source. The format of the connection string depends on the data source type.')
param dataSourceConnectionString string = ''

@description('Specifies the name of the table, view, collection, or blob container containing data to index.')
param dataSourceContainerName string = ''

@description('Specifies a query to use to filter the data source.')
param dataSourceContainerQuery string = ''

@description('Specifies the data source type to use for the Search Service. Must be one of the supported data source types.')
@allowed([
  'azuresql'
  'cosmosdb'
  'azureblob'
  'adlsgen2'
  'azuretable'
  'azurefile'
  'mysql'
  'sharepoint'
])
param dataSourceType string

@description('Specifies the URI of a file containing the JSON definition of the index to create/update.')
param indexDefinitionUri string

@description('Specifies the name of the indexer to create/update.')
param indexerName string

@description('Specifies whether the indexer is disabled. Set this property if you want to create the indexer definition without immediately running it.')
param indexerDisabled bool = false

@description('Specifies the interval at which the indexer should run. The format is an \'XSD\' xs:dayTimeDuration e.g. \'PT2H\' for every 2 hours. Optional, but runs once immediately if unspecified and not disabled.')
param indexerScheduleInterval string = ''

@description('Specifies the location for all resources.')
param location string

resource searchService 'Microsoft.Search/searchServices@2022-09-01' existing = {
  name: searchServiceName
}

// A user-assigned managed identity to give the deployment script the required access to complete operations in the script
resource scriptIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: '${searchService.name}-script-identity'
  location: location
}

module scriptIdentityRoleAssignmentModule 'searchServiceRoleAssignment.bicep' = {
  name: 'scriptIdentityRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: searchService.name
    principalIds: [scriptIdentity.properties.principalId]
    role: 'Search Service Contributor'
  }
}

var indexDefinitionFilename = substring(indexDefinitionUri, lastIndexOf(indexDefinitionUri, '/') + 1)

resource deploymentScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: '${searchService.name}-deployment-script'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${scriptIdentity.id}': {}
    }
  }
  properties: {
    azPowerShellVersion: '13.2'
    arguments: '-searchServiceName \\"${searchService.name}\\" -indexDefinitionFilename \\"${indexDefinitionFilename}\\" -indexerName \\"${indexerName}\\" -indexerDisabled \\"${indexerDisabled}\\" -indexerScheduleInterval \\"${indexerScheduleInterval}\\" -dataSourceName \\"${dataSourceName}\\" -dataSourceType \\"${dataSourceType}\\" -dataSourceConnectionString \\"${dataSourceConnectionString}\\" -dataSourceContainerName \\"${dataSourceContainerName}\\" -dataSourceContainerQuery \\"${dataSourceContainerQuery}\\"'
    scriptContent: loadTextContent('../scripts/SetupSearchService.ps1')
    supportingScriptUris: [
      indexDefinitionUri
    ]
    cleanupPreference: 'OnExpiration'
    retentionInterval: 'PT1H'
    timeout: 'PT5M'
  }
  dependsOn: [scriptIdentityRoleAssignmentModule]
}

output indexName string = deploymentScript.properties.outputs.indexName

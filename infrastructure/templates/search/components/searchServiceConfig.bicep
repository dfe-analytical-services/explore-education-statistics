@description('Specifies the Search Service name.')
param searchServiceName string

@description('Specifies the location for all resources.')
param location string

resource searchService 'Microsoft.Search/searchServices@2022-09-01' existing = {
  name: searchServiceName
}

// A user-assigned managed identity to give the deployment script the required access to complete operations in the script
resource scriptIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${searchService.name}-script-identity'
  location: location
}

// The built-in Search Service Contributor role. See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#search-service-contributor')
resource searchServiceContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
}

// Assign the Search Service Contributor role to the deployment script identity
// resource indexContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   scope: searchService
//   name: guid(searchService.id, scriptIdentity.id, searchServiceContributorRoleDefinition.id)
//   properties: {
//     roleDefinitionId: searchServiceContributorRoleDefinition.id
//     principalId: scriptIdentity.properties.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

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
    azPowerShellVersion: '13.3.0'
    arguments: searchServiceName
    scriptContent: loadTextContent('../scripts/SetupSearchService.ps1')
    cleanupPreference: 'OnExpiration'
    retentionInterval: 'PT1H'
    timeout: 'PT1M'
  }
}

output text string = deploymentScript.properties.outputs.text

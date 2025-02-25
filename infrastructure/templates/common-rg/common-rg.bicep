@description('Provide a tier of your Azure Container Registry (Basic/Classic/Premium/Standard)')
param acrSku string = 'Basic'

var acrName = 'eesacr'
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource acrUai 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  location: resourceGroup().location
  name: '${acrName}-mi'
}

resource acrUaiRbac 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, acrUai.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: acrUai.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource acrResource 'Microsoft.ContainerRegistry/registries@2024-11-01-preview' = {
  name: acrName
  location: resourceGroup().location
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: false
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrUai.id}': {}
    }
  }
}

output subscription object = subscription()
output resourceGroupId string = resourceGroup().id
output acrResourceId string = acrResource.id
output acrUaiResourceId string = acrUai.id

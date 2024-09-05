@description('Specifies the name of the Container Registry')
param containerRegistryName string

@description('Specifies the location for all resources.')
param location string

@allowed([
  'Basic'
  'Standard'
  'Premium'
])
@description('Tier of your Azure Container Registry.')
param skuName string = 'Basic'

@description('Deploy a new Container Registry or use the existing registry')
param deployRegistry bool

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = if (deployRegistry) {
  name: containerRegistryName
  location: location
  sku: {
    name: skuName
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    policies: {
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
    }
  }
  tags: tagValues
}

resource currentContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

output containerRegistryId string = currentContainerRegistry.id
output containerRegistryName string = currentContainerRegistry.name
output containerRegistryLoginServer string = currentContainerRegistry.properties.loginServer

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@minLength(5)
@maxLength(50)
@description('Name of the azure container registry (must be globally unique)')
param containerRegistryName string

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

var registryName = replace('${resourcePrefix}cr${containerRegistryName}', '-', '')

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = if (deployRegistry) {
  name: registryName
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
  name: registryName
}

output containerRegistryId string = currentContainerRegistry.id
output containerRegistryName string = currentContainerRegistry.name
output containerRegistryLoginServer string = currentContainerRegistry.properties.loginServer

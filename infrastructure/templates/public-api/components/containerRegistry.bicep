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

//Passed in Tags
param tagValues object

//Variables
var RegistryName = replace('${resourcePrefix}cr${containerRegistryName}', '-', '')


//Resources 
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = if (deployRegistry) {
  name: RegistryName
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


resource currentContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: RegistryName
}


// Outputs for exported use
output containerRegistryId string = currentContainerRegistry.id
output containerRegistryName string = currentContainerRegistry.name
output containerRegistryLoginServer string = currentContainerRegistry.properties.loginServer

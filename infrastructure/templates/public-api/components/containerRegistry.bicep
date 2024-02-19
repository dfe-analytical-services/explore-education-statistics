@minLength(5)
@maxLength(50)
@description('Name of the azure container registry (must be globally unique)')
param containerRegistryName string

//Resources
resource currentContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: containerRegistryName
}

// Outputs for exported use
output containerRegistryId string = currentContainerRegistry.id
output containerRegistryName string = currentContainerRegistry.name
output containerRegistryLoginServer string = currentContainerRegistry.properties.loginServer

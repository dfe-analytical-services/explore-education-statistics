//Resources
resource currentContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: registryName
}


// Outputs for exported use
output containerRegistryId string = currentContainerRegistry.id
output containerRegistryName string = currentContainerRegistry.name
output containerRegistryLoginServer string = currentContainerRegistry.properties.loginServer

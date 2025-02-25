@description('Provide a globally unique name of your Azure Container Registry. Alphanumeric only')
param acrName string = 'eesacr'

@description('Provide a location for the registry.')
param location string = resourceGroup().location

@description('Provide a tier of your Azure Container Registry.')
param acrSku string = 'Basic'

resource acrManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  location: location
  name: 'acrManagedIdentity'
}

resource acrResource 'Microsoft.ContainerRegistry/registries@2024-11-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: false
  }
}

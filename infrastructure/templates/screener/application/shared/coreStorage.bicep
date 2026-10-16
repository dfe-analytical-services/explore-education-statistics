import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

resource coreStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.existingResources.coreStorageAccount
  scope: resourceGroup(resourceGroup().name)
}

output coreStorageBlobEndpoint string = coreStorageAccount.properties.primaryEndpoints.blob

import { ResourceNames } from '../../../common/resource-names.bicep'

@description('Names of the storage accounts.')
param names ResourceNames.storageAccounts

resource coreStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: names.core
  scope: resourceGroup(resourceGroup().name)
}

output coreStorageBlobEndpoint string = coreStorageAccount.properties.primaryEndpoints.blob

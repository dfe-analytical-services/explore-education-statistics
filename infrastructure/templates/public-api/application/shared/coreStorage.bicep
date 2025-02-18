import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

resource coreStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.existingResources.coreStorageAccount
  scope: resourceGroup(resourceGroup().name)
}

var coreStorageAccountKey = coreStorageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var coreStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${coreStorageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${coreStorageAccountKey}'

var coreStorageConnectionStringSecretKey = 'ees-core-storage-connectionstring'

module storeCoreStorageConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeCoreStorageConnectionString'
  params: {
    keyVaultName: resourceNames.existingResources.keyVault
    secretName: coreStorageConnectionStringSecretKey
    secretValue: coreStorageConnectionString
  }
}

output coreStorageConnectionStringSecretKey string = coreStorageConnectionStringSecretKey

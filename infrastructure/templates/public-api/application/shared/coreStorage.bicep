@description('Specifies the legacy resource prefix')
param legacyResourcePrefix string

@description('Specifies the name of the Key Vault.')
param keyVaultName string

var coreStorageAccountName = '${legacyResourcePrefix}saeescore'

resource coreStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: coreStorageAccountName
  scope: resourceGroup(resourceGroup().name)
}

var coreStorageAccountKey = coreStorageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var coreStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${coreStorageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${coreStorageAccountKey}'

var coreStorageConnectionStringSecretKey = 'ees-core-storage-connectionstring'

module storeCoreStorageConnectionString '../../components/keyVaultSecret.bicep' = {
  name: 'storeCoreStorageConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: coreStorageConnectionStringSecretKey
    secretValue: coreStorageConnectionString
    contentType: 'text/plain'
  }
}

output coreStorageConnectionStringSecretKey string = coreStorageConnectionStringSecretKey


//Specific parameters for the resources
@description('Storage : Name of the blob store')
param blobStoreName string = 'default'

@description('Amount of days the soft deleted data is stored and available for recovery')
@minValue(1)
@maxValue(365)
param deleteRetentionPolicy int = 7

@description('Storage : Name of the Storage Account')
param storageAccountName string

// Variables and created data


//Resources

// Existsing Storage Account
resource StorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Blob Services for Storage Account
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2019-06-01' = {
  name: blobStoreName
  parent: StorageAccount
  properties: {
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      enabled: true
      days: deleteRetentionPolicy
    }
  }
}

//Outputs
output blobStoreName string = blobServices.name

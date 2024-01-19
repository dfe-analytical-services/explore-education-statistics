
//Specific parameters for the resources
@description('Name of the blob store')
param blobStoreName string = 'default'

@description('Amount of days the soft deleted data is stored and available for recovery')
@minValue(1)
@maxValue(365)
param deleteRetentionPolicy int = 7

@description(' Name of the Storage Account')
param storageAccountName string

// Variables and created data


//Resources

// Existsing Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

// Blob Services for Storage Account
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: blobStoreName
  parent: storageAccount
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

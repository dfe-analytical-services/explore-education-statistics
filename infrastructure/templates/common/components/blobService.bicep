@description('Name of the blob service')
param blobServiceName string = 'default'

@description('Amount of days the soft deleted data is stored and available for recovery')
@minValue(1)
@maxValue(365)
param deleteRetentionPolicy int = 7

@description('Name of the Storage Account')
param storageAccountName string

@description('Names of containers to create')
param containerNames array = []

// Reference an existing Storage Account.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  name: blobServiceName
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

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for containerName in containerNames: {
  name: containerName
  parent: blobService
}]

output blobServiceName string = blobService.name

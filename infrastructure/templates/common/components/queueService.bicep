@description('Name of the queue service')
param queueServiceName string = 'default'

@description('Name of the Storage Account')
param storageAccountName string

@description('Names of queues to create')
param queueNames array = []

// Reference an existing Storage Account.
resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {
  name: storageAccountName
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2024-01-01' = {
  parent: storageAccount
  name: queueServiceName
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource queues 'Microsoft.Storage/storageAccounts/queueServices/queues@2024-01-01' = [for queueName in queueNames: {
  parent: queueService
  name: queueName
  properties: {
    metadata: {}
  }
}]

output queueServiceName string = queueService.name

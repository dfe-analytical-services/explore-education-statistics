@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Name of the Service Bus namespace')
param namespaceName string

@description('Name of the Queue')
param queueName string

@description('The name of the Key Vault to store the connection strings')
param keyVaultName string

//Passed in Tags
param tagValues object


// Variables and created data
var serviceBusNamespaceName = '${resourcePrefix}-sbns-${namespaceName}'
var serviceBusQueueName = '${resourcePrefix}-sbq-${queueName}'
var serviceBusEndpoint = '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBusNamespace.apiVersion).primaryConnectionString
var connectionStringSecretName = '${resourcePrefix}-sbq-${queueName}-connectionString'


//Resources 

//ServiceBus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {}
  tags: tagValues
}

//ServiceBus Queue
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: serviceBusQueueName
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S' //Max time possible to stop messages being auto-deleted
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S' //Max time possible to stop messages being auto-deleted
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 10
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
  }
}

//store connections string
module storeADOConnectionStringToKeyVault './keyVaultSecret.bicep' = {
  name: 'sbqConnectionStringSecretDeploy'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretValue: serviceBusConnectionString 
    contentType: 'text/plain'
    secretName: connectionStringSecretName
  }
}


//Outputs
output serviceBusQueueRef string = serviceBusQueue.id
output serviceBusQueueName string = serviceBusQueue.name
output connectionStringSecretName string = connectionStringSecretName

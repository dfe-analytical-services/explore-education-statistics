@description('Specifies the Subscription to be used.')
param subscription string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Name of the Service Bus namespace')
param namespaceName string = 'etlnamespace'

@description('Name of the Queue')
param queueName string = 'etlfunctionqueue'


//Passed in Tags
param departmentName string = 'Public API'
param environmentName string = 'Development'
param solutionName string = 'API'
param subscriptionName string = 'Unknown'
param costCentre string = 'Unknown'
param serviceOwnerName string = 'Unknown'
param dateProvisioned string = utcNow('u')
param createdBy string = 'Unknown'
param deploymentRepo string = 'N/A'
param deploymentScript string = 'N/A'


// Variables and created data
var serviceBusNamespaceName = '${subscription}-sbns-${namespaceName}'
var serviceBusQueueName = '${subscription}-sbq-${queueName}'
var serviceBusEndpoint = '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBusNamespace.apiVersion).primaryConnectionString


//Resources 

//ServiceBus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {}
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'ServiceBus Namespace'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}

//ServiceBus Queue
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: serviceBusQueueName
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
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


//Outputs
output ServiceBusQueueRef string = serviceBusQueue.id
output ServiceBusQueueName string = serviceBusQueue.name
output ServiceBusConnectionString string = serviceBusConnectionString



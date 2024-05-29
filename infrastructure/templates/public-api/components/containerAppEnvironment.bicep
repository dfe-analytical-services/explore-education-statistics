@description('Specifies the subscription name of the Container App Environment - used for creating name prefix')
param subscription string

@description('Specifies the location of the Container App Environment - defaults to that of the Resource Group')
param location string

@description('Specifies the dedicated subnet created for the Container App Environment within the main VNet')
param subnetId string

@description('Specifies whether or not this Container App Environment is internal-only or will be provisioned with a Public IP address')
param internal bool = true

@description('Specifies the name of the Log Analytics workspace responsible for capturing logging from this resource')
param logAnalyticsWorkspaceName string

@description('Specifies the Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the workload profiles for this Container App Environment - defaults to Consumption')
param workloadProfiles {
 name: string
 workloadProfileType: string
}[] = [{
 name: 'Consumption'
 workloadProfileType: 'Consumption'
}]

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

@description('Specifies a suffix to append to the full name of the Container App Environment')
param containerAppEnvironmentNameSuffix string = ''

@description('Specifies an array of Azure File Shares to be available for Container Apps hosted within this Container App Environment')
param azureFileStorages {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  accessMode: 'ReadWrite' | 'ReadOnly'
}[] = []

var containerAppEnvironmentName = empty(containerAppEnvironmentNameSuffix)
  ? '${subscription}-ees-cae'
  : '${subscription}-ees-cae-${containerAppEnvironmentNameSuffix}'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppEnvironmentName
  location: location
  properties: {
    vnetConfiguration: {
      infrastructureSubnetId: subnetId
      internal: internal
    }
    daprAIInstrumentationKey: applicationInsightsKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    workloadProfiles: workloadProfiles
  }
  tags: tagValues
  
  resource azureFileStorage 'storages@2022-03-01' = [for storage in azureFileStorages: {
      name: storage.storageName
      properties: {
        azureFile: {
          accountKey: storage.storageAccountKey
          accountName: storage.storageAccountName
          shareName: storage.fileShareName
          accessMode: storage.accessMode
        }
      }
  }]
}

output containerAppEnvironmentName string = containerAppEnvironmentName
output containerAppEnvironmentId string = containerAppEnvironment.id

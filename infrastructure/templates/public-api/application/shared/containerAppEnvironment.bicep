@description('Specifies the location for all resources.')
param location string

@description('Specifies the Container App Environment name.')
param containerAppEnvironmentName string = ''

@description('Specifies the subnet id to connect this resource to the VNet.')
param subnetId string = ''

@description('Specifies the name of the Log Analytics workspace responsible for capturing logging from this resource.')
param logAnalyticsWorkspaceName string

@description('Specifies the Application Insights key that is associated with this resource.')
param applicationInsightsKey string

@description('Specifies the name of the Public API storage account.')
param publicApiStorageAccountName string

@description('Specifies the fileshare name of the Public API data.')
param publicApiFileShareName string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: publicApiStorageAccountName
}

module containerAppEnvironmentModule '../../components/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    location: location
    containerAppEnvironmentName: containerAppEnvironmentName
    subnetId: subnetId
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    applicationInsightsKey: applicationInsightsKey
    tagValues: tagValues
    azureFileStorages: [
      {
        storageName: publicApiFileShareName
        storageAccountName: publicApiStorageAccountName
        storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
        fileShareName: publicApiFileShareName
        accessMode: 'ReadWrite'
      }
    ]
  }
}

output containerAppEnvironmentId string = containerAppEnvironmentModule.outputs.containerAppEnvironmentId

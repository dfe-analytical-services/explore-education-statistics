import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Application Insights key that is associated with this resource.')
param applicationInsightsKey string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.publicApi.publicApiStorageAccount
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.containerAppEnvironment
  parent: vNet
}

module containerAppEnvironmentModule '../../components/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    location: location
    containerAppEnvironmentName: resourceNames.sharedResources.containerAppEnvironment
    subnetId: subnet.id
    logAnalyticsWorkspaceName: resourceNames.sharedResources.logAnalyticsWorkspace
    applicationInsightsKey: applicationInsightsKey
    tagValues: tagValues
    azureFileStorages: [
      {
        storageName: resourceNames.publicApi.publicApiFileshare
        storageAccountName: resourceNames.publicApi.publicApiStorageAccount
        storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
        fileShareName: resourceNames.publicApi.publicApiFileshare
        accessMode: 'ReadWrite'
      }
    ]
  }
}

output containerAppEnvironmentId string = containerAppEnvironmentModule.outputs.containerAppEnvironmentId
output containerAppEnvironmentIpAddress string = containerAppEnvironmentModule.outputs.containerAppEnvironmentIpAddress
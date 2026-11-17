import { ResourceNames } from '../../../common/resource-names.bicep'
import { ContainerAppWorkloadProfile } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Application Insights key that is associated with this resource.')
param applicationInsightsKey string

@description('Specifies the workload profiles for this Container App Environment - the default Consumption plan is always included')
param workloadProfiles ContainerAppWorkloadProfile[] = []

@description('Whether to spread the Container App Environment across availability zones.')
param zoneRedundant bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.analytics.storage.storageAccountName
}

resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.publicApi.storage.storageAccount
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.containerAppEnvironment
  parent: vNet
}

module containerAppEnvironmentModule '../../components/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    location: location
    containerAppEnvironmentName: resourceNames.containerAppEnvironment
    subnetId: subnet.id
    logAnalyticsWorkspaceName: resourceNames.logAnalyticsWorkspace
    applicationInsightsKey: applicationInsightsKey
    azureFileStorages: [
      {
        storageName: resourceNames.analytics.storage.fileShareName
        storageAccountName: resourceNames.analytics.storage.storageAccountName
        storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
        fileShareName: resourceNames.analytics.storage.fileShareName
        accessMode: 'ReadWrite'
      }
      {
        storageName: resourceNames.publicApi.storage.fileShare
        storageAccountName: resourceNames.publicApi.storage.storageAccount
        storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
        fileShareName: resourceNames.publicApi.storage.fileShare
        accessMode: 'ReadWrite'
      }
    ]
    workloadProfiles: workloadProfiles
    zoneRedundant: zoneRedundant
    tagValues: tagValues
  }
}

output containerAppEnvironmentId string = containerAppEnvironmentModule.outputs.containerAppEnvironmentId
output containerAppEnvironmentIpAddress string = containerAppEnvironmentModule.outputs.containerAppEnvironmentIpAddress

import { ResourceNames, ContainerAppWorkloadProfile } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Application Insights key that is associated with this resource.')
param applicationInsightsKey string

@description('Specifies the workload profiles for this Container App Environment - the default Consumption plan is always included')
param workloadProfiles ContainerAppWorkloadProfile[] = []

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: resourceNames.existingResources.analyticsStorageAccount
}

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
    azureFileStorages: [
      {
        storageName: resourceNames.existingResources.analyticsFileShare
        storageAccountName: resourceNames.existingResources.analyticsStorageAccount
        storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
        fileShareName: resourceNames.existingResources.analyticsFileShare
        accessMode: 'ReadWrite'
      }
      {
        storageName: resourceNames.publicApi.publicApiFileShare
        storageAccountName: resourceNames.publicApi.publicApiStorageAccount
        storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
        fileShareName: resourceNames.publicApi.publicApiFileShare
        accessMode: 'ReadWrite'
      }
    ]
    workloadProfiles: workloadProfiles
    tagValues: tagValues
  }
}

output containerAppEnvironmentId string = containerAppEnvironmentModule.outputs.containerAppEnvironmentId
output containerAppEnvironmentIpAddress string = containerAppEnvironmentModule.outputs.containerAppEnvironmentIpAddress

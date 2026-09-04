import { AzureFileShareMount } from '../storage/types.bicep'

@description('Name of the App Service that owns the swap slot.')
param appServiceName string

@description('Id of the App Service Plan that the owning App Service belongs to.')
param appServicePlanId string

@description('Name of the swap slot.')
param slotName string

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('Name of the VNet.')
param vnetLink {
  vnetName: string
  subnetName: string
}?

@description('File Shares to mount on this App Service slot.')
param azureFileShares AzureFileShareMount[]?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource stagingSlot 'Microsoft.Web/sites/slots@2025-03-01' = {
  name: '${appServiceName}/${slotName}'
  kind: 'app'
  location: resourceGroup().location
  tags: tagValues
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: 'v10.0'
      alwaysOn: false
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
    }
  }
}

module stagingSlotVNetLink 'slot-virtual-network-link.bicep' = if (vnetLink != null) {
  name: '${appServiceName}${slotName}VnetLinkDeploy'
  params: {
    appServiceName: appServiceName
    slotName: slotName
    vNetName: vnetLink!.vnetName
    subnetName: vnetLink!.subnetName
  }
}

module azureStorageAccountsConfigModule '../storage/file-share-mounts-for-site-slot.bicep' = {
  name: '${appServiceName}${slotName}StorageAccountsConfigDeploy'
  params: {
    siteName: appServiceName
    slotName: slotName
    azureFileShares: azureFileShares
  }
}

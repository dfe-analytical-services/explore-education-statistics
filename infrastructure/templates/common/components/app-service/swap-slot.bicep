import { ConnectionString } from '../../types.bicep'
import { AzureFileShareMount } from '../storage/types.bicep'

@description('Name of the App Service that owns the swap slot.')
param appServiceName string

@description('Id of the App Service Plan that the owning App Service belongs to.')
param appServicePlanId string

@description('The operating system to use to host App Services.')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('The kind of plan to create. Use "app,linux,container" and "Linux" for the "operatingSystem" param for App Services for Docker.')
param kind 'app' | 'app,linux,container' = 'app'

@description('Name of the swap slot.')
param slotName string

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('Path the platform should ping to judge the app healthy.')
param healthCheckPath string?

@description('''
Database connection strings. Connection strings (like appsettings) are NOT slot-specific by
default - they swap along with the deployment content unless explicitly marked as sticky via a
slotConfigNames resource, which this setup does not use. Since these connection strings don't
differ between the production and staging slots anyway, the fix is to configure the same values
on both slots, so a swap has no effect on them, rather than relying on sticky-setting semantics.
''')
param connectionStrings ConnectionString[]?

@description('Name of the VNet.')
param vnetLink {
  vnetName: string
  subnetName: string
}?

@description('File Shares to mount on this App Service slot.')
param azureFileShares AzureFileShareMount[]?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var vnetIntegrationSubnetRef = vnetLink != null 
  ? resourceId('Microsoft.Network/virtualNetworks/subnets', vnetLink!.vnetName, vnetLink!.subnetName)
  : null

resource stagingSlot 'Microsoft.Web/sites/slots@2025-03-01' = {
  name: '${appServiceName}/${slotName}'
  kind: kind
  location: resourceGroup().location
  tags: tagValues
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    reserved: operatingSystem == 'Linux'
    virtualNetworkSubnetId: vnetIntegrationSubnetRef ?? ''
    siteConfig: {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: 'v10.0'
      alwaysOn: true
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
      healthCheckPath: healthCheckPath
      connectionStrings: connectionStrings
    }
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

output slotIdentityPrincipalId string = stagingSlot.identity.principalId

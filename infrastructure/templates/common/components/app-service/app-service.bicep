import { ConnectionString } from 'types.bicep'
import { AzureFileShareMount } from '../storage/types.bicep'

@description('Name of the App Service.')
param appServiceName string

@description('Name of the App Insights instance that this App Service is connected to.')
param appInsightsName string

@description('Name of Key Vault to allow secret access to from this App Service.')
param keyVaultName string?

@description('Whether to use the default role assignment name generation or the legacy name generation scheme.')
param legacyKeyVaultRoleAssignmentName bool = false

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('The owning App Service Plan id.')
param appServicePlanId string

@description('Subnet used to connect the App Service to a VNet, if required.')
param vnetLink {
  vnetName: string
  subnetName: string
}?

@description('Database connection strings.')
param connectionStrings ConnectionString[]?

@description('Application-specific appsettings. These will be merged with infrastructure appsettings.')
param applicationAppSettings object

@description('Whether or not to display detailed error messages in this environment.')
param detailedErrors bool

@description('Whether or not to enable autoscaling in this environment.')
param autoscaleEnabled bool

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]?

@description('File Shares to mount on this App Service and its slots.')
param azureFileShares AzureFileShareMount[]?

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param alerts {
  appServiceHealth: bool
  httpErrors: bool
  alertsGroupName: string
}?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var deploySlotName = 'deploy'

resource appService 'Microsoft.Web/sites@2025-03-01' = {
  name: appServiceName
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  tags: union(tagValues, {
    ServiceType: 'App Service'
  })
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: true
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
      connectionStrings: connectionStrings
      cors: {
        allowedOrigins: allowedOrigins
      }
    }
  }
}

resource appSettings 'Microsoft.Web/sites/config@2025-03-01' = {
  parent: appService
  name: 'appsettings'
  properties: union(applicationAppSettings, {
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(
      resourceId('Microsoft.Insights/components', appInsightsName),
      '2020-02-02'
    ).InstrumentationKey
    AppInsights__InstrumentationKey: reference(
      resourceId('Microsoft.Insights/components', appInsightsName),
      '2020-02-02'
    ).InstrumentationKey
    WEBSITE_NODE_DEFAULT_VERSION: '22.23.1'
    WEBSITE_RUN_FROM_PACKAGE: '1'
    WEBSITE_LOAD_CERTIFICATES: '*'
    ASPNETCORE_DETAILEDERRORS: detailedErrors
  })
}

module appServiceSecretsUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultName != null) {
  name: '${appServiceName}KeyVaultSecretsUserRoleAssignmentModule'
  params: {
    keyVaultName: keyVaultName!
    roleAssignmentNameOverride: legacyKeyVaultRoleAssignmentName 
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6'), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appService.identity.principalId]
    role: 'Secrets User'
  }
}

module vNetLink 'virtual-network-link.bicep' = if (vnetLink != null) {
  name: '${appServiceName}VnetLinkDeploy'
  params: {
    appServiceName: appService.name
    vNetName: vnetLink!.vnetName
    subnetName: vnetLink!.subnetName
  }
}

module stagingSlotModule 'swap-slot.bicep' = {
  name: '${appServiceName}${deploySlotName}Deploy'
  params: {
    appServiceName: appService.name
    slotName: deploySlotName
    appServicePlanId: appServicePlanId
    minTlsVersion: minTlsVersion
    vnetLink: vnetLink
    tagValues: tagValues
  }
}

module autoscaleSettingsModule 'autoscale-settings.bicep' = {
  name: '${appServiceName}AutoscaleSettingsDeploy'
  params: {
    appServiceName: appService.name
    appServicePlanId: appServicePlanId
    autoscaleEnabled: autoscaleEnabled
  }
}

module azureStorageAccountsConfigModule '../storage/file-share-mounts-for-site.bicep' = {
  name: '${appServiceName}StorageAccountsConfigDeploy'
  params: {
    siteName: appServiceName
    azureFileShares: azureFileShares
  }
}

module alertsModule 'alerts.bicep' = if (alerts != null) {
  name: '${appServiceName}AlertsDeploy'
  params: {
    appServiceName: appServiceName
    alerts: alerts!
    tagValues: tagValues
  }  
}

output appServiceName string = appService.name

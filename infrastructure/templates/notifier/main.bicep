import { HttpStatusAlertConfig } from '../common/components/alerts/httpStatusAlertConfig.bicep'
import { StorageAccountSku } from '../common/components/storage/types.bicep'
import { ResourceNames } from '../common/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { IpRange } from '../common/types.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku FunctionAppServicePlanSku

@description('The operating system to host the Function App on.')
param operatingSystem 'Windows' | 'Linux'

@description('SKU of the Function App\'s storage account, which sets its redundancy.')
param storageAccountSku StorageAccountSku

@description('Whether the Function App uses its user-assigned managed identity rather than a system-assigned one.')
param useUserAssignedIdentity bool = false

@description('Whether to create a content share for the Function App, which is used to store its code in Consumption and Elastic plans.')
param deployContentShare bool = true

@description('Whether to store the storage account\'s connection string in Key Vault, for the apps that connect to it with a connection string rather than a managed identity.')
param storeStorageAccountConnectionString bool = true

@description('Whether the storage account accepts its account keys, for the apps that connect to it with a connection string rather than a managed identity.')
param storageAccountSharedKeyAccessEnabled bool = true

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[]

@description('Number of days to retain blobs after delete.')
param blobDeleteRetentionDays int

@description('The origins supported for CORS calls to this Function App.')
param allowedOrigins string[]

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Optional override of the existing Http401 rule. Omit to preserve current behaviour.')
param http401Alert HttpStatusAlertConfig?

@description('Optional override of the existing Http5xx rule. Omit to preserve current behaviour.')
param http5xxAlert HttpStatusAlertConfig?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@secure()
@description('''
The existing appsettings for this Function App, fetched by the pipeline before deployment. Used to
prevent infrastructure deploys from overriding application-specific appsettings that the code
deployment pipeline has since applied - see notifier-appsettings.bicep.
''')
param existingAppSettings object = {}

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}
resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.notifier
  parent: vNet
}

// Publisher's Function App reads and writes to this storage account (via App__NotifierStorageConnectionString),
// so its subnet needs to be allowed through this storage account's firewall too.
resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.publisher
  parent: vNet
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'notifierAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.notifier.appInsights
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      exceptionCount: true
      exceptionServerCount: true
      failedRequests: true
    } : null
    tagValues: tagValues
  }
}

module functionAppModule '../common/components/function-app/function-app.bicep' = {
  name: 'notifierFunctionAppModuleDeploy'
  params: {
    userAssignedIdentityName: useUserAssignedIdentity ? resourceNames.notifier.identity : ''
    functionAppName: resourceNames.notifier.functionApp
    appServicePlanName: resourceNames.notifier.appServicePlan
    storageAccountName: resourceNames.notifier.storageAccount
    storageAccountSku: storageAccountSku
    deployContentShare: deployContentShare
    storageAccountSharedKeyAccessEnabled: storageAccountSharedKeyAccessEnabled
    storageAccountConnectionStringSecretName: storeStorageAccountConnectionString
      ? resourceNames.keyVault.secrets.notifierStorageAccountConnectionString
      : null
    keyVaultName: resourceNames.keyVault.keyVault
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    sku: appServiceSku
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: operatingSystem
    netFrameworkVersion: operatingSystem == 'Windows' ? 'v10.0' : null
    linuxFxVersion: 'DOTNET-ISOLATED|10.0'
    elasticCapacity: null
    alwaysOn: true
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: []
    allowedOrigins: allowedOrigins
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: outboundVnetSubnet.id
    storageAccountAllowedSubnetIds: [publisherSubnet.id]
    storageFirewallRules: maintenanceIpRanges
    storageAccountPublicNetworkAccessEnabled: true
    ipSecurityRestrictionsDefaultAction: 'Allow'
    deployQueueRoleAssignment: true
    // The Notifier keeps its subscriptions in tables.
    // TODO EES-7502 - grant this unconditionally once every environment has moved to the ELZ.
    deployTableRoleAssignment: useUserAssignedIdentity
    minTlsVersion: minTlsVersion
    connectionStrings: [
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=notifier@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
    ]
    alerts: deployAlerts ? {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      http401: http401Alert
      http5xx: http5xxAlert
      memoryPercentage: true
      storageAccountAvailability: false
      storageLatency: false
      fileServiceAvailability: false
      fileServiceLatency: false
      fileServiceCapacity: false
      alertsGroupName: resourceNames.alertsGroup
    } : null
    healthCheckPath: '/api/health'
    diagnosticSettingsLogAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    appSettings: []
    existingAppSettings: existingAppSettings
    tagValues: tagValues
  }
}

module storageAccountBlobServiceModule '../common/components/blobService.bicep' = {
  name: 'notifierStorageAccountBlobServiceModuleDeploy'
  params: {
    storageAccountName: resourceNames.notifier.storageAccount
    deleteRetentionPolicy: blobDeleteRetentionDays
  }
  dependsOn: [
    functionAppModule
  ]
}

// When the release apps use their user-assigned identities, allow the Publisher to send messages to the Notifier's
// queues. Nothing uses this role until the Publisher connects to storage with its identity rather than a connection
// string.
// TODO EES-7502 - make this grant unconditional once every environment has moved to the ELZ.
resource publisherIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (useUserAssignedIdentity) {
  name: resourceNames.publisher.identity
}

module publisherStorageAccountRoleAssignmentModule '../common/components/storageAccountRoleAssignment.bicep' = if (useUserAssignedIdentity) {
  name: 'notifierPublisherStorageAccountRoleAssignmentModuleDeploy'
  params: {
    storageAccountName: functionAppModule.outputs.storageAccountName
    principalIds: [publisherIdentity!.properties.principalId]
    role: 'Storage Queue Data Contributor'
  }
}

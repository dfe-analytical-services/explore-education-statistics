import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { keyVaultRef } from '../common/functions.bicep'
import { IpRange } from '../common/types.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku FunctionAppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[]

@description('Replaces Notify exceptions with logged messages only when team-only API keys are used and a recipient email address is not valid for that key.')
param suppressExceptionsForTeamOnlyApiKeyErrors bool

@description('The public-facing URL of the public site.')
param publicAppUrl string

@description('The origins supported for CORS calls to this Function App.')
param allowedOrigins string[]

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri

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
    functionAppName: resourceNames.notifier.functionApp
    appServicePlanName: resourceNames.notifier.appServicePlan
    storageAccountName: resourceNames.notifier.storageAccount
    keyVaultName: resourceNames.keyVault.keyVault
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    sku: appServiceSku
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: 'Windows'
    netFrameworkVersion: 'v10.0'
    elasticCapacity: null
    alwaysOn: true
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: []
    allowedOrigins: allowedOrigins
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: outboundVnetSubnet.id
    storageAccountAllowedSubnetIds: [publisherSubnet.id]
    storageFirewallRules: maintenanceIpRanges
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
      memoryPercentage: true
      storageAccountAvailability: false
      storageLatency: false
      fileServiceAvailability: false
      fileServiceLatency: false
      fileServiceCapacity: false
      alertsGroupName: resourceNames.alertsGroup
    } : null
    diagnosticSettingsLogAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    appSettings: [
      {
        name: 'App__EmailEnabled'
        value: 'true'
      }
      {
        name: 'App__SuppressExceptionsForTeamOnlyApiKeyErrors'
        value: string(suppressExceptionsForTeamOnlyApiKeyErrors)
      }
      {
        name: 'App__Url'
        value: 'https://${resourceNames.notifier.functionApp}.azurewebsites.net/api'
      }
      {
        name: 'App__PublicAppUrl'
        value: publicAppUrl
      }
      {
        name: 'App__NotifierStorageConnectionString'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.notifierStorageAccountConnectionString)
      }
      {
        name: 'App__TokenSecretKey'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.notifier.tokenSecretKey)
      }
      {
        name: 'GovUkNotify__ApiKey'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.notifier.govUkNotifyApiKey)
      }
    ]
    tagValues: tagValues
  }
}

import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
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

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@secure()
@description('''
The existing appsettings for this Function App, fetched by the pipeline before deployment. Used to
prevent infrastructure deploys from overriding application-specific appsettings that the code
deployment pipeline has since applied - see importer-appsettings.bicep.
''')
param existingAppSettings object = {}

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'importerAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.importer.appInsights
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

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.importer
  parent: vNet
}

resource adminSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.admin
  parent: vNet
}

module functionAppModule '../common/components/function-app/function-app.bicep' = {
  name: 'importerFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.importer.functionApp
    appServicePlanName: resourceNames.importer.appServicePlan
    storageAccountName: resourceNames.importer.storageAccount
    keyVaultName: resourceNames.keyVault.keyVault
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    sku: appServiceSku
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: 'Windows'
    netFrameworkVersion: 'v10.0'
    alwaysOn: true
    deployQueueRoleAssignment: true
    healthCheckPath: '/api/health'
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: outboundVnetSubnet.id
    storageAccountAllowedSubnetIds: [adminSubnet.id]
    storageAccountPublicNetworkAccessEnabled: true
    publicNetworkAccessEnabled: true
    storageFirewallRules: maintenanceIpRanges
    minTlsVersion: minTlsVersion
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=importer@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=importer@${coreSqlServerFqdn};Password=${databaseUserPassword};'
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
    // Application-specific appsettings (App__RowsPerBatch, App__PrivateStorageConnectionString,
    // App__ImporterStorageConnectionString) are no longer seeded from here - the app-release
    // pipeline applies them ahead of each code deploy via importer-appsettings.bicep. See
    // existingAppSettings above for how they're preserved across infrastructure deploys.
    appSettings: []
    existingAppSettings: existingAppSettings
    tagValues: tagValues
  }
}

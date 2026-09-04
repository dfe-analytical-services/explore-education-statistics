import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { keyVaultRef } from '../bicep-main-infrastructure-release/functions.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku AppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri

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

module functionAppModule '../common/components/function-app/function-app.bicep' = {
  name: 'importerFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.importer.functionApp
    appServicePlanName: resourceNames.importer.appServicePlan
    storageAccountName: resourceNames.importer.storageAccount
    keyVaultName: resourceNames.keyVault.keyVault
    sku: appServiceSku
    functionAppExists: true
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: 'Windows'
    alwaysOn: true
    deployQueueRoleAssignment: true
    healthCheckPath: '/'
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: resourceNames.vnet.subnets.importer
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
    diagnosticSettingEnabled: true
    appSettings: [
      {
        name: 'App__RowsPerBatch'
        value: '3000'
      }
      {
        name: 'App__PrivateStorageConnectionString'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.coreStorageAccountConnectionString)
      }
    ]
    tagValues: tagValues
  }
}

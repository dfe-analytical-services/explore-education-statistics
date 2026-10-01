import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('App Service Plan SKU.')
param appServiceSku AppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Whether to display detailed error messages in this environment or not.')
param detailedErrors bool

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]

@description('Whether analytics is enabled')
param analyticsEnabled bool

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@secure()
@description('The existing appsettings for the production slot, fetched by the pipeline before deployment. Used to prevent infrastructure deploys from overriding application-specific appsettings back to their original values.')
param existingProdAppSettings object = {}

@secure()
@description('The existing appsettings for the staging slot, fetched by the pipeline before deployment. Used to prevent infrastructure deploys from overriding application-specific appsettings back to their original values.')
param existingStagingSlotAppSettings object = {}

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName
var publicSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.publicSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

var analyticsFileShareMountPath string = '\\mounts\\analytics'

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' existing = {
  name: resourceNames.analytics.storage.storageAccountName
}

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'contentApiAppServicePlanModule'
  params: {
    planName: resourceNames.contentApi.appServicePlan
    sku: appServiceSku
    kind: 'app'
    operatingSystem: 'Windows'
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      cpuPercentage: true
      memoryPercentage: true
    } : null
    tagValues: tagValues
  }
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'contentApiAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.contentApi.appInsights
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

module appServiceModule '../common/components/app-service/app-service.bicep' = {
  name: 'contentApiAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.contentApi.appService
    kind: 'app'
    operatingSystem: 'Windows'
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    keyVaultRoles: {
      keyVaultName: resourceNames.keyVault.keyVault
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${publicSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=content@${publicSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=content@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
    ]
    vnetLink: {
      vnetName: resourceNames.vnet.vnet
      subnetName: resourceNames.vnet.subnets.contentApi
    }
    appInsightsName: appInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    allowedOrigins: allowedOrigins
    azureFileShares: analyticsEnabled ? [
      {
        storageName: analyticsStorageAccount.name
        storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
        storageAccountName: analyticsStorageAccount.name
        fileShareName: resourceNames.analytics.storage.fileShareName
        mountPath: analyticsFileShareMountPath
      }
    ] : []
    alerts: deployAlerts ? {
      appServiceHealth: true
      httpErrors: true
      alertsGroupName: resourceNames.alertsGroup
    } : null
    // Application-specific appsettings are controlled in the application release pipeline
    // rather than in the infrastructure rollout so that we can support slot swapping.
    applicationAppSettings: {}
    existingProdAppSettings: existingProdAppSettings
    existingStagingSlotAppSettings: existingStagingSlotAppSettings
    tagValues: tagValues
  }
}

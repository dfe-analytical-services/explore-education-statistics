import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { keyVaultRef } from '../bicep-main-infrastructure-release/functions.bicep'
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

@description('Whether or not to support Swagger routes for these APIs.')
param enableSwagger bool

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool

@description('Maximum number of table cells that a table builder query could potentially render for a request to be valid.')
param tableBuilderMaxTableCellsAllowed int

@description('Public URL of the public site.')
param publicAppUrl string

@description('Enables Basic Auth on the public application, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuth bool

@description('Username protecting the public app, no requirement to be secret, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuthUsername string

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]

@secure()
@description('Password protecting the public app, no requirement to be secret, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuthPassword string

@description('Whether analytics is enabled')
param analyticsEnabled bool

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName
var publicSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.publicSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

var analyticsFileShareMountPath string = '\\mounts\\analytics'

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' existing = {
  name: resourceNames.analytics.storage.storageAccountName
}

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'dataApiAppServicePlanModule'
  params: {
    planName: resourceNames.dataApi.appServicePlan
    sku: appServiceSku
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
  name: 'dataApiAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.dataApi.appInsights
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
  name: 'dataApiAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.dataApi.appService
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${publicSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=data@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=data@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
    ]
    vnetLink: {
      vnetName: resourceNames.vnet.vnet
      subnetName: resourceNames.vnet.subnets.dataApi
    }
    appInsightsName: appInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    allowedOrigins: allowedOrigins
    azureFileShares: [
      {
        storageName: analyticsStorageAccount.name
        storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
        storageAccountName: analyticsStorageAccount.name
        fileShareName: resourceNames.analytics.storage.fileShareName
        mountPath: analyticsFileShareMountPath
      }
    ]
    applicationAppSettings: {
      PublicStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
      enableSwagger: enableSwagger
      PublicApp__Url: publicAppUrl
      PublicApp__BasicAuth: publicAppBasicAuth
      PublicApp__BasicAuthUsername: publicAppBasicAuthUsername
      PublicApp__BasicAuthPassword: publicAppBasicAuthPassword
      Analytics__Enabled: analyticsEnabled
      Analytics__BasePath: analyticsFileShareMountPath
      TableBuilder__MaxTableCellsAllowed: tableBuilderMaxTableCellsAllowed
    }
    tagValues: tagValues
  }
}

import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { keyVaultRef } from '../common/functions.bicep'

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

@description('Whether the PrepareScheduledReleaseVersionsNow HTTP-triggered function is enabled.')
param prepareScheduledReleaseVersionsNowEnabled bool

@description('Whether the PublishScheduledReleaseVersionsNow HTTP-triggered function is enabled.')
param publishScheduledReleaseVersionsNowEnabled bool

@description('The time zone used for evaluating Cron expressions of the functions running with Cron triggers.')
param functionAppTimeZone string

@description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs.')
param prepareScheduledReleaseVersionsFunctionCronSchedule string

@description('Cron expression that defines when the PublishScheduledReleaseVersions function runs.')
param publishScheduledReleaseVersionsFunctionCronSchedule string

@description('The public-facing URL of the Admin site.')
param adminAppUrl string

@description('The public-facing URL of the public site.')
param publicAppUrl string

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
  name: resourceNames.vnet.subnets.publisher
  parent: vNet
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'publisherAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.publisher.appInsights
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
  name: 'publisherFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.publisher.functionApp
    appServicePlanName: resourceNames.publisher.appServicePlan
    storageAccountName: resourceNames.publisher.storageAccount
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
    healthCheckPath: '/'
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: outboundVnetSubnet.id
    minTlsVersion: minTlsVersion
    connectionStrings: [
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=publisher@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=publisher@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'PublicDataDb'
        type: 'Custom'
        connectionString: '@Microsoft.KeyVault(VaultName=${resourceNames.keyVault.keyVault};SecretName=${resourceNames.keyVault.secrets.publisher.publicDataDbConnectionString})'
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
        name: 'WEBSITE_TIME_ZONE'
        value: functionAppTimeZone
      }
      {
        name: 'AzureWebJobs.PrepareScheduledReleaseVersionsNow.Disabled'
        value: string(!prepareScheduledReleaseVersionsNowEnabled)
      }
      {
        name: 'AzureWebJobs.PublishScheduledReleaseVersionsNow.Disabled'
        value: string(!publishScheduledReleaseVersionsNowEnabled)
      }
      {
        name: 'App__PrepareScheduledReleaseVersionsFunctionCronSchedule'
        value: prepareScheduledReleaseVersionsFunctionCronSchedule
      }
      {
        name: 'App__PublishScheduledReleaseVersionsFunctionCronSchedule'
        value: publishScheduledReleaseVersionsFunctionCronSchedule
      }
      {
        name: 'App__PrivateStorageConnectionString'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.coreStorageAccountConnectionString)
      }
      {
        name: 'App__NotifierStorageConnectionString'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.notifierStorageAccountConnectionString)
      }
      {
        name: 'App__PublicStorageConnectionString'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
      }
      {
        name: 'App__BauEmail'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.bauEmail)
      }
      {
        name: 'App__AdminAppUrl'
        value: adminAppUrl
      }
      {
        name: 'App__PublicAppUrl'
        value: publicAppUrl
      }
      {
        name: 'Notify__ApiKey'
        value: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publisher.notifyApiKey)
      }
      {
        name: 'EventGrid__EventTopics__0__Key'
        value: 'PublicationChangedEvent'
      }
      {
        name: 'EventGrid__EventTopics__0__TopicEndpoint'
        value: reference(resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.publicationChanged), '2025-02-15').endpoint
      }
      {
        name: 'EventGrid__EventTopics__1__Key'
        value: 'ReleaseVersionChangedEvent'
      }
      {
        name: 'EventGrid__EventTopics__1__TopicEndpoint'
        value: reference(resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.releaseChanged), '2025-02-15').endpoint
      }
    ]
    tagValues: tagValues
  }
}

import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { MemoryCacheConfig } from '../bicep-main-infrastructure-release/types.bicep'
import { keyVaultRef } from '../bicep-main-infrastructure-release/functions.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { SignalRSku } from '../common/components/signalr/types.bicep'

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

@description('The origins supported for CORS calls to the Admin SignalR service.')
param signalRAllowedOrigins string[]

@description('SKU of the Admin SignalR service.')
param signalRSku SignalRSku

@description('Whether to display detailed error messages in this environment or not.')
param detailedErrors bool

@description('Public URI of the Admin site.')
param adminHostname string

@description('Whether or not to support Swagger routes for these APIs.')
param enableSwagger bool

@description('Whether or not theme deletion is allowed in this environmnt.')
param enableThemeDeletion bool

@description('Whether or not EIN published page deletion is allowed in this environment.')
param enableEinPublishedPageDeletion bool

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool

@description('Memory cache configuration.')
param memoryCacheConfig MemoryCacheConfig

@description('Pre-release start time as number of minutes before a release is scheduled to be published.')
param preReleaseMinutesBeforeStart int

@description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs in the Publisher Function App.')
param prepareScheduledReleaseVersionsFunctionCronSchedule string

@description('Cron expression that defines when the PublishScheduledReleaseVersions function runs in the Publisher Function App')
param publishScheduledReleaseVersionsFunctionCronSchedule string

@description('Maximum number of table cells that a table builder query could potentially render for a request to be valid.')
param tableBuilderMaxTableCellsAllowed int

@description('Public URL of the public site.')
param publicAppUrl string

@description('Public URL of the public API.')
param publicApiUrl string

@description('Public URL of the public API documentation site.')
param publicApiDocsUrl string

@description('The Client ID of a manually-created App Registration that represents the Public API Container App in Entra ID.')
param apiAppRegistrationClientId string

@description('The Client ID of a manually-created App Registration that represents the Public API Data Processor Function App in Entra ID.')
param publicDataProcessorAppRegistrationClientId string

@description('The Client ID of a manually-created App Registration that represents the Screener API Function App in Entra ID.')
param screenerAppRegistrationClientId string

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri

var signalrConnectionStringSecretUri = keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.adminSignalrConnectionString)

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'adminAppServicePlanModule'
  params: {
    planName: resourceNames.admin.appServicePlan
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
  name: 'adminAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.admin.appInsights
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
  name: 'adminAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.admin.appService
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    keyVaultName: resourceNames.keyVault.keyVault
    legacyKeyVaultRoleAssignmentName: true
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=adminapp@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=adminapp@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'PublicDataDb'
        type: 'Custom'
        connectionString: '@Microsoft.KeyVault(VaultName=${resourceNames.keyVault.keyVault};SecretName=ees-admin-connectionstring-publicdatadb)'
      }
    ]
    vnetLink: {
      vnetName: resourceNames.vnet.vnet
      subnetName: resourceNames.vnet.subnets.admin
    }
    appInsightsName: appInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    applicationAppSettings: {
      App__Url: 'https://${adminHostname}'
      App__EnableSwagger: enableSwagger
      App__EnableThemeDeletion: enableThemeDeletion
      App__EnableEinPublishedPageDeletion: enableEinPublishedPageDeletion
      Azure__SignalR__ConnectionString: signalrConnectionStringSecretUri
      EventGrid__EventTopics__0__Key: 'PublicationChangedEvent'
      EventGrid__EventTopics__0__TopicEndpoint: reference(
        resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.publicationChanged),
        '2025-02-15'
      ).endpoint
      EventGrid__EventTopics__1__Key: 'ReleaseChangedEvent'
      EventGrid__EventTopics__1__TopicEndpoint: reference(
        resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.releaseChanged),
        '2025-02-15'
      ).endpoint
      EventGrid__EventTopics__2__Key: 'ThemeChangedEvent'
      EventGrid__EventTopics__2__TopicEndpoint: reference(
        resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.themeChanged),
        '2025-02-15'
      ).endpoint
      IdentityServer__IssuerUri: 'urn=${adminHostname}'
      IdentityServer__Key__Name: 'CN=${adminHostname}'
      Notify__ApiKey: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.adminGovUkNotifyApiKey)
      OpenIdConnectIdentityFramework__ClientId: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectClientId)
      OpenIdConnectIdentityFramework__Authority: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectAuthority)
      OpenIdConnectIdentityFramework__TokenValidationParameters__ValidAudience: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectValidAudience)
      OpenIdConnectIdentityFramework__TokenValidationParameters__ValidIssuers: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectValidIssuers)
      OpenIdConnectSpaClient__ClientId: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectClientId)
      OpenIdConnectSpaClient__Authority: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectAuthority)
      'OpenIdConnectSpaClient__KnownAuthorities:0': keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectAuthority)
      OpenIdConnectSpaClient__AdminApiScope: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.openIdConnectFullyQualifiedScopeName)
      MemoryCache__Enabled: true
      MemoryCache__MaxCacheSizeMb: memoryCacheConfig.maxCacheSizeMb
      MemoryCache__ExpirationScanFrequencySeconds: memoryCacheConfig.expirationScanFrequencySeconds
      MemoryCache__Overrides__DurationInSeconds: memoryCacheConfig.?overridesDurationInSeconds
      MemoryCache__Overrides__ExpirySchedule: memoryCacheConfig.?overridesExpirySchedule
      CoreStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.coreStorageAccountConnectionString)
      PublicStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
      PublisherStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publisherStorageAccountConnectionString)
      PreReleaseAccess__AccessWindow__MinutesBeforeReleaseTimeStart: preReleaseMinutesBeforeStart
      ReleaseApproval__PrepareScheduledReleaseVersionsFunctionCronSchedule: prepareScheduledReleaseVersionsFunctionCronSchedule
      ReleaseApproval__PublishScheduledReleaseVersionsFunctionCronSchedule: publishScheduledReleaseVersionsFunctionCronSchedule
      TableBuilder__MaxTableCellsAllowed: tableBuilderMaxTableCellsAllowed
      PublicApp__Url: publicAppUrl
      PublicDataDbExists: true
      PublicDataApi__PublicUrl: 'https://${publicApiUrl}'
      PublicDataApi__PrivateUrl: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicApiContainerAppPrivateUrl)
      PublicDataApi__DocsUrl: 'https://${publicApiDocsUrl}'
      PublicDataApi__AppRegistrationClientId: apiAppRegistrationClientId
      PublicDataProcessor__Url: 'https://${resourceNames.publicApi.processor.functionApp}.azurewebsites.net'
      PublicDataProcessor__AppRegistrationClientId: publicDataProcessorAppRegistrationClientId
      DataScreener__Url: 'https://${resourceNames.screener.functionApp}.azurewebsites.net/api'
      DataScreener__AppRegistrationClientId: screenerAppRegistrationClientId
      DataScreener__ScreenerStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.screenerStorageAccountConnectionString)
      DataScreener__ScreenerProgressUpdateIntervalSeconds: 5
      DataScreener__ScreenerProgressUpdateFailureIntervalMinutes: 1440
    }
    tagValues: tagValues
  }
}

module adminSignalRService '../common/components/signalr/signalr.bicep' = {
  name: 'adminSignalRServiceDeploy'
  params: {
    signalRName: resourceNames.admin.signalRName
    sku: signalRSku
    allowedOrigins: signalRAllowedOrigins
    hubEventBaseUrl: 'https://${adminHostname}'
  }
}

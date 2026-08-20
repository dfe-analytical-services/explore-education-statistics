import { ResourceNames } from '../resource-names.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { MemoryCacheConfig } from '../types.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@secure()
@description('''Admin database user's password.''')
param sqlAdminUserPassword string

@description('Admin App Service Plan SKU.')
param adminSku AppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Whether to display detailed error messages in this environment or not.')
param detailedErrors bool

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

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

var adminSubnetRef = resourceId('Microsoft.Network/virtualNetworks/subnets', resourceNames.vnet.vnet, resourceNames.vnet.subnets.admin)

module adminAppServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'adminAppServicePlanModule'
  params: {
    planName: resourceNames.admin.appServicePlan
    sku: adminSku
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      cpuPercentage: true
      memoryPercentage: true
    } : null
    tagValues: tagValues
  }
}

module adminAppInsightsModule '../common/components/monitoring/appInsights.bicep' = {
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

module adminAppServiceModule '../common/components/app-service/app-service.bicep' = {
  name: 'adminAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.admin.appService
    minTlsVersion: minTlsVersion
    appServicePlanId: adminAppServicePlanModule.outputs.planId
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}').fullyQualifiedDomainName},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=adminapp@${reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}').fullyQualifiedDomainName};Password=${sqlAdminUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}').fullyQualifiedDomainName},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=adminapp@${reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}').fullyQualifiedDomainName};Password=${sqlAdminUserPassword};'
      }
      {
        name: 'PublicDataDb'
        type: 'Custom'
        connectionString: '@Microsoft.KeyVault(VaultName=${resourceNames.keyVault.keyVault};SecretName=ees-admin-connectionstring-publicdatadb)'
      }
    ]
    subnetRef: adminSubnetRef
    appInsightsName: adminAppInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    applicationAppSettings: {
      App__Url: 'https://${adminHostname}'
      App__EnableSwagger: enableSwagger
      App__EnableThemeDeletion: enableThemeDeletion
      App__EnableEinPublishedPageDeletion: enableEinPublishedPageDeletion
      Azure__SignalR__ConnectionString: '@Microsoft.KeyVault(SecretUri=${reference('ees_signalr_admin_connectionstring', '2018-02-14').secretUriWithVersion})'
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
      Notify__ApiKey: '@Microsoft.KeyVault(SecretUri=${reference('ees_admin_govuknotify_api_key', '2018-02-14').secretUriWithVersion})'
      OpenIdConnectIdentityFramework__ClientId: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_clientid', '2018-02-14').secretUriWithVersion})'
      OpenIdConnectIdentityFramework__Authority: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_authority', '2018-02-14').secretUriWithVersion})'
      OpenIdConnectIdentityFramework__TokenValidationParameters__ValidAudience: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_valid_audience','2018-02-14').secretUriWithVersion})'
      OpenIdConnectIdentityFramework__TokenValidationParameters__ValidIssuers: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_valid_issuers','2018-02-14').secretUriWithVersion})'
      OpenIdConnectSpaClient__ClientId: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_clientid', '2018-02-14').secretUriWithVersion})'
      OpenIdConnectSpaClient__Authority: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_authority', '2018-02-14').secretUriWithVersion})'
      'OpenIdConnectSpaClient__KnownAuthorities:0': '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_authority', '2018-02-14').secretUriWithVersion})'
      OpenIdConnectSpaClient__AdminApiScope: '@Microsoft.KeyVault(SecretUri=${reference('ees_openidconnect_fully_qualified_scope_name', '2018-02-14').secretUriWithVersion})'
      MemoryCache__Enabled: true
      MemoryCache__MaxCacheSizeMb: memoryCacheConfig.maxCacheSizeMb
      MemoryCache__ExpirationScanFrequencySeconds: memoryCacheConfig.expirationScanFrequencySeconds
      MemoryCache__Overrides__DurationInSeconds: memoryCacheConfig.?overridesDurationInSeconds
      MemoryCache__Overrides__ExpirySchedule: memoryCacheConfig.?overridesExpirySchedule
      CoreStorage: '@Microsoft.KeyVault(SecretUri=${reference('ees_storage_core', '2018-02-14').secretUriWithVersion})'
      PublicStorage: '@Microsoft.KeyVault(SecretUri=${reference('ees_storage_public').secretUriWithVersion})'
      PublisherStorage: '@Microsoft.KeyVault(SecretUri=${reference('ees_storage_publisher', '2018-02-14').secretUriWithVersion})'
      PreReleaseAccess__AccessWindow__MinutesBeforeReleaseTimeStart: preReleaseMinutesBeforeStart
      ReleaseApproval__PrepareScheduledReleaseVersionsFunctionCronSchedule: prepareScheduledReleaseVersionsFunctionCronSchedule
      ReleaseApproval__PublishScheduledReleaseVersionsFunctionCronSchedule: publishScheduledReleaseVersionsFunctionCronSchedule
      TableBuilder__MaxTableCellsAllowed: tableBuilderMaxTableCellsAllowed
      PublicApp__Url: publicAppUrl
      PublicDataDbExists: true
      PublicDataApi__PublicUrl: 'https://${publicApiUrl}'
      PublicDataApi__PrivateUrl: '@Microsoft.KeyVault(SecretUri=${reference('ees_publicapi_public_api_containerapp_private_url','2018-02-14').secretUriWithVersion})'
      PublicDataApi__DocsUrl: 'https://${publicApiDocsUrl}'
      PublicDataApi__AppRegistrationClientId: apiAppRegistrationClientId
      PublicDataProcessor__Url: 'https://${resourceNames.publicApi.processor.functionApp}.azurewebsites.net'
      PublicDataProcessor__AppRegistrationClientId: publicDataProcessorAppRegistrationClientId
      DataScreener__Url: 'https://${resourceNames.screener.functionApp}.azurewebsites.net/api'
      DataScreener__AppRegistrationClientId: screenerAppRegistrationClientId
      DataScreener__ScreenerStorage: '@Microsoft.KeyVault(SecretUri=${reference(resourceNames.keyVault.secrets.screenerStorageAccountConnectionString, '2018-02-14').secretUriWithVersion})'
      DataScreener__ScreenerProgressUpdateIntervalSeconds: 5
      DataScreener__ScreenerProgressUpdateFailureIntervalMinutes: 1440
    }
    tagValues: tagValues
  }
}


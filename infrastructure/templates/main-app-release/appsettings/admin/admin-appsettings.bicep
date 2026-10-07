import { getResourceNamesForEnvironment } from '../../../bicep-main-infrastructure-release/resource-names.bicep'
import { secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('Whether or not to enable Swagger API pages in this environment.')
param enableSwagger bool = false

@description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs in the Publisher Function App.')
param prepareScheduledReleaseVersionsFunctionCronSchedule string = '0 5 0 * * *'

@description('Cron expression that defines when the PublishScheduledReleaseVersions function runs in the Publisher Function App.')
param publishScheduledReleaseVersionsFunctionCronSchedule string = '0 30 9 * * *'

@description('Maximum number of table cells that a table builder query could potentially render for a request to be valid.')
param tableBuilderMaxTableCellsAllowed int = 1000000

@description('Global configuration for memory caches.')
param memoryCacheConfig {

  @description('The frequency of scans to evict expired entries from the in-memory cache.')
  expirationScanFrequencySeconds: int
  
  @description('Max size of in-memory cache in MBs.  This is an approximation based on the size of the cached objects in JSON notation.')
  maxCacheSizeMb: int
  
  @description('Override duration in seconds for all entities cached in memory')
  overridesDurationInSeconds: int?

  @description('Override cron expression for all entities cached in memory')
  overridesExpirySchedule: string?
} = {
  expirationScanFrequencySeconds: 60
  maxCacheSizeMb: 50
}

@description('Whether or not to enable theme deletion in this environment (for test teardown).')
param enableThemeDeletion bool = false

@description('Whether or not to enable published Education In Numbers pages deletion in this environment.')
param enableEinPublishedPageDeletion bool = false

@description('Pre-release start time as number of minutes before a release is scheduled to be published.')
param preReleaseMinutesBeforeStart int = 870

@description('The public URL for the public API (excluding "https://").')
param publicApiUrl string

@description('''
A deploy timestamp as an appsettings that is exposed via the health endpoint and allows us to determine
which deploy the currently running instances belong to. This gives us assurances during deployment that
the health check responses we are receiving are being served from newly-deployed instances, not
pre-existing instances that haven't shut down yet.
''')
param deployedAt string = ''

// getResourceNamesForEnvironment only actually reads environmentIdentifier/environmentName,
// but still expects something shaped like the shared EnvironmentConfig type - this plain
// object literal satisfies that structurally, without this file needing to import the type
// itself just to pass these two values through.
var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: environmentIdentifier
  environmentName: environmentName
})

var adminHostname = 'admin.${domain}'

var keyVaultName = resourceNames.keyVault.keyVault

// Used to encrypt the ASP.NET Core Data Protection key ring - see app-service.bicep /
// bicep-main-infrastructure-release/main.bicep's dataProtectionKey resource for why this is
// needed (the key ring can't safely live on local disk when shared across deployment slots).
// Deterministic, so no live lookup needed - unlike secrets, a Key Vault key's own identifier
// isn't sensitive.
var dataProtectionKeyUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/keys/${resourceNames.keyVault.keys.dataProtection}'
var keyVaultUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/'

// Every secret this stub's appsettings reference, looked up in one array/resource-loop rather
// than one named "existing" resource per secret.
var secretNames = [
  resourceNames.keyVault.secrets.admin.adminSignalrConnectionString
  resourceNames.keyVault.secrets.admin.adminGovUkNotifyApiKey
  resourceNames.keyVault.secrets.admin.openIdConnectClientId
  resourceNames.keyVault.secrets.admin.openIdConnectAuthority
  resourceNames.keyVault.secrets.admin.openIdConnectValidAudience
  resourceNames.keyVault.secrets.admin.openIdConnectValidIssuers
  resourceNames.keyVault.secrets.admin.openIdConnectFullyQualifiedScopeName
  resourceNames.keyVault.secrets.coreStorageAccountConnectionString
  resourceNames.keyVault.secrets.importerStorageAccountConnectionString
  resourceNames.keyVault.secrets.publicStorageAccountConnectionString
  resourceNames.keyVault.secrets.publisherStorageAccountConnectionString
  resourceNames.keyVault.secrets.publicApiContainerAppPrivateUrl
  resourceNames.keyVault.secrets.publicApi.apiAppRegistrationClientId
  resourceNames.keyVault.secrets.publicApi.dataProcessorAppRegistrationClientId
  resourceNames.keyVault.secrets.screener.appRegistrationClientId
  resourceNames.keyVault.secrets.admin.screenerStorageAccountConnectionString
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for Admin, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  App__Url: 'https://${adminHostname}'
  App__EnableSwagger: enableSwagger
  App__EnableThemeDeletion: enableThemeDeletion
  App__EnableEinPublishedPageDeletion: enableEinPublishedPageDeletion
  Azure__SignalR__ConnectionString: secretRefs[resourceNames.keyVault.secrets.admin.adminSignalrConnectionString]
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
  Notify__ApiKey: secretRefs[resourceNames.keyVault.secrets.admin.adminGovUkNotifyApiKey]
  OpenIdConnectIdentityFramework__ClientId: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectClientId]
  OpenIdConnectIdentityFramework__Authority: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectAuthority]
  OpenIdConnectIdentityFramework__TokenValidationParameters__ValidAudience: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectValidAudience]
  OpenIdConnectIdentityFramework__TokenValidationParameters__ValidIssuers: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectValidIssuers]
  OpenIdConnectSpaClient__ClientId: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectClientId]
  OpenIdConnectSpaClient__Authority: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectAuthority]
  'OpenIdConnectSpaClient__KnownAuthorities:0': secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectAuthority]
  OpenIdConnectSpaClient__AdminApiScope: secretRefs[resourceNames.keyVault.secrets.admin.openIdConnectFullyQualifiedScopeName]
  MemoryCache__Enabled: true
  MemoryCache__MaxCacheSizeMb: memoryCacheConfig.maxCacheSizeMb
  MemoryCache__ExpirationScanFrequencySeconds: memoryCacheConfig.expirationScanFrequencySeconds
  MemoryCache__Overrides__DurationInSeconds: memoryCacheConfig.?overridesDurationInSeconds
  MemoryCache__Overrides__ExpirySchedule: memoryCacheConfig.?overridesExpirySchedule
  CoreStorage: secretRefs[resourceNames.keyVault.secrets.coreStorageAccountConnectionString]
  ImporterStorage: secretRefs[resourceNames.keyVault.secrets.importerStorageAccountConnectionString]
  PublicStorage: secretRefs[resourceNames.keyVault.secrets.publicStorageAccountConnectionString]
  PublisherStorage: secretRefs[resourceNames.keyVault.secrets.publisherStorageAccountConnectionString]
  PreReleaseAccess__AccessWindow__MinutesBeforeReleaseTimeStart: preReleaseMinutesBeforeStart
  ReleaseApproval__PrepareScheduledReleaseVersionsFunctionCronSchedule: prepareScheduledReleaseVersionsFunctionCronSchedule
  ReleaseApproval__PublishScheduledReleaseVersionsFunctionCronSchedule: publishScheduledReleaseVersionsFunctionCronSchedule
  TableBuilder__MaxTableCellsAllowed: tableBuilderMaxTableCellsAllowed
  PublicApp__Url: 'https://${domain}'
  PublicDataDbExists: true
  PublicDataApi__PublicUrl: 'https://${publicApiUrl}'
  PublicDataApi__PrivateUrl: secretRefs[resourceNames.keyVault.secrets.publicApiContainerAppPrivateUrl]
  PublicDataApi__DocsUrl: 'https://${publicApiUrl}/docs'
  PublicDataApi__AppRegistrationClientId: secretRefs[resourceNames.keyVault.secrets.publicApi.apiAppRegistrationClientId]
  PublicDataProcessor__Url: 'https://${resourceNames.publicApi.processor.functionApp}.azurewebsites.net'
  PublicDataProcessor__AppRegistrationClientId: secretRefs[resourceNames.keyVault.secrets.publicApi.dataProcessorAppRegistrationClientId]
  DataScreener__Url: 'https://${resourceNames.screener.functionApp}.azurewebsites.net/api'
  DataScreener__AppRegistrationClientId: secretRefs[resourceNames.keyVault.secrets.screener.appRegistrationClientId]
  DataScreener__ScreenerStorage: secretRefs[resourceNames.keyVault.secrets.admin.screenerStorageAccountConnectionString]
  DataScreener__ScreenerProgressUpdateIntervalSeconds: 5
  DataScreener__ScreenerProgressUpdateFailureIntervalMinutes: 1440
  DataProtection__KeyVaultKeyUri: dataProtectionKeyUri
  DataProtection__KeyVaultUri: keyVaultUri
  Deploy__DeployedAt: deployedAt
}

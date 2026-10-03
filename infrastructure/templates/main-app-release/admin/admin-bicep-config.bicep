import { getResourceNamesForEnvironment } from '../../bicep-main-infrastructure-release/resource-names.bicep'
import { EnvironmentConfig, mergeEnvironmentConfig } from '../../bicep-main-infrastructure-release/configuration/environment-configuration.bicep'
import { AdminConfig, mergeAdminConfig } from '../../bicep-main-infrastructure-release/configuration/admin-configuration.bicep'
import { PublicApiConfig, mergePublicApiConfig } from '../../bicep-main-infrastructure-release/configuration/public-api-configuration.bicep'
import { secretRefsFromSecrets } from '../../common/functions.bicep'

@description('Environment-wide configuration values needed to compute this app\'s appsettings.')
param environmentConfigParam EnvironmentConfig = {}

@description('Admin-specific configuration values needed to compute this app\'s appsettings.')
param adminConfigParam AdminConfig = {}

@description('Public API configuration values needed to compute this app\'s appsettings.')
param publicApiConfigParam PublicApiConfig = {}

@description('A marker unique to this deploy (the pipeline run\'s own timestamp), surfaced via the app\'s health/config endpoint so the pipeline can confirm the new appsettings and code have actually taken effect - see wait-for-app-service-restart.yml.')
param deployedAt string = ''

var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)
var adminConfig = mergeAdminConfig(adminConfigParam)
var publicApiConfig = mergePublicApiConfig(publicApiConfigParam)
var resourceNames = getResourceNamesForEnvironment(environmentConfig)

var adminHostname = 'admin.${environmentConfig.domain!}'
var publicApiUrl = publicApiConfig.publicUrl!
var memoryCacheConfig = environmentConfig.memoryCacheConfig!

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
  App__EnableSwagger: environmentConfig.enableSwagger!
  App__EnableThemeDeletion: adminConfig.enableThemeDeletion!
  App__EnableEinPublishedPageDeletion: adminConfig.enableEinPublishedPageDeletion!
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
  PreReleaseAccess__AccessWindow__MinutesBeforeReleaseTimeStart: adminConfig.preReleaseMinutesBeforeStart!
  ReleaseApproval__PrepareScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.prepareScheduledReleaseVersionsFunctionCronSchedule!
  ReleaseApproval__PublishScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.publishScheduledReleaseVersionsFunctionCronSchedule!
  TableBuilder__MaxTableCellsAllowed: environmentConfig.tableBuilderMaxTableCellsAllowed!
  PublicApp__Url: 'https://${environmentConfig.domain!}'
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

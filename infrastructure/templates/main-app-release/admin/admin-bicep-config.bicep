import { getResourceNamesForEnvironment } from '../../bicep-main-infrastructure-release/resource-names.bicep'
import { EnvironmentConfig, mergeEnvironmentConfig } from '../../bicep-main-infrastructure-release/configuration/environment-configuration.bicep'
import { AdminConfig, mergeAdminConfig } from '../../bicep-main-infrastructure-release/configuration/admin-configuration.bicep'
import { PublicApiConfig, mergePublicApiConfig } from '../../bicep-main-infrastructure-release/configuration/public-api-configuration.bicep'
import { keyVaultRef } from '../../common/functions.bicep'

@description('Environment-wide configuration values needed to compute this app\'s appsettings.')
param environmentConfigParam EnvironmentConfig = {}

@description('Admin-specific configuration values needed to compute this app\'s appsettings.')
param adminConfigParam AdminConfig = {}

@description('Public API configuration values needed to compute this app\'s appsettings.')
param publicApiConfigParam PublicApiConfig = {}

var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)
var adminConfig = mergeAdminConfig(adminConfigParam)
var publicApiConfig = mergePublicApiConfig(publicApiConfigParam)
var resourceNames = getResourceNamesForEnvironment(environmentConfig)

var adminHostname = 'admin.${environmentConfig.domain!}'
var publicApiUrl = publicApiConfig.publicUrl!

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri
var memoryCacheConfig = environmentConfig.memoryCacheConfig!

// Used to encrypt the ASP.NET Core Data Protection key ring - see app-service.bicep /
// bicep-main-infrastructure-release/main.bicep's dataProtectionKey resource for why this is
// needed (the key ring can't safely live on local disk when shared across deployment slots).
var dataProtectionKeyUri = '${vaultUri}keys/${resourceNames.keyVault.keys.dataProtection}'

@description('Application-specific appsettings for Admin, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  App__Url: 'https://${adminHostname}'
  App__EnableSwagger: environmentConfig.enableSwagger!
  App__EnableThemeDeletion: adminConfig.enableThemeDeletion!
  App__EnableEinPublishedPageDeletion: adminConfig.enableEinPublishedPageDeletion!
  Azure__SignalR__ConnectionString: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.adminSignalrConnectionString)
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
  ImporterStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.importerStorageAccountConnectionString)
  PublicStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
  PublisherStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publisherStorageAccountConnectionString)
  PreReleaseAccess__AccessWindow__MinutesBeforeReleaseTimeStart: adminConfig.preReleaseMinutesBeforeStart!
  ReleaseApproval__PrepareScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.prepareScheduledReleaseVersionsFunctionCronSchedule!
  ReleaseApproval__PublishScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.publishScheduledReleaseVersionsFunctionCronSchedule!
  TableBuilder__MaxTableCellsAllowed: environmentConfig.tableBuilderMaxTableCellsAllowed!
  PublicApp__Url: 'https://${environmentConfig.domain!}'
  PublicDataDbExists: true
  PublicDataApi__PublicUrl: 'https://${publicApiUrl}'
  PublicDataApi__PrivateUrl: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicApiContainerAppPrivateUrl)
  PublicDataApi__DocsUrl: 'https://${publicApiUrl}/docs'
  PublicDataApi__AppRegistrationClientId: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicApi.apiAppRegistrationClientId)
  PublicDataProcessor__Url: 'https://${resourceNames.publicApi.processor.functionApp}.azurewebsites.net'
  PublicDataProcessor__AppRegistrationClientId: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicApi.dataProcessorAppRegistrationClientId)
  DataScreener__Url: 'https://${resourceNames.screener.functionApp}.azurewebsites.net/api'
  DataScreener__AppRegistrationClientId: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.screener.appRegistrationClientId)
  DataScreener__ScreenerStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.admin.screenerStorageAccountConnectionString)
  DataScreener__ScreenerProgressUpdateIntervalSeconds: 5
  DataScreener__ScreenerProgressUpdateFailureIntervalMinutes: 1440
  DataProtection__KeyVaultKeyUri: dataProtectionKeyUri
  DataProtection__KeyVaultUri: vaultUri
}

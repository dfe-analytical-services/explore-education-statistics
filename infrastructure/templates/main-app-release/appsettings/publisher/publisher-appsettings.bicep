import { getResourceNamesForEnvironment } from '../../../bicep-main-infrastructure-release/resource-names.bicep'
import { secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('The time zone used for evaluating Cron expressions of the functions running with Cron triggers.')
param functionAppTimeZone string = 'GMT Standard Time'

@description('Whether the PrepareScheduledReleaseVersionsNow HTTP-triggered function is enabled. Should remain disabled in Production.')
param prepareScheduledReleaseVersionsNowEnabled bool = false

@description('Whether the PublishScheduledReleaseVersionsNow HTTP-triggered function is enabled. Should remain disabled in Production.')
param publishScheduledReleaseVersionsNowEnabled bool = false

@description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs in the Publisher Function App.')
param prepareScheduledReleaseVersionsFunctionCronSchedule string = '0 5 0 * * *'

@description('Cron expression that defines when the PublishScheduledReleaseVersions function runs in the Publisher Function App.')
param publishScheduledReleaseVersionsFunctionCronSchedule string = '0 30 9 * * *'

@description('''
Whether the Publisher is allowed to purge superseded all-files ZIPs from Azure Front Door. Requires the Front
Door role assignment granting the Publisher purge permissions to have been deployed to this environment.
''')
param frontDoorCachePurgeEnabled bool = true

@description('''
A deploy timestamp as an appsettings that is exposed via the health endpoint and allows us to determine
which deploy the currently running instances belong to. This gives us assurances during deployment that
the health check responses we are receiving are being served from newly-deployed instances, not
pre-existing instances that haven't shut down yet.
''')
param deployedAt string = ''

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: environmentIdentifier
  environmentName: environmentName
})

var keyVaultName = resourceNames.keyVault.keyVault

var adminAppUrl = 'https://admin.${domain}'
var publicAppUrl = 'https://${domain}'

// Pre-Production's public Content API hostname is "cont.", not "content." - see
// public-site-appsettings.bicep for the same quirk.
var contentApiHostName = '${environmentName == 'Pre-Production' ? 'cont' : 'content'}.${domain}'

// Deterministic, so no live lookup needed - matches bicep-main-infrastructure-release/main.bicep's
// own afdEndpointResourceId.
var frontDoorEndpointResourceId = resourceId(
  'Microsoft.Cdn/profiles/afdEndpoints',
  resourceNames.frontDoor.frontDoorName,
  resourceNames.frontDoor.defaultEndpoint.endpointName
)

var secretNames = [
  resourceNames.keyVault.secrets.coreStorageAccountConnectionString
  resourceNames.keyVault.secrets.notifierStorageAccountConnectionString
  resourceNames.keyVault.secrets.publicStorageAccountConnectionString
  resourceNames.keyVault.secrets.publisherStorageAccountConnectionString
  resourceNames.keyVault.secrets.bauEmail
  resourceNames.keyVault.secrets.publisher.notifyApiKey
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-appsettings.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for Publisher, applied to it ahead of each code deploy.')
output appSettings object = {
  WEBSITE_TIME_ZONE: functionAppTimeZone
  'AzureWebJobs.PrepareScheduledReleaseVersionsNow.Disabled': string(!prepareScheduledReleaseVersionsNowEnabled)
  'AzureWebJobs.PublishScheduledReleaseVersionsNow.Disabled': string(!publishScheduledReleaseVersionsNowEnabled)
  App__PrepareScheduledReleaseVersionsFunctionCronSchedule: prepareScheduledReleaseVersionsFunctionCronSchedule
  App__PublishScheduledReleaseVersionsFunctionCronSchedule: publishScheduledReleaseVersionsFunctionCronSchedule
  App__PrivateStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.coreStorageAccountConnectionString]
  App__NotifierStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.notifierStorageAccountConnectionString]
  App__PublicStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.publicStorageAccountConnectionString]
  App__PublisherStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.publisherStorageAccountConnectionString]
  App__BauEmail: secretRefs[resourceNames.keyVault.secrets.bauEmail]
  App__AdminAppUrl: adminAppUrl
  App__PublicAppUrl: publicAppUrl
  AzureFrontDoor__CachePurgeEnabled: string(frontDoorCachePurgeEnabled)
  AzureFrontDoor__EndpointResourceId: frontDoorEndpointResourceId
  AzureFrontDoor__ContentApiHostName: contentApiHostName
  Notify__ApiKey: secretRefs[resourceNames.keyVault.secrets.publisher.notifyApiKey]
  DataFiles__BasePath: '\\mounts\\public-api-data'
  EventGrid__EventTopics__0__Key: 'PublicationChangedEvent'
  EventGrid__EventTopics__0__TopicEndpoint: reference(
    resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.publicationChanged),
    '2025-02-15'
  ).endpoint
  EventGrid__EventTopics__1__Key: 'ReleaseVersionChangedEvent'
  EventGrid__EventTopics__1__TopicEndpoint: reference(
    resourceId('Microsoft.EventGrid/topics', resourceNames.eventGrid.topics.releaseVersionChanged),
    '2025-02-15'
  ).endpoint
  PublicDataDbExists: 'true'
  Deploy__DeployedAt: deployedAt
}

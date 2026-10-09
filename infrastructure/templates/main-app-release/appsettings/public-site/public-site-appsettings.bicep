import { getResourceNamesForEnvironment } from '../../../bicep-main-infrastructure-release/resource-names.bicep'
import { secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('Enables Basic Auth on the public application, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance).')
param basicAuthEnabled bool = false

@description('Google Analytics tracking ID for the public app. Leave as empty string to disable Google Analytics.')
param googleAnalyticsTrackingId string

@description('The default duration in seconds for Azure Front Door to cache content.')
param defaultCacheMaxAgeSeconds int = 30

@description('The public URL for the public API (excluding "https://").')
param publicApiUrl string

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

var contentApiPublicHostname = '${environmentName == 'Pre-Production' ? 'cont' : 'content'}.${domain}'
var dataApiPublicHostname = 'data.${domain}'
var publicAppUrl = 'https://${domain}'

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: resourceNames.search.service
}

resource nlSearchFunctionApp 'Microsoft.Web/sites@2025-03-01' existing = {
  name: resourceNames.nlSearch.functionApp
}

var secretNames = [
  resourceNames.keyVault.secrets.publicSite.basicAuthUsername
  resourceNames.keyVault.secrets.publicSite.basicAuthPassword
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-appsettings.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for the Public Site, applied ahead of each code deploy.')
output appSettings object = {
  APP_ENV: environmentName
  AZURE_SEARCH_ENDPOINT: searchService.properties.endpoint
  AZURE_SEARCH_INDEX: 'index-1'
  AZURE_DATASETS_SEARCH_INDEX: 'nl-search-dataset-index'
  AZURE_TABLE_TOOL_SEARCH_ENDPOINT: 'https://${nlSearchFunctionApp.properties.defaultHostName}/api/natural_language_search_function'
  BASIC_AUTH: basicAuthEnabled
  BASIC_AUTH_USERNAME: secretRefs[resourceNames.keyVault.secrets.publicSite.basicAuthUsername]
  BASIC_AUTH_PASSWORD: secretRefs[resourceNames.keyVault.secrets.publicSite.basicAuthPassword]
  CONTENT_API_BASE_URL: 'https://${contentApiPublicHostname}/api'
  DATA_API_BASE_URL: 'https://${dataApiPublicHostname}/api'
  NOTIFICATION_API_BASE_URL: 'https://${resourceNames.notifier.functionApp}.azurewebsites.net/api'
  GA_TRACKING_ID: googleAnalyticsTrackingId
  PUBLIC_URL: '${publicAppUrl}/'
  PUBLIC_API_BASE_URL: 'https://${publicApiUrl}'
  PUBLIC_API_DOCS_URL: 'https://${publicApiUrl}/docs'
  DEFAULT_CACHE_MAX_AGE_SECONDS: defaultCacheMaxAgeSeconds
  DEPLOYED_AT: deployedAt
}

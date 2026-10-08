import { getResourceNamesForEnvironment } from '../../../bicep-main-infrastructure-release/resource-names.bicep'
import { secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('Replaces Notify exceptions with logged messages only when team-only API keys are used and a recipient email address is not valid for that key.')
param suppressExceptionsForTeamOnlyApiKeyErrors bool = false

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

var publicAppUrl = 'https://${domain}'

var secretNames = [
  resourceNames.keyVault.secrets.notifierStorageAccountConnectionString
  resourceNames.keyVault.secrets.notifier.tokenSecretKey
  resourceNames.keyVault.secrets.notifier.govUkNotifyApiKey
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-appsettings.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for Notifier, applied to it ahead of each code deploy.')
output appSettings object = {
  App__EmailEnabled: 'true'
  App__SuppressExceptionsForTeamOnlyApiKeyErrors: string(suppressExceptionsForTeamOnlyApiKeyErrors)
  App__Url: 'https://${resourceNames.notifier.functionApp}.azurewebsites.net/api'
  App__PublicAppUrl: publicAppUrl
  App__NotifierStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.notifierStorageAccountConnectionString]
  App__TokenSecretKey: secretRefs[resourceNames.keyVault.secrets.notifier.tokenSecretKey]
  GovUkNotify__ApiKey: secretRefs[resourceNames.keyVault.secrets.notifier.govUkNotifyApiKey]
  Deploy__DeployedAt: deployedAt
}

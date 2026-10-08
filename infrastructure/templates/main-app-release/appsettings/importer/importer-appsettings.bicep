import { getResourceNamesForEnvironment } from '../../../bicep-main-infrastructure-release/resource-names.bicep'
import { secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('Number of rows processed per batch during data file import.')
param rowsPerBatch int = 3000

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

var secretNames = [
  resourceNames.keyVault.secrets.coreStorageAccountConnectionString
  resourceNames.keyVault.secrets.importerStorageAccountConnectionString
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-appsettings.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for Importer, applied to it ahead of each code deploy.')
output appSettings object = {
  App__RowsPerBatch: rowsPerBatch
  App__PrivateStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.coreStorageAccountConnectionString]
  App__ImporterStorageConnectionString: secretRefs[resourceNames.keyVault.secrets.importerStorageAccountConnectionString]
  Deploy__DeployedAt: deployedAt
}

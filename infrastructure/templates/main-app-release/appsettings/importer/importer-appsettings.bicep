import { getResourceNamesForEnvironment } from '../../../common/resource-names.bicep'
import { keylessStorageConnectionString, secretRefsFromSecrets } from '../../../common/functions.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01. Supplied by the pipeline from the environment\'s variable group ("subscription").')
param environmentIdentifier string = ''

@description('Name of this environment e.g. Development, Test.')
param environmentName string

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

// TODO EES-7502 - remove the Key Vault alternative once every environment has moved to the ELZ.
@description('Whether the app connects to storage accounts with its managed identity, given keyless connection strings, rather than with connection strings from Key Vault. Only the param files in "parameters/elz" turn this on.')
param identityBasedStorageConnections bool = false

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
  elz: elz
})

var keyVaultName = resourceNames.keyVault.keyVault

// The app's only secrets are its storage connection strings, only looked up when the app doesn't connect to storage
// with its managed identity. They aren't stored in that case.
var storageSecretNames = identityBasedStorageConnections
  ? []
  : [
      resourceNames.keyVault.secrets.coreStorageAccountConnectionString
      resourceNames.keyVault.secrets.importerStorageAccountConnectionString
    ]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in storageSecretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-appsettings.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

// Each storage account's connection string: keyless, so that the app connects with its managed identity, or else a
// Key Vault reference.
var storageConnectionStrings = identityBasedStorageConnections
  ? {
      core: keylessStorageConnectionString(resourceNames.storageAccounts.core)
      importer: keylessStorageConnectionString(resourceNames.importer.storageAccount)
    }
  : {
      core: secretRefs[resourceNames.keyVault.secrets.coreStorageAccountConnectionString]
      importer: secretRefs[resourceNames.keyVault.secrets.importerStorageAccountConnectionString]
    }

@description('Application-specific appsettings for Importer, applied to it ahead of each code deploy.')
output appSettings object = {
  App__RowsPerBatch: rowsPerBatch
  App__PrivateStorageConnectionString: storageConnectionStrings.core
  App__ImporterStorageConnectionString: storageConnectionStrings.importer
  Deploy__DeployedAt: deployedAt
}

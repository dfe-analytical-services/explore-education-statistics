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

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('Whether or not to enable Swagger API pages in this environment.')
param enableSwagger bool = false

@description('Whether analytics is enabled.')
param analyticsEnabled bool = true

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

var analyticsFileShareMountPath = '\\mounts\\analytics'

var keyVaultName = resourceNames.keyVault.keyVault

// Used to encrypt the ASP.NET Core Data Protection key ring - see app-service.bicep /
// bicep-main-infrastructure-release/main.bicep's dataProtectionKey resource for why this is
// needed (the key ring can't safely live on local disk when shared across deployment slots).
// Deterministic, so no live lookup needed - unlike secrets, a Key Vault key's own identifier
// isn't sensitive.
var dataProtectionKeyUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/keys/${resourceNames.keyVault.keys.dataProtection}'
var keyVaultUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/'

// The app's only secrets are its storage connection strings, only looked up when the app doesn't connect to storage
// with its managed identity. They aren't stored in that case.
var storageSecretNames = identityBasedStorageConnections
  ? []
  : [
      resourceNames.keyVault.secrets.publicStorageAccountConnectionString
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
      public: keylessStorageConnectionString(resourceNames.storageAccounts.public)
    }
  : {
      public: secretRefs[resourceNames.keyVault.secrets.publicStorageAccountConnectionString]
    }

@description('Application-specific appsettings for the Content API, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  PublicStorage: storageConnectionStrings.public
  enableSwagger: enableSwagger
  PublicApp__Url: 'https://${domain}'
  Analytics__Enabled: analyticsEnabled
  Analytics__BasePath: analyticsFileShareMountPath
  DataProtection__KeyVaultKeyUri: dataProtectionKeyUri
  DataProtection__KeyVaultUri: keyVaultUri
  Deploy__DeployedAt: deployedAt
}

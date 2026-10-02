import { getResourceNamesForEnvironment } from '../../bicep-main-infrastructure-release/resource-names.bicep'
import { EnvironmentConfig, mergeEnvironmentConfig } from '../../bicep-main-infrastructure-release/configuration/environment-configuration.bicep'
import { secretRefsFromSecrets } from '../../common/functions.bicep'

@description('Environment-wide configuration values needed to compute this app\'s appsettings.')
param environmentConfigParam EnvironmentConfig = {}

var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)
var resourceNames = getResourceNamesForEnvironment(environmentConfig)

var analyticsFileShareMountPath = '\\mounts\\analytics'

var keyVaultName = resourceNames.keyVault.keyVault

// Used to encrypt the ASP.NET Core Data Protection key ring - see app-service.bicep /
// bicep-main-infrastructure-release/main.bicep's dataProtectionKey resource for why this is
// needed (the key ring can't safely live on local disk when shared across deployment slots).
// Deterministic, so no live lookup needed - unlike secrets, a Key Vault key's own identifier
// isn't sensitive.
var dataProtectionKeyUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/keys/${resourceNames.keyVault.keys.dataProtection}'
var keyVaultUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/'

var secretNames = [
  resourceNames.keyVault.secrets.publicStorageAccountConnectionString
]

resource secrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = [for secretName in secretNames: {
  name: '${keyVaultName}/${secretName}'
}]

// Maps each secret name above to a ready-to-use Key Vault reference pinned to that secret's
// current version (rather than "latest"), so the resulting appsetting value changes whenever
// the secret is rotated - see admin-bicep-config.bicep for why that matters.
var secretRefs = secretRefsFromSecrets(secrets)

@description('Application-specific appsettings for the Content API, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  PublicStorage: secretRefs[resourceNames.keyVault.secrets.publicStorageAccountConnectionString]
  enableSwagger: environmentConfig.enableSwagger!
  PublicApp__Url: 'https://${environmentConfig.domain!}'
  Analytics__Enabled: environmentConfig.analyticsEnabled!
  Analytics__BasePath: analyticsFileShareMountPath
  DataProtection__KeyVaultKeyUri: dataProtectionKeyUri
  DataProtection__KeyVaultUri: keyVaultUri
}

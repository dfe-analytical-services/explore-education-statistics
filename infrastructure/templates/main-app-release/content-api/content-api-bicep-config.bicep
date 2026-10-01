import { getResourceNamesForEnvironment } from '../../bicep-main-infrastructure-release/resource-names.bicep'
import { EnvironmentConfig, mergeEnvironmentConfig } from '../../bicep-main-infrastructure-release/configuration/environment-configuration.bicep'
import { keyVaultRef } from '../../common/functions.bicep'

@description('Environment-wide configuration values needed to compute this app\'s appsettings.')
param environmentConfigParam EnvironmentConfig = {}

var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)
var resourceNames = getResourceNamesForEnvironment(environmentConfig)

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var vaultUri = keyVault.properties.vaultUri
var analyticsFileShareMountPath = '\\mounts\\analytics'

// Used to encrypt the ASP.NET Core Data Protection key ring - see app-service.bicep /
// bicep-main-infrastructure-release/main.bicep's dataProtectionKey resource for why this is
// needed (the key ring can't safely live on local disk when shared across deployment slots).
var dataProtectionKeyUri = '${vaultUri}keys/${resourceNames.keyVault.keys.dataProtection}'

@description('Application-specific appsettings for the Content API, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  PublicStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
  enableSwagger: environmentConfig.enableSwagger!
  PublicApp__Url: 'https://${environmentConfig.domain!}'
  Analytics__Enabled: environmentConfig.analyticsEnabled!
  Analytics__BasePath: analyticsFileShareMountPath
  DataProtection__KeyVaultKeyUri: dataProtectionKeyUri
  DataProtection__KeyVaultUri: vaultUri
}

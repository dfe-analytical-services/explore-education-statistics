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

@description('Application-specific appsettings for the Data API, applied to its staging slot ahead of each code deploy.')
output appSettings object = {
  PublicStorage: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicStorageAccountConnectionString)
  enableSwagger: environmentConfig.enableSwagger!
  PublicApp__Url: 'https://${environmentConfig.domain!}'
  PublicApp__BasicAuth: environmentConfig.basicAuthEnabled!
  PublicApp__BasicAuthUsername: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicSite.basicAuthUsername)
  PublicApp__BasicAuthPassword: keyVaultRef(vaultUri, resourceNames.keyVault.secrets.publicSite.basicAuthPassword)
  Analytics__Enabled: environmentConfig.analyticsEnabled!
  Analytics__BasePath: analyticsFileShareMountPath
  TableBuilder__MaxTableCellsAllowed: environmentConfig.tableBuilderMaxTableCellsAllowed!
}

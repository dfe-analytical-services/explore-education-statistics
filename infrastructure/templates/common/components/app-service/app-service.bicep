import { ConnectionString } from '../../types.bicep'
import { AzureFileShareMount } from '../storage/types.bicep'
import { builtInRoleDefinitionIds } from '../../builtInRoles.bicep'

@description('Name of the App Service.')
param appServiceName string

@description('Name of the App Insights instance that this App Service is connected to.')
param appInsightsName string

@description('The operating system to use to host App Services.')
param operatingSystem 'Windows' | 'Linux'

@description('The kind of plan to create. Use "app,linux,container" and "Linux" for the "operatingSystem" param for App Services for Docker.')
param kind 'app' | 'app,linux,container'

@description('Details of common Key Vault roles to apply to this App Service.')
param keyVaultRoles {
  keyVaultName: string
  secretsUser: bool?
  secretsOfficer: bool?
  certificateUser: bool?
  cryptoUser: bool?

  @description('Whether to use the default role assignment name generation or the legacy name generation scheme.')
  legacyKeyVaultRoleAssignmentName: bool
}?

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('The owning App Service Plan id.')
param appServicePlanId string

@description('Subnet used to connect the App Service to a VNet, if required.')
param vnetLink {
  vnetName: string
  subnetName: string
}?

@description('Database connection strings.')
param connectionStrings ConnectionString[]?

@description('''
Application-specific appsettings. These will be merged with infrastructure appsettings and applied
to both the production and staging slots. This serves only as a bootstrap default for the very first
deploy of this App Service - on every subsequent deploy, "existingProdAppSettings" / "existingStagingSlotAppSettings"
take precedence over these values, so that infrastructure deploys do not reset application-specific
appsettings back to these original values.
''')
param applicationAppSettings object

@secure()
@description('''
The existing appsettings for the production slot, fetched by the pipeline before deployment. Used to
prevent infrastructure deploys from overriding application-specific appsettings back to their original values.
See https://blog.dotnetstudio.nl/posts/2021/04/merge-appsettings-with-bicep.
''')
param existingProdAppSettings object = {}

@secure()
@description('The existing appsettings for the staging slot, fetched by the pipeline before deployment. Used to prevent infrastructure deploys from overriding application-specific appsettings back to their original values.')
param existingStagingSlotAppSettings object = {}

@description('Whether or not to display detailed error messages in this environment.')
param detailedErrors bool

@description('Whether or not to enable autoscaling in this environment.')
param autoscaleEnabled bool

@description('Whether or not to enable slot swapping. Deploys a swap slot if enabled.')
param swapSlotEnabled bool = true

@description('Path the platform should ping to judge the app healthy during its own slot-swap warm-up, when swapSlotEnabled is true.')
param healthCheckPath string = '/api/health'

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]?

@description('File Shares to mount on this App Service and its slots.')
param azureFileShares AzureFileShareMount[]?

@description('Specific port to serve site traffic from.')
param websitePort int?

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param alerts {
  appServiceHealth: bool
  httpErrors: bool
  responseTimeSeconds: int?
  alertsGroupName: string
}?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var stagingSlotName = 'deploy'

var vnetIntegrationSubnetRef = vnetLink != null 
  ? resourceId('Microsoft.Network/virtualNetworks/subnets', vnetLink!.vnetName, vnetLink!.subnetName)
  : null

resource appService 'Microsoft.Web/sites@2025-03-01' = {
  name: appServiceName
  kind: kind
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  tags: union(tagValues, {
    ServiceType: 'App Service'
  })
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    reserved: operatingSystem == 'Linux'
    virtualNetworkSubnetId: vnetIntegrationSubnetRef ?? ''
    siteConfig: {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: 'v10.0'
      alwaysOn: true
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
      connectionStrings: connectionStrings
      healthCheckPath: healthCheckPath
      cors: {
        allowedOrigins: allowedOrigins
      }
    }
  }
}

var baseSettings = union(applicationAppSettings, {
  APPINSIGHTS_INSTRUMENTATIONKEY: reference(
    resourceId('Microsoft.Insights/components', appInsightsName),
    '2020-02-02'
  ).InstrumentationKey
  AppInsights__InstrumentationKey: reference(
    resourceId('Microsoft.Insights/components', appInsightsName),
    '2020-02-02'
  ).InstrumentationKey
  WEBSITE_NODE_DEFAULT_VERSION: '22.23.1'
  ASPNETCORE_DETAILEDERRORS: detailedErrors
  WEBSITES_PORT: websitePort

  // Enable the App Service to access file shares over the VNet if
  // file shares are available for this App Service.
  WEBSITE_CONTENTOVERVNET: length(azureFileShares ?? []) > 0 ? '1' : null
}, swapSlotEnabled ? {
  // Use the healthcheck endpoint to identify when a slot is warmed up.
  WEBSITE_SWAP_WARMUP_PING_PATH: healthCheckPath
  WEBSITE_SWAP_WARMUP_PING_STATUSES: '200'
} : {})

var osSpecificSettings = union(baseSettings,
  kind != 'app,linux,container' ? {
    WEBSITE_RUN_FROM_PACKAGE: '1'
  } : {},
  operatingSystem == 'Windows' ? {
    WEBSITE_LOAD_CERTIFICATES: '*'
  } : {}
)

// Existing settings take precedence over settings computed in this Bicep file so that
// infrastructure deploys do not reset application-specific appsettings that have been
// deployed by the application deploy pipeline.
var combinedProdSettings = union(osSpecificSettings, existingProdAppSettings)
var combinedStagingSlotSettings = union(osSpecificSettings, existingStagingSlotAppSettings)

resource appSettings 'Microsoft.Web/sites/config@2025-03-01' = {
  parent: appService
  name: 'appsettings'
  properties: combinedProdSettings
}

module appServiceSecretsUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?secretsUser ?? false) {
  name: '${appServiceName}KeyVaultSecretsUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName 
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultSecretsUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appService.identity.principalId]
    role: 'Secrets User'
  }
}

module appServiceCertificateUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?certificateUser ?? false) {
  name: '${appServiceName}KeyVaultCertificateUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName 
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultCertificateUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appService.identity.principalId]
    role: 'Certificate User'
  }
}

module appServiceSecretsOfficerRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?secretsOfficer ?? false) {
  name: '${appServiceName}KeyVaultSecretsOfficerRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultSecretsOfficer), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appService.identity.principalId]
    role: 'Secrets Officer'
  }
}

module appServiceCryptoUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?cryptoUser ?? false) {
  name: '${appServiceName}KeyVaultCryptoUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultCryptoUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appService.identity.principalId]
    role: 'Crypto User'
  }
}

module stagingSlotModule 'swap-slot.bicep' = if (swapSlotEnabled) {
  name: '${appServiceName}${stagingSlotName}Deploy'
  params: {
    appServiceName: appService.name
    kind: kind
    operatingSystem: operatingSystem
    slotName: stagingSlotName
    appServicePlanId: appServicePlanId
    minTlsVersion: minTlsVersion
    vnetLink: vnetLink
    tagValues: tagValues
    healthCheckPath: healthCheckPath
    connectionStrings: connectionStrings
  }
}

resource stagingSlotAppSettings 'Microsoft.Web/sites/slots/config@2025-03-01' = if (swapSlotEnabled) {
  name: '${appServiceName}/${stagingSlotName}/appsettings'
  properties: combinedStagingSlotSettings
  dependsOn: [
    stagingSlotModule
  ]
}

module stagingSlotSecretsUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (swapSlotEnabled && (keyVaultRoles.?secretsUser ?? false)) {
  name: '${appServiceName}StagingSlotKeyVaultSecretsUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    principalIds: [stagingSlotModule!.outputs.slotIdentityPrincipalId]
    role: 'Secrets User'
  }
}

module stagingSlotCertificateUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (swapSlotEnabled && (keyVaultRoles.?certificateUser ?? false)) {
  name: '${appServiceName}StagingSlotKeyVaultCertificateUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    principalIds: [stagingSlotModule!.outputs.slotIdentityPrincipalId]
    role: 'Certificate User'
  }
}

module autoscaleSettingsModule 'autoscale-settings.bicep' = {
  name: '${appServiceName}AutoscaleSettingsDeploy'
  params: {
    appServiceName: appService.name
    appServicePlanId: appServicePlanId
    autoscaleEnabled: autoscaleEnabled
  }
}

module azureStorageAccountsConfigModule '../storage/file-share-mounts-for-site.bicep' = {
  name: '${appServiceName}StorageAccountsConfigDeploy'
  params: {
    siteName: appServiceName
    azureFileShares: azureFileShares
  }
}

module alertsModule 'alerts.bicep' = if (alerts != null) {
  name: '${appServiceName}AlertsDeploy'
  params: {
    appServiceName: appServiceName
    alerts: alerts!
    tagValues: tagValues
  }  
}

output appServiceName string = appService.name
output appServiceSystemIdentityId string = appService.identity.principalId

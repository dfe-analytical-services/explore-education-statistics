import { HttpStatusAlertConfig } from '../alerts/httpStatusAlertConfig.bicep'
import { ConnectionString, FirewallRule } from '../../types.bicep'
import { AzureFileShareMount } from '../storage/types.bicep'
import { builtInRoleDefinitionIds } from '../../builtInRoles.bicep'

@description('Name of the App Service.')
param appServiceName string

@description('Name of the App Insights instance that this App Service is connected to.')
param appInsightsName string

@description('The operating system to use to host App Services.')
param operatingSystem 'Windows' | 'Linux'

@description('''The kind of App Service to create. Use "app" with the "Windows" operating system for code on Windows,
"app,linux" with "Linux" for code on Linux, and "app,linux,container" with "Linux" for Docker.''')
param kind 'app' | 'app,linux' | 'app,linux,container'

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

@description('The name of an existing user-assigned managed identity for the App Service and its deploy slot to use instead of system-assigned ones, for Key Vault references and the app code. Its Key Vault roles are granted to this identity.')
param userAssignedIdentityName string = ''

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

@description('''
Inbound firewall rules for this App Service. Applied to the production site only, not to the deploy slot, so
that deployments and slot swaps are unaffected.
''')
param firewallRules FirewallRule[] = []

@description('''
The action to take for inbound requests matching none of the "firewallRules". Leave unset to use the Azure
default of allowing unmatched requests.
''')
param ipSecurityRestrictionsDefaultAction ('Allow' | 'Deny')?

@description('File Shares to mount on this App Service and its slots.')
param azureFileShares AzureFileShareMount[]?

@description('Specific port to serve site traffic from.')
param websitePort int?

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param alerts {
  appServiceHealth: bool
  httpErrors: bool
  http401: HttpStatusAlertConfig?
  http5xx: HttpStatusAlertConfig?
  responseTimeSeconds: int?
  alertsGroupName: string
}?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var stagingSlotName = 'deploy'

var vnetIntegrationSubnetRef = vnetLink != null 
  ? resourceId('Microsoft.Network/virtualNetworks/subnets', vnetLink!.vnetName, vnetLink!.subnetName)
  : null

var ipSecurityRestrictions = [
  for (firewallRule, index) in firewallRules: {
    name: firewallRule.name
    ipAddress: firewallRule.cidr
    action: 'Allow'
    tag: firewallRule.tag != null ? firewallRule.tag : 'Default'
    priority: firewallRule.priority != null ? firewallRule.priority : 100 + index
    headers: firewallRule.?headers
  }
]

// Code on Linux takes its runtime stack from linuxFxVersion, and code on Windows from netFrameworkVersion. Docker App
// Services get their image in linuxFxVersion from the application release pipeline, so this leaves it unset for them.
var runtimeStackConfig = kind == 'app,linux' ? { linuxFxVersion: 'DOTNETCORE|10.0' } : { netFrameworkVersion: 'v10.0' }

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (!empty(userAssignedIdentityName)) {
  name: userAssignedIdentityName
}

var identity = !empty(userAssignedIdentityName)
  ? {
      type: 'UserAssigned'
      userAssignedIdentities: {
        '${userAssignedIdentity.id}': {}
      }
    }
  : {
      type: 'SystemAssigned'
    }

resource appService 'Microsoft.Web/sites@2025-03-01' = {
  name: appServiceName
  kind: kind
  location: resourceGroup().location
  identity: identity
  tags: union(tagValues, {
    ServiceType: 'App Service'
  })
  properties: union({
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    reserved: operatingSystem == 'Linux'
    virtualNetworkSubnetId: vnetIntegrationSubnetRef ?? ''
    siteConfig: union(runtimeStackConfig, {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      alwaysOn: true
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
      connectionStrings: connectionStrings
      healthCheckPath: healthCheckPath
      ipSecurityRestrictions: length(ipSecurityRestrictions) > 0 ? ipSecurityRestrictions : null
      ipSecurityRestrictionsDefaultAction: ipSecurityRestrictionsDefaultAction
      cors: {
        allowedOrigins: allowedOrigins
      }
    })
  }, !empty(userAssignedIdentityName) ? { keyVaultReferenceIdentity: userAssignedIdentity.id } : {})
}

// The identity that the App Service uses, to which its roles are granted.
var appServicePrincipalId = !empty(userAssignedIdentityName)
  ? userAssignedIdentity!.properties.principalId
  : appService.identity.principalId

var infraAppSettings = union({
  APPINSIGHTS_INSTRUMENTATIONKEY: reference(
    resourceId('Microsoft.Insights/components', appInsightsName),
    '2020-02-02'
  ).InstrumentationKey
  AppInsights__InstrumentationKey: reference(
    resourceId('Microsoft.Insights/components', appInsightsName),
    '2020-02-02'
  ).InstrumentationKey
  WEBSITE_NODE_DEFAULT_VERSION: '22.23.4'
  ASPNETCORE_DETAILEDERRORS: detailedErrors
  WEBSITES_PORT: websitePort

  // Enable the App Service to access file shares over the VNet if
  // file shares are available for this App Service.
  WEBSITE_CONTENTOVERVNET: length(azureFileShares ?? []) > 0 ? '1' : null
}, swapSlotEnabled ? {
  // Use the healthcheck endpoint to identify when a slot is warmed up.
  WEBSITE_SWAP_WARMUP_PING_PATH: healthCheckPath
  WEBSITE_SWAP_WARMUP_PING_STATUSES: '200'
} : {}, kind != 'app,linux,container' ? {
  WEBSITE_RUN_FROM_PACKAGE: '1'
} : {}, operatingSystem == 'Windows' ? {
  WEBSITE_LOAD_CERTIFICATES: '*'
} : {}, !empty(userAssignedIdentityName) ? {
  // Picked up by DefaultAzureCredential in the app code.
  AZURE_CLIENT_ID: userAssignedIdentity!.properties.clientId
} : {})

// Combine any appsettings supplied by the caller of this template along with any appsettings
// under the control of the application deploy pipeline, and finally with the infrastructure
// appsettings from this template.  This ensures all appsettings are considered and retained,
// and allows this infrastructure deploy to update its infrastructure appsetting changes,
// winning over any conflicts.
var combinedProdSettings = union(union(applicationAppSettings, existingProdAppSettings), infraAppSettings)
var combinedStagingSlotSettings = union(union(applicationAppSettings, existingStagingSlotAppSettings), infraAppSettings)

resource appSettings 'Microsoft.Web/sites/config@2025-03-01' = {
  parent: appService
  name: 'appsettings'
  properties: combinedProdSettings
}

module appServiceSecretsUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?secretsUser ?? false) {
  name: '${appServiceName}KeyVaultSecretsUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName && empty(userAssignedIdentityName)
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultSecretsUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appServicePrincipalId]
    role: 'Secrets User'
  }
}

module appServiceCertificateUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?certificateUser ?? false) {
  name: '${appServiceName}KeyVaultCertificateUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName && empty(userAssignedIdentityName)
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultCertificateUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appServicePrincipalId]
    role: 'Certificate User'
  }
}

module appServiceSecretsOfficerRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?secretsOfficer ?? false) {
  name: '${appServiceName}KeyVaultSecretsOfficerRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName && empty(userAssignedIdentityName)
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultSecretsOfficer), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appServicePrincipalId]
    role: 'Secrets Officer'
  }
}

module appServiceCryptoUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (keyVaultRoles.?cryptoUser ?? false) {
  name: '${appServiceName}KeyVaultCryptoUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    roleAssignmentNameOverride: keyVaultRoles!.legacyKeyVaultRoleAssignmentName && empty(userAssignedIdentityName)
      ? guid(resourceId('Microsoft.KeyVault/vaults', keyVaultRoles!.keyVaultName!), subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.KeyVaultCryptoUser), 'Microsoft.Web/sites/${appServiceName}')
      : null
    principalIds: [appServicePrincipalId]
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
    userAssignedIdentityId: !empty(userAssignedIdentityName) ? userAssignedIdentity.id : ''
  }
}

resource stagingSlotAppSettings 'Microsoft.Web/sites/slots/config@2025-03-01' = if (swapSlotEnabled) {
  name: '${appServiceName}/${stagingSlotName}/appsettings'
  properties: combinedStagingSlotSettings
  dependsOn: [
    stagingSlotModule
  ]
}

// Grants for the deploy slot's own system-assigned identity. With a user-assigned identity, the slot shares the App
// Service's identity, which is granted its roles above.
module stagingSlotSecretsUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (swapSlotEnabled && empty(userAssignedIdentityName) && (keyVaultRoles.?secretsUser ?? false)) {
  name: '${appServiceName}StagingSlotKeyVaultSecretsUserRole'
  params: {
    keyVaultName: keyVaultRoles!.keyVaultName!
    principalIds: [stagingSlotModule!.outputs.slotIdentityPrincipalId]
    role: 'Secrets User'
  }
}

module stagingSlotCertificateUserRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = if (swapSlotEnabled && empty(userAssignedIdentityName) && (keyVaultRoles.?certificateUser ?? false)) {
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
output appServiceIdentityPrincipalId string = appServicePrincipalId

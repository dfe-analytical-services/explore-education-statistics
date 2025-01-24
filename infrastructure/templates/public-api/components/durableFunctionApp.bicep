import {
  FirewallRule
  IpRange
  AzureFileShareMount
  EntraIdAuthentication
  StorageAccountPrivateEndpoints
} from '../types.bicep'

import { staticAverageLessThanHundred, staticMinGreaterThanZero } from 'alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan } from 'alerts/dynamicAlertConfig.bicep'
import { abbreviations } from '../abbreviations.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Function App name')
param functionAppName string

@description('Specifies the App Service plan name')
param appServicePlanName string

@description('Specifies the name prefix for all storage accounts')
param storageAccountsNamePrefix string

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('Function App runtime')
param functionAppRuntime 'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' = 'dotnet'

@description('Specifies the additional setting to add to the Function App')
param appSettings object = {}

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the subnet id for the function app outbound traffic across the VNet')
param subnetId string

@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpoints {
  functionApp: string?
  storageAccounts: string
}

@description('Specifies whether this Function App is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@description('An existing Managed Identity\'s Resource Id with which to associate this Function App')
param userAssignedManagedIdentityParams {
  id: string
  name: string
  principalId: string
}?

@description('An existing App Registration registered with Entra ID that will be used to control access to this Function App')
param entraIdAuthentication EntraIdAuthentication?

@description('Specifies the SKU for the Function App hosting plan')
param sku object

@description('Specifies the Key Vault name that this Function App will be permitted to get and list secrets from')
param keyVaultName string

@description('Specifies whether or not the Function App already exists. This is used to determine whether or not to look for existing appsettings')
param functionAppExists bool

@description('Specifies the number of pre-warmed instances for this Function App - must be compatible with the chosen hosting plan')
param preWarmedInstanceCount int?

@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan')
param alwaysOn bool?

@description('Specifies an optional URL for Azure to use to monitor the health of this resource')
param healthCheckPath string?

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

@description('Specifies firewall rules for the various storage accounts in use by the Function App')
param storageFirewallRules IpRange[] = []

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  functionAppHealth: bool
  httpErrors: bool
  cpuPercentage: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
  fileServiceAvailability: bool
  fileServiceLatency: bool
  fileServiceCapacity: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var identity = userAssignedManagedIdentityParams != null
  ? {
      type: 'UserAssigned'
      userAssignedIdentities: {
        '${userAssignedManagedIdentityParams!.id}': {}
      }
    }
  : {
      type: 'SystemAssigned'
    }

module appServicePlanModule 'appServicePlan.bicep' = {
  name: appServicePlanName
  params: {
    planName: appServicePlanName
    location: location
    kind: 'functionapp'
    sku: sku
    operatingSystem: operatingSystem
    alerts: alerts != null
      ? {
          cpuPercentage: alerts!.cpuPercentage
          memoryPercentage: alerts!.memoryPercentage
          alertsGroupName: alerts!.alertsGroupName
        }
      : null
    tagValues: tagValues
  }
}

// Configure a single management storage account for access key storage, and 2 individual storage accounts to be split
// between the production slot and staging slot for reliable execution. See
// https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-zero-downtime-deployment#status-check-with-slot
var managementStorageAccountName = '${storageAccountsNamePrefix}mg'
var slot1StorageAccountName = '${storageAccountsNamePrefix}s1'
var slot2StorageAccountName = '${storageAccountsNamePrefix}s2'
var functionAppCodeFileShareName = '${functionAppName}-${abbreviations.fileShare}'
var keyVaultReferenceIdentity = userAssignedManagedIdentityParams != null ? userAssignedManagedIdentityParams!.id : null

var storageAlerts = alerts != null
  ? {
      availability: alerts!.storageAccountAvailability
      latency: alerts!.storageLatency
      alertsGroupName: alerts!.alertsGroupName
    }
  : null

var fileServiceAlerts = alerts != null
  ? {
      availability: alerts!.fileServiceAvailability
      latency: alerts!.fileServiceLatency
      capacity: alerts!.fileServiceCapacity
      alertsGroupName: alerts!.alertsGroupName
    }
  : null

// This is the shared Storage Account for this Durable Function App that is used for key management, timer trigger
// management etc.  For performance, it is considered good practice for each Function App to have its own dedicated 
// Storage Account. See https://learn.microsoft.com/en-us/azure/azure-functions/storage-considerations?tabs=azure-cli#optimize-storage-performance.
module manamementStorageAccountModule 'storageAccount.bicep' = {
  name: '${managementStorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: managementStorageAccountName
    allowedSubnetIds: [subnetId]
    keyVaultName: keyVaultName
    publicNetworkAccessEnabled: false
    privateEndpointSubnetIds: {
      blob: privateEndpoints.storageAccounts
      queue: privateEndpoints.storageAccounts
      table: privateEndpoints.storageAccounts
    }
    firewallRules: storageFirewallRules
    alerts: storageAlerts
    tagValues: tagValues
  }
}

// This is a storage account dedicated to slot 1. It uses this for its own reliable execution.
// It also contains a file share where its slot-specific version of the code lives.
module slot1StorageAccountModule 'storageAccount.bicep' = {
  name: '${slot1StorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: slot1StorageAccountName
    allowedSubnetIds: [subnetId]
    keyVaultName: keyVaultName
    publicNetworkAccessEnabled: false
    privateEndpointSubnetIds: {
      file: privateEndpoints.storageAccounts
      blob: privateEndpoints.storageAccounts
      queue: privateEndpoints.storageAccounts
      table: privateEndpoints.storageAccounts
    }
    firewallRules: storageFirewallRules
    alerts: storageAlerts
    tagValues: tagValues
  }
}

// This is the file share for slot 1 to use for its code storage.
module slot1FileShareModule 'fileShare.bicep' = {
  name: '${slot1StorageAccountName}${functionAppCodeFileShareName}Deploy'
  params: {
    storageAccountName: slot1StorageAccountName
    fileShareName: functionAppCodeFileShareName
    alerts: fileServiceAlerts
    tagValues: tagValues
  }
  dependsOn: [
    slot1StorageAccountModule
  ]
}

// This is a storage account dedicated to slot 2. It uses this for its own reliable execution.
// It also contains a file share where its slot-specific version of the code lives.
module slot2StorageAccountModule 'storageAccount.bicep' = {
  name: '${slot2StorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: slot2StorageAccountName
    allowedSubnetIds: [subnetId]
    keyVaultName: keyVaultName
    publicNetworkAccessEnabled: false
    privateEndpointSubnetIds: {
      file: privateEndpoints.storageAccounts
      blob: privateEndpoints.storageAccounts
      queue: privateEndpoints.storageAccounts
      table: privateEndpoints.storageAccounts
    }
    firewallRules: storageFirewallRules
    alerts: storageAlerts
    tagValues: tagValues
  }
}

// This is the file share for slot 2 to use for its code storage.
module slot2FileShareModule 'fileShare.bicep' = {
  name: '${slot2StorageAccountName}${functionAppCodeFileShareName}Deploy'
  params: {
    storageAccountName: slot2StorageAccountName
    fileShareName: functionAppCodeFileShareName
    alerts: fileServiceAlerts
    tagValues: tagValues
  }
  dependsOn: [
    slot2StorageAccountModule
  ]
}

var firewallRules = [
  for (firewallRule, index) in functionAppFirewallRules: {
    name: firewallRule.name
    ipAddress: firewallRule.cidr
    action: 'Allow'
    tag: firewallRule.tag != null ? firewallRule.tag : 'Default'
    priority: firewallRule.priority != null ? firewallRule.priority : 100 + index
  }
]

var commonSiteProperties = {
  enabled: true
  httpsOnly: true
  serverFarmId: appServicePlanModule.outputs.planId

  // This property integrates the Function App into a VNet given the supplied subnet id.
  virtualNetworkSubnetId: subnetId

  clientAffinityEnabled: true
  reserved: operatingSystem == 'Linux'
  siteConfig: {
    alwaysOn: alwaysOn ?? null
    healthCheckPath: healthCheckPath
    preWarmedInstanceCount: preWarmedInstanceCount ?? null
    netFrameworkVersion: '8.0'
    linuxFxVersion: operatingSystem == 'Linux' ? 'DOTNET-ISOLATED|8.0' : null
    keyVaultReferenceIdentity: keyVaultReferenceIdentity
    minTlsVersion: '1.3'
    publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
    ipSecurityRestrictions: publicNetworkAccessEnabled && length(firewallRules) > 0 ? firewallRules : null
    ipSecurityRestrictionsDefaultAction: 'Deny'
    // TODO EES-5446 - this setting controls access to the deploy site for the Function App.
    // This is currently the default value, but ideally we would lock this down to only be accessible
    // by our runners and certain other whitelisted IP address ranges (e.g. trusted VPNs).
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
  }
  keyVaultReferenceIdentity: keyVaultReferenceIdentity
}

// Create the main production deploy slot.
resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: identity
  properties: commonSiteProperties
  tags: tagValues
}

// Create the staging deploy slot.
resource stagingSlot 'Microsoft.Web/sites/slots@2023-12-01' = {
  name: 'staging'
  parent: functionApp
  location: location
  identity: identity
  properties: commonSiteProperties
  tags: tagValues
}

module azureAuthentication 'siteAzureAuthentication.bicep' = if (entraIdAuthentication != null) {
  name: '${functionAppName}AzureAuthentication'
  params: {
    clientId: entraIdAuthentication!.appRegistrationClientId
    siteName: functionAppName
    stagingSlotName: stagingSlot.name
    allowedClientIds: entraIdAuthentication!.allowedClientIds
    allowedPrincipalIds: entraIdAuthentication!.allowedPrincipalIds
    requireAuthentication: entraIdAuthentication!.requireAuthentication
  }
}

// Allow Key Vault references passed as secure appsettings to be resolved by the Function App and its deployment slots.
// Where the staging slot's managed identity differs from the main slot's managed identity, add its id to the list.
var keyVaultPrincipalIds = userAssignedManagedIdentityParams != null
  ? [userAssignedManagedIdentityParams!.principalId]
  : [functionApp.identity.principalId, stagingSlot.identity.principalId]

// TODO EES-5382 - remove when the switch over the Key Vault RBAC is enabled.
module functionAppKeyVaultAccessPolicy 'keyVaultAccessPolicy.bicep' = {
  name: '${functionAppName}FunctionAppKeyVaultAccessPolicy'
  params: {
    keyVaultName: keyVaultName
    principalIds: keyVaultPrincipalIds
  }
}

module functionAppKeyVaultRoleAssignments 'keyVaultRoleAssignment.bicep' = {
  name: '${functionAppName}FunctionAppKeyVaultRoleAssignment'
  params: {
    keyVaultName: keyVaultName
    principalIds: keyVaultPrincipalIds
    role: 'Secrets User'
  }
}

resource azureStorageAccountsConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'azurestorageaccounts'
  parent: functionApp
  properties: reduce(
    azureFileShares,
    {},
    (cur, next) =>
      union(cur, {
        '${next.storageName}': {
          type: 'AzureFiles'
          shareName: next.fileShareName
          mountPath: next.mountPath
          accountName: next.storageAccountName
          accessKey: next.storageAccountKey
        }
      })
  )
}

// We determine any pre-existing appsettings for both the production and the staging slots during this infrastructure
// deploy and supply them as the most important appsettings. This prevents infrastructure deploys from overriding any
// appsettings back to their original values by allowing existing ones to take precedence.
//
// See https://blog.dotnetstudio.nl/posts/2021/04/merge-appsettings-with-bicep.
var existingStagingAppSettings = functionAppExists
  ? list(resourceId('Microsoft.Web/sites/slots/config', functionApp.name, 'staging', 'appsettings'), '2021-03-01').properties
  : {}
var existingProductionAppSettings = functionAppExists
  ? list(resourceId('Microsoft.Web/sites/config', functionApp.name, 'appsettings'), '2021-03-01').properties
  : {}

// Create staging and production deploy slots, and set base app settings on both.
// These will be infrastructure-specific appsettings, and the YAML pipeline will handle the deployment of
// application-specific appsettings so as to be able to control the rollout of new, updated and deleted
// appsettings to the correct swap slots.
module functionAppSlotSettings 'appServiceSlotConfig.bicep' = {
  name: '${functionAppName}AppServiceSlotConfigDeploy'
  params: {
    appName: functionApp.name
    existingStagingAppSettings: existingStagingAppSettings
    existingProductionAppSettings: existingProductionAppSettings
    slotSpecificSettingKeys: [
      // This value is sticky to its individual slot and will not swap when slot swapping occurs.
      // This "SLOT_NAME" configuration value is merely to help enable debugging and checking which
      // site is being viewed.
      'SLOT_NAME'
    ]
    commonSettings: union(appSettings, {
      // This tells the Function App where to store its "azure-webjobs-hosts" and "azure-webjobs-secrets" files.
      AzureWebJobsStorage: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${manamementStorageAccountModule.outputs.connectionStringSecretName})'

      // These 2 properties indicate that the traffic which pulls down the deployment code for the Function App
      // from Storage should go over the VNet and find their code in file shares within their linked Storage Account.
      WEBSITE_CONTENTOVERVNET: 1
      vnetContentShareEnabled: true

      // This property instructs the Function App to direct all outbound traffic over the VNet.
      WEBSITE_VNET_ROUTE_ALL: 1

      // This setting is necessary in order to allow slot swapping to work without complaining that
      // "Storage volume is currently in R/O mode".
      // See https://learn.microsoft.com/en-us/azure/azure-functions/functions-app-settings#website_override_sticky_diagnostics_settings.
      WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS: 0

      // This value puts the Function App filesystem into readonly mode and runs code from a ZIP deployment.
      WEBSITE_RUN_FROM_PACKAGE: '1'

      // This is set by the Azure ZIP deploy commands in the YAML pipeline, so it is included here to prevent any
      // unnecessary changes when a code deploy is pushed out.
      SCM_DO_BUILD_DURING_DEPLOYMENT: false

      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: functionAppRuntime
      APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsightsKey

      // This indicates the name of the file share where the Function App code and configuration lives.
      WEBSITE_CONTENTSHARE: functionAppCodeFileShareName

      // It's only possible to UPDATE a Function App using a Key Vault reference for WEBSITE_CONTENTAZUREFILECONNECTIONSTRING setting,
      // not creating a new Function App, unless we use the below setting to skip validation for the first time we create
      // this Function App.  See https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references?tabs=azure-cli#considerations-for-azure-files-mounting.
      WEBSITE_SKIP_CONTENTSHARE_VALIDATION: functionAppExists ? 0 : 1
    })
    stagingOnlySettings: {
      SLOT_NAME: 'staging'
      DurableManagementStorage: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${slot1StorageAccountModule.outputs.connectionStringSecretName})'

      // The following property tell the Function App slot that its deployment code file share (as identified by the WEBSITE_CONTENTSHARE setting)
      // resides in the specified Storage account.
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${slot1StorageAccountModule.outputs.connectionStringSecretName})'
    }
    prodOnlySettings: {
      SLOT_NAME: 'production'
      DurableManagementStorage: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${slot2StorageAccountModule.outputs.connectionStringSecretName})'

      // The following property tell the Function App slot that its deployment code file share (as identified by the WEBSITE_CONTENTSHARE setting)
      // resides in the specified Storage account.
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${slot2StorageAccountModule.outputs.connectionStringSecretName})'
    }
    azureFileShares: azureFileShares
  }
  dependsOn: [
    functionAppKeyVaultRoleAssignments
    slot1FileShareModule
    slot2FileShareModule
  ]
}

module privateEndpointModule 'privateEndpoint.bicep' = if (privateEndpoints.functionApp != null) {
  name: '${functionAppName}PrivateEndpointDeploy'
  params: {
    serviceId: functionApp.id
    serviceName: functionApp.name
    serviceType: 'sites'
    subnetId: privateEndpoints.functionApp!
    location: location
    tagValues: tagValues
  }
}

module healthAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.functionAppHealth) {
  name: '${functionAppName}HealthAlertModule'
  params: {
    resourceName: functionAppName
    resourceMetric: {
      resourceType: 'Microsoft.Web/sites'
      metric: 'HealthCheckStatus'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'health'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

var unexpectedHttpStatusCodeMetrics = ['Http401', 'Http5xx']

module unexpectedHttpStatusCodeAlerts 'alerts/staticMetricAlert.bicep' = [
  for httpStatusCode in unexpectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${functionAppName}${httpStatusCode}Module'
    params: {
      resourceName: functionAppName
      resourceMetric: {
        resourceType: 'Microsoft.Web/sites'
        metric: httpStatusCode
      }
      config: {
        ...staticMinGreaterThanZero
        nameSuffix: toLower(httpStatusCode)
      }
      alertsGroupName: alerts!.alertsGroupName
      tagValues: tagValues
    }
  }
]

var expectedHttpStatusCodeMetrics = ['Http403', 'Http4xx']

module expectedHttpStatusCodeAlerts 'alerts/dynamicMetricAlert.bicep' = [
  for httpStatusCode in expectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${functionAppName}${httpStatusCode}Module'
    params: {
      resourceName: functionAppName
      resourceMetric: {
        resourceType: 'Microsoft.Web/sites'
        metric: httpStatusCode
      }
      config: {
        ...dynamicAverageGreaterThan
        nameSuffix: toLower(httpStatusCode)
        severity: 'Informational'
      }
      alertsGroupName: alerts!.alertsGroupName
      tagValues: tagValues
    }
  }
]

output functionAppName string = functionApp.name
output url string = 'https://${functionApp.name}.azurewebsites.net'
output stagingUrl string = 'https://${functionApp.name}-staging.azurewebsites.net'
output managementStorageAccountName string = managementStorageAccountName
output slot1StorageAccountName string = slot1StorageAccountName
output slot2StorageAccountName string = slot2StorageAccountName

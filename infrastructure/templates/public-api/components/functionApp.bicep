@description('Specifies the subscription name - used for creating a shorter name for Storage Accounts')
param subscription string

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Function App name')
param functionAppName string

@description('Function App Plan : operating system')
@allowed([
  'Windows'
  'Linux'
])
param appServicePlanOS string = 'Linux'

@description('Function App runtime')
@allowed([
  'dotnet'
  'dotnet-isolated'
  'node'
  'python'
  'java'
])
param functionAppRuntime string = 'dotnet'

@description('Specifies the additional setting to add to the functionapp.')
param settings object = {}

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the subnet id')
param subnetId string

@description('Specifies whether this Function App is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

@description('An existing Managed Identity\'s Resource Id with which to associate this Function App')
param userAssignedManagedIdentityParams {
  id: string
  name: string
  principalId: string
}?

@description('An existing App Registration registered with Entra ID that will be used to control access to this Function App')
param entraIdAuthentication {
  appRegistrationClientId: string
  allowedClientIds: string[]
  allowedPrincipalIds: string[]
}?

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

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}[] = []

@description('Specifies firewall rules for the various storage accounts in use by the Function App')
param storageFirewallRules {
  name: string
  cidr: string
}[] = []

var appServicePlanName = '${resourcePrefix}-asp-${functionAppName}'
var reserved = appServicePlanOS == 'Linux'
var fullFunctionAppName = '${subscription}-ees-papi-fa-${functionAppName}'

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

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: sku
  properties: {
    reserved: reserved
  }
}

// Configure a single shared storage account for access key storage, and 2 individual storage accounts to be split
// between the production slot and staging slot for reliable execution. See
// https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-zero-downtime-deployment#status-check-with-slot
var sharedStorageAccountName = replace('${subscription}eessa${functionAppName}mg', '-', '')
var slot1StorageAccountName = replace('${subscription}eessa${functionAppName}s1', '-', '')
var slot2StorageAccountName = replace('${subscription}eessa${functionAppName}s2', '-', '')
var functionAppCodeFileShareName = '${fullFunctionAppName}-fs'
var keyVaultReferenceIdentity = userAssignedManagedIdentityParams != null ? userAssignedManagedIdentityParams!.id : null

// This is the shared Storage Account for this Durable Function App that is used for key management, timer trigger
// management etc.
//
// The Durable Function App is granted access by whitelisting its subnet for inbound traffic.
//
// For performance, it is considered good practice for each Function App to have its own dedicated Storage Account. See
// https://learn.microsoft.com/en-us/azure/azure-functions/storage-considerations?tabs=azure-cli#optimize-storage-performance.

// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
module sharedStorageAccountModule 'storageAccount.bicep' = {
  name: '${sharedStorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: sharedStorageAccountName
    allowedSubnetIds: [subnetId]
    skuStorageResource: 'Standard_LRS'
    keyVaultName: keyVaultName
    firewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

// This is a storage account dedicated to slot 1. It uses this for its own reliable execution.
// It also contains a file share where its slot-specific version of the code lives.
// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
module slot1StorageAccountModule 'storageAccount.bicep' = {
  name: '${slot1StorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: slot1StorageAccountName
    allowedSubnetIds: [subnetId]
    skuStorageResource: 'Standard_LRS'
    keyVaultName: keyVaultName
    firewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

// This is the file share for slot 1 to use for its code storage.
resource slot1FileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${slot1StorageAccountName}/default/${functionAppCodeFileShareName}'
  dependsOn: [
    slot1StorageAccountModule
  ]
}

// This is a storage account dedicated to slot 2. It uses this for its own reliable execution.
// It also contains a file share where its slot-specific version of the code lives.
// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
module slot2StorageAccountModule 'storageAccount.bicep' = {
  name: '${slot2StorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: slot2StorageAccountName
    allowedSubnetIds: [subnetId]
    skuStorageResource: 'Standard_LRS'
    keyVaultName: keyVaultName
    firewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

// This is the file share for slot 2 to use for its code storage.
resource slot2FileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${slot2StorageAccountName}/default/${functionAppCodeFileShareName}'
  dependsOn: [
    slot2StorageAccountModule
  ]
}

var commonSiteProperties = {
  enabled: true
  httpsOnly: true
  serverFarmId: appServicePlan.id
  
  // This property integrates the Function App into a VNet given the supplied subnet id.
  virtualNetworkSubnetId: subnetId
  
  clientAffinityEnabled: true
  reserved: reserved
  siteConfig: {
    alwaysOn: alwaysOn ?? null
    preWarmedInstanceCount: preWarmedInstanceCount ?? null
    netFrameworkVersion: '8.0'
    linuxFxVersion: appServicePlanOS == 'Linux' ? 'DOTNET-ISOLATED|8.0' : null
    keyVaultReferenceIdentity: keyVaultReferenceIdentity
  }
  keyVaultReferenceIdentity: keyVaultReferenceIdentity
  publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
}

// Create the main production deploy slot.
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: fullFunctionAppName
  location: location
  kind: 'functionapp'
  identity: identity
  properties: commonSiteProperties
  tags: tagValues
}

// Create the staging deploy slot.
resource stagingSlot 'Microsoft.Web/sites/slots@2023-01-01' = {
  name: 'staging'
  parent: functionApp
  location: location
  identity: identity
  properties: commonSiteProperties
  tags: tagValues
}

var authSettingsV2Properties = entraIdAuthentication == null ? {} : {
  globalValidation: {
    requireAuthentication: true
    unauthenticatedClientAction: 'Return401'
  }
  httpSettings: {
    requireHttps: true
  }
  identityProviders: {
    azureActiveDirectory: {
      enabled: true
      registration: {
        clientId: entraIdAuthentication!.appRegistrationClientId
        openIdIssuer: 'https://sts.windows.net/${tenant().tenantId}/v2.0'
      }
      validation: {
        allowedAudiences: [
          'api://${entraIdAuthentication!.appRegistrationClientId}'
        ]
        defaultAuthorizationPolicy: {
          allowedApplications: union(
            [entraIdAuthentication!.appRegistrationClientId],
            entraIdAuthentication!.allowedClientIds
          )
          allowedPrincipals: {
            identities: entraIdAuthentication!.allowedPrincipalIds
          }
        }
      }
    }
  }
  platform: {
    enabled: true
    runtimeVersion: '~1'
  }
}

resource functionAppAuthSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'authsettingsV2'
  parent: functionApp
  properties: authSettingsV2Properties
}

resource stagingSlotAuthSettings 'Microsoft.Web/sites/slots/config@2022-03-01' = {
  name: 'authsettingsV2'
  parent: stagingSlot
  properties: authSettingsV2Properties
}

// Allow Key Vault references passed as secure appsettings to be resolved by the Function App and its deployment slots.
// Where the staging slot's managed identity differs from the main slot's managed identity, add its id to the list.
var keyVaultPrincipalIds = userAssignedManagedIdentityParams != null
  ? [userAssignedManagedIdentityParams!.principalId]
  : [functionApp.identity.principalId, stagingSlot.identity.principalId]

module functionAppKeyVaultAccessPolicy 'keyVaultAccessPolicy.bicep' = {
  name: '${functionAppName}FunctionAppKeyVaultAccessPolicy'
  params: {
    keyVaultName: keyVaultName
    principalIds: keyVaultPrincipalIds
  }
}

resource azureStorageAccountsConfig 'Microsoft.Web/sites/config@2023-12-01' = {
   name: 'azurestorageaccounts'
   parent: functionApp
   properties: reduce(azureFileShares, {}, (cur, next) => union(cur, {
     '${next.storageName}': {
       type: 'AzureFiles'
       shareName: next.fileShareName
       mountPath: next.mountPath
       accountName: next.storageAccountName
       accessKey: next.storageAccountKey
     }
   }))
}

// We determine any pre-existing appsettings for both the production and the staging slots during this infrastructure
// deploy and supply them as the most important appsettings. This prevents infrastructure deploys from overriding any
// appsettings back to their original values by allowing existing ones to take precedence.
//
// See https://blog.dotnetstudio.nl/posts/2021/04/merge-appsettings-with-bicep.
var existingStagingAppSettings = functionAppExists ? list(resourceId('Microsoft.Web/sites/slots/config', functionApp.name, 'staging', 'appsettings'), '2021-03-01').properties : {}
var existingProductionAppSettings = functionAppExists ? list(resourceId('Microsoft.Web/sites/config', functionApp.name, 'appsettings'), '2021-03-01').properties : {}

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
    commonSettings: union(settings, {

      // This tells the Function App where to store its "azure-webjobs-hosts" and "azure-webjobs-secrets" files.
      AzureWebJobsStorage: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${sharedStorageAccountModule.outputs.connectionStringSecretName})'

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
    functionAppKeyVaultAccessPolicy
    slot1FileShare
    slot2FileShare
  ]
}

output functionAppName string = functionApp.name

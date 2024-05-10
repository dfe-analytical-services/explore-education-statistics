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
param additionalAzureFileStorage {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}?

var appServicePlanName = '${resourcePrefix}-asp-${functionAppName}'
var reserved = appServicePlanOS == 'Linux'
var fullFunctionAppName = '${subscription}-ees-papi-fa-${functionAppName}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: sku
  properties: {
    reserved: reserved
  }
}

var dedicatedStorageAccountName = replace('${subscription}eessa${functionAppName}', '-', '')

// Create a dedicated Storage Account for this Function App for it to store its deployment code and jobs in.
// Grant the Function App access by whitelisting its subnet for inbound traffic.
//
// For performance, it is considered good practice for each Function App to have its own dedicated Storage Account. See
// https://learn.microsoft.com/en-us/azure/azure-functions/storage-considerations?tabs=azure-cli#optimize-storage-performance.
resource dedicatedStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: dedicatedStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

var dedicatedStorageAccountString = 'DefaultEndpointsProtocol=https;AccountName=${dedicatedStorageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${dedicatedStorageAccount.listKeys().keys[0].value}'

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: fullFunctionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
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
    }
  }
  tags: tagValues
}

resource azureStorageAccount 'Microsoft.Web/sites/config@2021-01-15' = if (additionalAzureFileStorage != null) {
   name: 'azurestorageaccounts'
   parent: functionApp
   properties: {
     '${additionalAzureFileStorage!.storageName}': {
       type: 'AzureFiles'
       shareName: additionalAzureFileStorage!.fileShareName
       mountPath: additionalAzureFileStorage!.mountPath
       accountName: additionalAzureFileStorage!.storageAccountName
       accessKey: additionalAzureFileStorage!.storageAccountKey
     }
   }
}

// We determine any pre-existing appsettings for both the production and the staging slots during this infrastructure
// deploy and supply them as the most important appsettings. This prevents infrastructure deploys from overriding any
// appsettings back to their original values by allowing existing ones to take precedence.
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
    location: location
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
      AzureWebJobsStorage: dedicatedStorageAccountString

      // This property tells the Function App that the deployment code resides in this Storage account.
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: dedicatedStorageAccountString

      WEBSITE_CONTENTSHARE: fullFunctionAppName

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
    })
    stagingOnlySettings: {
      SLOT_NAME: 'staging'
    }
    prodOnlySettings: {
      SLOT_NAME: 'production'
    }
    tagValues: tagValues
  }
}

// Allow Key Vault references passed as secure appsettings to be resolved by the Function App and its deployment slots.
module functionAppKeyVaultAccessPolicy 'keyVaultAccessPolicy.bicep' = {
  name: '${functionAppName}FunctionAppKeyVaultAccessPolicy'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      functionApp.identity.principalId
      functionAppSlotSettings.outputs.stagingSlotPrincipalId
    ]
  }
}

output functionAppName string = functionApp.name
output principalId string = functionApp.identity.principalId
output tenantId string = functionApp.identity.tenantId

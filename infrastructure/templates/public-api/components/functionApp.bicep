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
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          action: 'Allow'
          id: subnetId
        }
      ]
    }
  }
}

var dedicatedStorageAccountString = 'DefaultEndpointsProtocol=https;AccountName=${dedicatedStorageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${dedicatedStorageAccount.listKeys().keys[0].value}'

// When deploying to a Function App utilising a secured VNet to store its deployment files,
// unique file shares must be pre-generated and unique to each slot prior to deploying the
// Function App itself if we wish to use slot swapping.
//
// See the second paragraph of https://learn.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code?tabs=json%2Clinux%2Cdevops&pivots=premium-plan#secured-deployments.
var fileShareName1 = '${toLower(fullFunctionAppName)}-1'
var fileShareName2 = '${toLower(fullFunctionAppName)}-2'

resource fileShare1 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${dedicatedStorageAccountName}/default/${fileShareName1}'
  dependsOn: [
    dedicatedStorageAccount
  ]
}

resource fileShare2 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${dedicatedStorageAccountName}/default/${fileShareName2}'
  dependsOn: [
    dedicatedStorageAccount
  ]
}

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

      // These 2 properties indicate that the traffic which pulls down the deployment code for the Function App
      // from Storage should go over the VNet and find their code in file shares within their linked Storage Account.
      WEBSITE_CONTENTOVERVNET: 1
      vnetContentShareEnabled: true

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
      // When deploying to a Function App utilising a secured VNet to store its deployment files,
      // unique file shares must be pre-generated and unique to each slot prior to deploying the
      // Function App itself if we wish to use slot swapping.
      //
      // In conjunction with WEBSITE_CONTENTAZUREFILECONNECTIONSTRING, this property tells the
      // Function App that the deployment code resides in this Storage account and within *this*
      // file share.
      //
      // See the second paragraph of https://learn.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code?tabs=json%2Clinux%2Cdevops&pivots=premium-plan#secured-deployments.
      //
      // When slots are swapped, this fileshare configuration value will swap with the slots and thereby make the new
      // code deployment location available to the other slot.
      WEBSITE_CONTENTSHARE: fileShareName2
    }
    prodOnlySettings: {
      SLOT_NAME: 'production'
      // As above, this value is distinct from the initial staging slot equivalent and will swap to the other slot upon
      // a slot swap operation.
      WEBSITE_CONTENTSHARE: fileShareName1
    }
    tagValues: tagValues
  }
  dependsOn: [
    fileShare1
    fileShare2
  ]
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

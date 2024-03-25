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
param settings object

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the subnet id')
param subnetId string

@description('Specifies the SKU for the Function App hosting plan')
param sku object

// param existingFileShareNameStaging string
// param existingFileShareNameProduction string
param functionAppExists boolean

var appServicePlanName = '${resourcePrefix}-asp-${functionAppName}'
var reserved = appServicePlanOS == 'Linux' ? true : false
var functionName = 'test-fa-${functionAppName}'
var fileShareName = toLower(functionAppName)
var fileShareNameProduction = '${fileShareName}-1'// !empty(existingFileShareNameProduction) ? existingFileShareNameProduction : '${fileShareName}-1'
var fileShareNameStaging = '${fileShareName}-2'// !empty(existingFileShareNameStaging) ? existingFileShareNameStaging : '${fileShareName}-2'


resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: sku
  properties: {
    reserved: reserved
  }
}

var storageAccountName = replace('${subscription}eessa${functionAppName}', '-', '')

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
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

// When deploying to a Function App utilising a secured VNet to store its deployment files,
// unique file shares must be pre-generated and unique to each slot prior to deploying the
// Function App itself if we wish to use slot swapping.
//
// See the second paragraph of https://learn.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code?tabs=json%2Clinux%2Cdevops&pivots=premium-plan#secured-deployments.
resource fileShareStaging 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  name: '${storageAccountName}/default/${fileShareNameStaging}'
  dependsOn: [
    storageAccount
  ]
}

resource fileShareProduction 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  name: '${storageAccountName}/default/${fileShareNameProduction}'
  dependsOn: [
    storageAccount
  ]
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionName
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
    reserved: appServicePlanOS == 'Linux'
    siteConfig: {
      linuxFxVersion: appServicePlanOS == 'Linux' ? 'DOTNET-ISOLATED|8.0' : null
    }
  }
  tags: tagValues
}

var dedicatedStorageAccountString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'

// Create staging and production deploy slots, and set base app settings on both.
// These will be infrastructure-specific appsettings, and the YAML pipeline will handle the deployment of
// application-specific appsettings so as to be able to control the rollout of new, updated and deleted
// appsettings to the correct swap slots.
module functionAppSlotSettings 'appServiceSlotConfig.bicep' = {
  name: '${functionApp.name}AppServiceSlotConfigDeploy'
  params: {
    appName: functionName
    location: location
    appServiceExists: functionAppExists
    slotSpecificSettingKeys: [
      // This value is sticky to its individual slot and will not swap when slot swapping occurs.
      // This "SLOT_NAME" configuration value is merely to help enable debugging and checking which.
      // site is being viewed.
      'SLOT_NAME'
    ]
    baseSettings: union(settings, {

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

      // This is set by the Azure ZIP deploy commands, so it si included here to prevent any unnecessary checking for changes in this
      // property when a code deploy is pushed out.
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
      WEBSITE_CONTENTSHARE: fileShareNameStaging
    }
    prodOnlySettings: {
      SLOT_NAME: 'production'
      // As above, this value is distinct from its staging slot equivalent.
      WEBSITE_CONTENTSHARE: fileShareNameProduction
    }
  }
  dependsOn: [
    fileShareStaging
    fileShareProduction
  ]
}

output functionAppName string = functionApp.name
output principalId string = functionApp.identity.principalId
output tenantId string = functionApp.identity.tenantId

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

var appServicePlanName = '${resourcePrefix}-asp-${functionAppName}'
var reserved = appServicePlanOS == 'Linux' ? true : false
var functionName = 'test-fa-${functionAppName}'
var fileShareName = toLower(functionAppName)

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

//resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
//  name: '${storageAccountName}/default/${fileShareName}'
//  dependsOn: [
//    storageAccount
//  ]
//}

//resource stagingFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
//  name: '${storageAccountName}/default/${fileShareName}-staging'
//  dependsOn: [
//    storageAccount
//  ]
//}

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
    virtualNetworkSubnetId: subnetId
    clientAffinityEnabled: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
    }
  }
  tags: tagValues
  //dependsOn: [
  //  fileShare
  //]
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
    slotSpecificSettingKeys: [
      'APP_CONFIGURATION_LABEL'
      // 'WEBSITE_CONTENTSHARE'
    ]
    baseSettings: union(settings, {
      AzureWebJobsStorage: dedicatedStorageAccountString
      // WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: dedicatedStorageAccountString
      WEBSITE_CONTENTOVERVNET: 1
      WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'true'
      FUNCTIONS_EXTENSION_VERSION: '~4'
      APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsightsKey
      FUNCTIONS_WORKER_RUNTIME: functionAppRuntime
      WEBSITE_RUN_FROM_PACKAGE: '1'
    })
    stagingOnlySettings: {
      APP_CONFIGURATION_LABEL: 'staging'
      // WEBSITE_CONTENTSHARE: '${fileShareName}-staging'
    }
    prodOnlySettings: {
      APP_CONFIGURATION_LABEL: 'production'
      // WEBSITE_CONTENTSHARE: fileShareName
    }
  }
}

output functionAppName string = functionApp.name
output principalId string = functionApp.identity.principalId
output tenantId string = functionApp.identity.tenantId

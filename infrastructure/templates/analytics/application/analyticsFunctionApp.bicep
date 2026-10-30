import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../../common/resource-names.bicep'

@description('The IP address ranges that can access the Analytics Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The IP address ranges that can access the Analytics Function App storage accounts.')
param storageFirewallRules IpRange[]

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Name of the shared analytics storage account.')
param analyticsStorageAccountName string

@description('Name of the shared analytics file share.')
param analyticsFileShareName string

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('The cron schedule for the analytics consumer Function.')
param analyticsRequestFilesConsumerCron string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.keyVault.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}
resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.analyticsFunctionApp
  parent: vNet
}

resource analyticsStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: analyticsStorageAccountName
}

resource storagePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.analyticsStoragePrivateEndpoints
  parent: vNet
}

// Shared log analytics workspace. Currently the Public API deployment is responsible for creating this.
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: resourceNames.logAnalyticsWorkspace
}

var fileShareMountPath = '/data/analytics'

module functionAppModule '../../common/components/function-app/function-app.bicep' = {
  name: 'analyticsFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.analytics.functionApp
    applicationInsightsConnectionString: applicationInsightsConnectionString
    diagnosticSettingsLogAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    appServicePlanName: resourceNames.analytics.appServicePlan
    appSettings: [
      {
        name: 'Analytics__BasePath'
        value: fileShareMountPath
      }
      {
        name: 'App__ConsumeAnalyticsRequestFilesCronSchedule'
        value: analyticsRequestFilesConsumerCron
      }
    ]
    keyVaultName: keyVault.name
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: false
    }
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    elasticCapacity: {
      minimumInstanceCount: 1
      maximumInstanceCount: 1
      instanceMemoryMB: 2048
    }
    healthCheckPath: '/api/HealthCheck'
    operatingSystem: 'Linux'
    functionAppRuntime: 'dotnet-isolated'
    linuxFxVersion: 'DOTNET-ISOLATED|10.0'
    storageAccountName: resourceNames.analytics.functionStorageAccount
    storageAccountConnectionStringSecretName: resourceNames.keyVault.secrets.analyticsStorageAccountConnectionString
    storageAccountPublicNetworkAccessEnabled: false
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    outboundSubnetId: outboundVnetSubnet.id
    privateEndpoints: {
      storageAccounts: storagePrivateEndpointSubnet.id
    }
    alerts: {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      memoryPercentage: true
      storageAccountAvailability: false
      storageLatency: false
      fileServiceAvailability: false
      fileServiceLatency: false
      fileServiceCapacity: false
      alertsGroupName: resourceNames.alertsGroup
    }
    azureFileShares: [{
      storageName: analyticsStorageAccountName
      storageAccountKey: analyticsStorageAccount.listKeys().keys[0].value
      storageAccountName: analyticsStorageAccountName
      fileShareName: analyticsFileShareName
      mountPath: fileShareMountPath
    }]
    tagValues: tagValues
  }
}

output functionAppName string = functionAppModule.outputs.name
output functionAppUrl string = functionAppModule.outputs.url

import { IpRange, FirewallRule } from '../types.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Function App name.')
param functionAppName string

@description('Specifies the App Service plan name.')
param appServicePlanName string

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('The language worker runtime to load in the function app.')
@allowed([
  'dotnet'
  'dotnet-isolated'
  'node'
  'java'
  'python'
])
param functionAppRuntime string = 'dotnet-isolated'

@description('Function app runtime version.')
param functionAppRuntimeVersion string = '8.0'

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Set the amount of memory allocated to each instance of the function app in MB.')
param instanceMemoryMB int = 2048

@description('The maximum number of instances for the function app.')
param maximumInstanceCount int = 100

@description('Specifies the subnet id for the function app outbound traffic across the VNet')
param subnetId string

@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpoints {
  functionApp: string? // TODO not used?
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

@description('Specifies the SKU for the Function App hosting plan.')
param sku object

@description('Specifies the Key Vault name that this Function App will be permitted to get and list secrets from')
param keyVaultName string

@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan')
param alwaysOn bool = false

@description('Specifies firewall rules for the various storage accounts in use by the Function App')
param storageFirewallRules IpRange[] = []

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  cpuPercentage: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
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

var storageAlerts = alerts != null
  ? {
      availability: alerts!.storageAccountAvailability
      latency: alerts!.storageLatency
      alertsGroupName: alerts!.alertsGroupName
    }
  : null

module appServicePlanModule '../../public-api/components/appServicePlan.bicep' = {
  name: 'appServicePlanModuleDeploy'
  params: {
    planName: appServicePlanName
    location: location
    kind: 'functionapp'
    sku: sku
    operatingSystem: operatingSystem
    alerts: alerts != null ? {
      cpuPercentage: alerts!.cpuPercentage
      memoryPercentage: alerts!.memoryPercentage
      alertsGroupName: alerts!.alertsGroupName
    } : null
    tagValues: tagValues
  }
}

module deploymentStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: '${deploymentStorageAccountName}StorageAccountDeploy'
  params: {
    location: location
    storageAccountName: deploymentStorageAccountName
    allowedSubnetIds: [subnetId]
    firewallRules: storageFirewallRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVaultName
    privateEndpointSubnetIds: {
      blob: privateEndpoints.storageAccounts
      queue: privateEndpoints.storageAccounts
    }
    publicNetworkAccessEnabled: false
    alerts: storageAlerts
    tagValues: tagValues
  }
}

module deploymentBlobServiceModule '../../public-api/components/blobService.bicep' = {
  name: 'blobServiceModuleDeploy'
  params: {
    storageAccountName: deploymentStorageAccountName
    containerNames: ['app-package']
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: identity
  properties: {
    serverFarmId: appServicePlanModule.outputs.planId
    siteConfig: {
      alwaysOn: alwaysOn
      appSettings: [
        // {
        //   name: 'AzureWebJobsStorage'
        //   value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        // }
        {
          name: 'AzureWebJobsStorage__accountName'
          value: deploymentStorageAccountModule.outputs.storageAccountName
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsightsKey
        }
        // {
        //   name: 'WEBSITE_RUN_FROM_PACKAGE'
        //   value: '1'
        // }
        // {
        //   name: 'FUNCTIONS_EXTENSION_VERSION'
        //   value: '~4'
        // }
        // {
        //   name: 'FUNCTIONS_WORKER_RUNTIME'
        //   value: functionAppRuntime
        // }
      ]
      ftpsState: 'FtpsOnly'
      linuxFxVersion: operatingSystem == 'Linux' ? 'DOTNET-ISOLATED|8.0' : null
      minTlsVersion: '1.3'
      netFrameworkVersion: '8.0'
      publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
    }
    functionAppConfig: {
      deployment: {
        storage: {
          authentication: {
            type: 'SystemAssignedIdentity'
          }
          type: 'blobContainer'
          value: '${deploymentStorageAccountModule.properties.primaryEndpoints.blob}${deploymentStorageContainerName}'
        }
      }
      scaleAndConcurrency: {
        instanceMemoryMB: instanceMemoryMB
        maximumInstanceCount: maximumInstanceCount
      }
      runtime: { 
        name: functionAppRuntime
        version: functionAppRuntimeVersion
      }
    }
    httpsOnly: true
  }
  tags: tagValues
}

module storageAccountRoleAssignmentModule 'storageAccountRoleAssignment.bicep' = {
  name: 'storageAccountRoleAssignmentModuleDeploy'
  params: {
    principalIds: [functionApp.identity.principalId]
    storageAccountName: deploymentStorageAccountModule.outputs.storageAccountName
    role: 'Storage Blob Data Owner'
  }
}

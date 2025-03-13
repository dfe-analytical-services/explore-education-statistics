import { IpRange, FirewallRule, AzureFileShareMount } from '../types.bicep'
import { abbreviations } from '../../common/abbreviations.bicep'
import { staticAverageLessThanHundred, staticMinGreaterThanZero } from '../../public-api/components/alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan } from '../../public-api/components/alerts/dynamicAlertConfig.bicep'

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

@description('Name of the storage account in use by the Function App.')
param storageAccountName string

@description('Specifies whether the storage account in use by the Function App is accessible from the public internet.')
param storageAccountPublicNetworkAccessEnabled bool = false

@description('Specifies firewall rules for the storage account in use by the Function App.')
param storageFirewallRules IpRange[] = []

@description('Specifies additional setting to add to the Function App.')
param appSettings {
  name: string
  @secure()
  value: string
}[]

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string

@description('Specifies whether to grant the Function App role-based access to storage account queue data.')
param deployQueueRoleAssignment bool = false

@description('Set the amount of memory allocated to each instance of the function app in MB.')
param instanceMemoryMB int = 2048

@description('The maximum number of instances for the function app.')
param maximumInstanceCount int = 100

@description('Specifies the subnet id for the function app outbound traffic across the VNet.')
param outboundSubnetId string?

@description('Specifies the optional subnet id for function app inbound traffic from the VNet.')
param privateEndpoints {
  functionApp: string?
  storageAccounts: string
}

@description('Specifies whether this Function App is accessible from the public internet.')
param publicNetworkAccessEnabled bool = false

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@description('Specifies the list of origins that should be allowed to make cross-origin calls.')
param allowedOrigins array = []

@description('Specifies an optional URL for Azure to use to monitor the health of this resource')
param healthCheckPath string?

@description('An existing Managed Identity\'s Resource Id with which to associate this Function App.')
param userAssignedManagedIdentityParams {
  id: string
  name: string
  principalId: string
}?

@description('Specifies the SKU for the Function App hosting plan.')
param sku object

@description('Specifies the Key Vault name that this Function App will be permitted to get and list secrets from.')
param keyVaultName string

@description('Specifies whether or not the Function App already exists.')
param functionAppExists bool

@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan.')
param alwaysOn bool = false

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param alerts {
  cpuPercentage: bool
  functionAppHealth: bool
  httpErrors: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
  fileServiceAvailability: bool
  fileServiceLatency: bool
  fileServiceCapacity: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var firewallRules = [
  for (firewallRule, index) in functionAppFirewallRules: {
    name: firewallRule.name
    ipAddress: firewallRule.cidr
    action: 'Allow'
    tag: firewallRule.tag != null ? firewallRule.tag : 'Default'
    priority: firewallRule.priority != null ? firewallRule.priority : 100 + index
  }
]

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

var fileServiceAlerts = alerts != null
? {
    availability: alerts!.fileServiceAvailability
    latency: alerts!.fileServiceLatency
    capacity: alerts!.fileServiceCapacity
    alertsGroupName: alerts!.alertsGroupName
  }
: null

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

module appServicePlanModule '../../public-api/components/appServicePlan.bicep' = {
  name: '${appServicePlanName}ModuleDeploy'
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

module storageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: 'storageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: storageAccountName
    allowedSubnetIds: outboundSubnetId != null ? [outboundSubnetId!] : []
    firewallRules: storageFirewallRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVault.name
    privateEndpointSubnetIds: {
      blob: privateEndpoints.storageAccounts
      file: privateEndpoints.storageAccounts
      queue: privateEndpoints.storageAccounts
    }
    publicNetworkAccessEnabled: storageAccountPublicNetworkAccessEnabled
    alerts: storageAlerts
    tagValues: tagValues
  }
}

module fileShareModule '../../public-api/components/fileShare.bicep' = {
  name: '${storageAccountName}FileShareModuleDeploy'
  params: {
    storageAccountName: storageAccountModule.outputs.storageAccountName
    fileShareName: '${functionAppName}-${abbreviations.fileShare}'
    alerts: fileServiceAlerts
    tagValues: tagValues
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: (operatingSystem == 'Linux' ? 'functionapp,linux' : 'functionapp')
  identity: identity
  properties: {
    containerSize: instanceMemoryMB
    reserved: operatingSystem == 'Linux'
    serverFarmId: appServicePlanModule.outputs.planId
    vnetContentShareEnabled: true
    virtualNetworkSubnetId: outboundSubnetId
    siteConfig: {
      alwaysOn: alwaysOn
      appSettings: union([
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        // Use managed identity to access the storage account rather than key based access with a connection string
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccountModule.outputs.storageAccountName
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${storageAccountModule.outputs.connectionStringSecretName})'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: fileShareModule.outputs.fileShareName
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionAppRuntime
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        // Prevent the function app from building when performing deployment of an already-built site
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        // It's only possible to UPDATE a Function App using a Key Vault reference for WEBSITE_CONTENTAZUREFILECONNECTIONSTRING setting,
        // not creating a new Function App, unless we use the below setting to skip validation for the first time we create
        // this Function App.  See https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references?tabs=azure-cli#considerations-for-azure-files-mounting.
        {
          name: 'WEBSITE_SKIP_CONTENTSHARE_VALIDATION'
          value: !functionAppExists ? '1' : null
        }
      ], appSettings)
      cors: {
        allowedOrigins: union(['https://portal.azure.com'], allowedOrigins)
        supportCredentials: false
      }
      ftpsState: 'FtpsOnly'
      functionAppScaleLimit: maximumInstanceCount
      healthCheckPath: healthCheckPath
      minTlsVersion: '1.3'
      netFrameworkVersion: functionAppRuntimeVersion
      linuxFxVersion: operatingSystem == 'Linux' ? 'DOTNET-ISOLATED|8.0' : null
      publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
      ipSecurityRestrictions: publicNetworkAccessEnabled && length(firewallRules) > 0 ? firewallRules : null
      ipSecurityRestrictionsDefaultAction: 'Deny'
      scmIpSecurityRestrictions: [
        {
          ipAddress: 'Any'
          action: 'Allow'
          priority: 1
          name: 'Allow all'
          description: 'Allow all access'
        }
      ]
      scmIpSecurityRestrictionsUseMain: false
    }
    httpsOnly: true
  }
  tags: tagValues
}

resource azureStorageAccountsConfig 'Microsoft.Web/sites/config@2023-12-01' = if (length(azureFileShares) > 0) {
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

module keyVaultRoleAssignmentModule '../../public-api/components/keyVaultRoleAssignment.bicep' = {
  name: '${functionAppName}KeyVaultRoleAssignmentModuleDeploy'
  params: {
    principalIds: userAssignedManagedIdentityParams != null ? [userAssignedManagedIdentityParams!.principalId] : [functionApp.identity.principalId]
    keyVaultName: keyVault.name
    role: 'Secrets User'
  }
}

module storageAccountBlobRoleAssignmentModule 'storageAccountRoleAssignment.bicep' = {
  name: '${storageAccountName}BlobRoleAssignmentModuleDeploy'
  params: {
    principalIds: [functionApp.identity.principalId]
    storageAccountName: storageAccountModule.outputs.storageAccountName
    role: 'Storage Blob Data Owner'
  }
}

module storageAccountQueueRoleAssignmentModule 'storageAccountRoleAssignment.bicep' = if (deployQueueRoleAssignment) {
  name: '${storageAccountName}QueueRoleAssignmentModuleDeploy'
  params: {
    principalIds: [functionApp.identity.principalId]
    storageAccountName: storageAccountModule.outputs.storageAccountName
    role: 'Storage Queue Data Contributor'
  }
}

module privateEndpointModule '../../public-api/components/privateEndpoint.bicep' = if (privateEndpoints.?functionApp != null) {
  name: '${functionAppName}PrivateEndpointModuleDeploy'
  params: {
    serviceId: functionApp.id
    serviceName: functionApp.name
    serviceType: 'sites'
    subnetId: privateEndpoints.?functionApp ?? ''
    location: location
    tagValues: tagValues
  }
}

module healthAlert '../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.functionAppHealth) {
  name: '${functionAppName}HealthAlertModule'
  params: {
    resourceName: functionApp.name
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

module unexpectedHttpStatusCodeAlerts '../../public-api/components/alerts/staticMetricAlert.bicep' = [
  for httpStatusCode in unexpectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${functionAppName}${httpStatusCode}Module'
    params: {
      resourceName: functionApp.name
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

module expectedHttpStatusCodeAlerts '../../public-api/components/alerts/dynamicMetricAlert.bicep' = [
  for httpStatusCode in expectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${functionAppName}${httpStatusCode}Module'
    params: {
      resourceName: functionApp.name
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

output name string = functionApp.name
output url string = 'https://${functionApp.name}.azurewebsites.net'

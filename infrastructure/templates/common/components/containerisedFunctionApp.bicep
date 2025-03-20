import {
  FirewallRule
  IpRange
} from '../../common/types.bicep'

import {
  AzureFileShareMount
  EntraIdAuthentication
} from '../../public-api/types.bicep'

import { abbreviations } from '../../common/abbreviations.bicep'
import { staticAverageLessThanHundred, staticMinGreaterThanZero } from '../../public-api/components/alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan } from '../../public-api/components/alerts/dynamicAlertConfig.bicep'

@description('An existing Managed Identity\'s Resource Id with which to associate this Function App')
param userAssignedManagedIdentityParams {
  id: string
  name: string
  principalId: string
}?

@description('Specifies the Function App name')
param functionAppName string

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

@description('Name of the deployment storage account.')
param deploymentStorageAccountName string

@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan')
param alwaysOn bool?

@description('Specifies whether this Function App is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@description('Specifies the list of origins that should be allowed to make cross-origin calls.')
param allowedOrigins array = []

@description('Specifies firewall rules for the various storage accounts in use by the Function App')
param storageFirewallRules IpRange[] = []

@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpoints {
  functionApp: string?
  storageAccounts: string
}

@description('Specifies the location for all resources.')
param location string

@description('The Docker image tag. This value should represent a pipeline build number')
param functionAppDockerImageTag string

@description('Specifies the App Service plan name')
param appServicePlanName string

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string

@description('Specifies the SKU for the Function App hosting plan')
param sku object

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('An existing Service Principal\'s Client Id with which this Function App can pull Docker images (using it as the ACR username)')
@secure()
param dockerPullManagedIdentityClientId string

@description('An existing Service Principal\'s Secret value with which this Function App can pull Docker images (using it as the ACR password)')
@secure()
param dockerPullManagedIdentitySecretValue string

@description('Specifies the container image to deploy from the registry.')
param functionAppImageName string

@description('An existing App Registration registered with Entra ID that will be used to control access to this Function App')
param entraIdAuthentication EntraIdAuthentication?

@description('Enable pulling an image over a Virtual Network')
param vnetImagePullEnabled bool = false

@description('Specifies the subnet id for the function app outbound traffic across the VNet')
param subnetId string

@description('The number of preWarmed instances')
param preWarmedInstanceCount int = 1

@description('Site redundancy mode')
param redundancyMode string = 'None'

@description('Specifies the Key Vault name that this Function App will be permitted to get and list secrets from')
param keyVaultName string

@description('Specifies an optional URL for Azure to use to monitor the health of this resource')
param healthCheckPath string?

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  functionAppHealth: bool
  httpErrors: bool
  cpuPercentage: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
  alertsGroupName: string
  fileServiceAvailability: bool
  fileServiceLatency: bool
  fileServiceCapacity: bool
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
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

var storageAlerts = alerts != null
  ? {
      availability: alerts!.storageAccountAvailability
      latency: alerts!.storageLatency
      alertsGroupName: alerts!.alertsGroupName
    }
  : null

module appServicePlanModule '../../public-api/components/appServicePlan.bicep' = {
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

module keyVaultRoleAssignmentModule '../../public-api/components/keyVaultRoleAssignment.bicep' = {
  name: '${functionAppName}KeyVaultRoleAssignmentModuleDeploy'
  params: {
    principalIds: userAssignedManagedIdentityParams != null
      ? [userAssignedManagedIdentityParams!.principalId]
      : [functionApp.identity.principalId]
    keyVaultName: keyVault.name
    role: 'Secrets User'
  }
}

// module storageAccountBlobRoleAssignmentModule 'storageAccountRoleAssignment.bicep' = {
//   name: '${deploymentStorageAccountName}BlobRoleAssignmentModuleDeploy'
//   params: {
//     principalIds: userAssignedManagedIdentityParams != null
//     ? [userAssignedManagedIdentityParams!.principalId]
//     : [functionApp.identity.principalId]
//     storageAccountName: deploymentStorageAccountModule.outputs.storageAccountName
//     role: 'Storage Blob Data Owner'
//   }
// }

var keyVaultReferenceIdentity = userAssignedManagedIdentityParams != null ? userAssignedManagedIdentityParams!.id : null

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

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  tags: tagValues
  kind: 'functionapp,linux,container'
  identity: identity
  properties: {
    enabled: true
    serverFarmId: appServicePlanModule.outputs.planId
    reserved: operatingSystem == 'Linux'
    vnetImagePullEnabled: vnetImagePullEnabled
    virtualNetworkSubnetId: subnetId
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/${functionAppImageName}:${functionAppDockerImageTag}'
      alwaysOn: alwaysOn ?? null
      keyVaultReferenceIdentity: keyVaultReferenceIdentity
      scmType: 'None'
      autoHealEnabled: true
      healthCheckPath: healthCheckPath
      vnetRouteAllEnabled: true
      publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
      ipSecurityRestrictions: publicNetworkAccessEnabled && length(firewallRules) > 0 ? firewallRules : null
      cors: {
        allowedOrigins: union(['https://portal.azure.com'], allowedOrigins)
        supportCredentials: false
      }
      http20Enabled: true
      minTlsVersion: '1.3'
      preWarmedInstanceCount: preWarmedInstanceCount
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        // Use managed identity to access the storage account rather than key based access with a connection string
        {
          name: 'AzureWebJobsStorage__accountName'
          value: deploymentStorageAccountModule.outputs.storageAccountName
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: acrLoginServer
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: dockerPullManagedIdentityClientId
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: dockerPullManagedIdentitySecretValue
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
      ]
    }
    httpsOnly: true
    redundancyMode: redundancyMode
    keyVaultReferenceIdentity: keyVaultReferenceIdentity
  }
}

module deploymentStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: '${functionAppName}DeploymentStorageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: deploymentStorageAccountName
    allowedSubnetIds: [subnetId]
    firewallRules: storageFirewallRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVaultName
    privateEndpointSubnetIds: {
      // TODO DW - do we need more in order to support tables for the Function App etc?
      blob: privateEndpoints.storageAccounts
    }
    publicNetworkAccessEnabled: false
    alerts: storageAlerts
    tagValues: tagValues
  }
}

resource azureStorageAccountsConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: functionApp
  name: 'azurestorageaccounts'
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

module privateEndpointModule '../../public-api/components/privateEndpoint.bicep' = if (privateEndpoints.?functionApp != null) {
  name: '${functionAppName}PrivateEndpointDeploy'
  params: {
    serviceId: functionApp.id
    serviceName: functionApp.name
    serviceType: 'sites'
    subnetId: privateEndpoints.?functionApp ?? ''
    location: location
    tagValues: tagValues
  }
}

module azureAuthentication '../../public-api/components/siteAzureAuthentication.bicep' = if (entraIdAuthentication != null) {
  name: '${functionAppName}AzureAuthentication'
  params: {
    clientId: entraIdAuthentication!.appRegistrationClientId
    siteName: functionApp.name
    allowedClientIds: entraIdAuthentication!.allowedClientIds
    allowedPrincipalIds: entraIdAuthentication!.allowedPrincipalIds
    requireAuthentication: entraIdAuthentication!.requireAuthentication
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

output url string = 'https://${functionApp.name}.azurewebsites.net'

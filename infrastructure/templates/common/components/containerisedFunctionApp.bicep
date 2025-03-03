import {
  FirewallRule
  AzureFileShareMount
  EntraIdAuthentication
} from '../../public-api/types.bicep'

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

@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan')
param alwaysOn bool?

@description('Specifies whether this Function App is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpoints {
  functionApp: string?
  storageAccounts: string
}

@description('Specifies the location for all resources.')
param location string

@description('The Docker image tag. This value represents a pipeline build number')
param dockerImageTag string = '1.0.0'

@description('Specifies the App Service plan name')
param appServicePlanName string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the SKU for the Function App hosting plan')
param sku object

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param functionAppImageName string

@description('An existing App Registration registered with Entra ID that will be used to control access to this Container App')
param entraIdAuthentication EntraIdAuthentication?

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  functionAppHealth: bool
  httpErrors: bool
  cpuPercentage: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
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

// Added
var keyVaultReferenceIdentity = userAssignedManagedIdentityParams != null ? userAssignedManagedIdentityParams!.id : null

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  tags: tagValues
  kind: 'functionapp,linux,container'
  properties: {
    enabled: true
    serverFarmId: appServicePlanModule.outputs.planId
    reserved: true // Assumed this is Linux
    // vnetRouteAllEnabled: false
    vnetImagePullEnabled: false // TODO: Add parameter
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/${functionAppImageName}:${dockerImageTag}'
      acrUseManagedIdentityCreds: true
      alwaysOn: alwaysOn ?? null
      keyVaultReferenceIdentity: keyVaultReferenceIdentity
      scmType: 'None'
      autoHealEnabled: true
      vnetRouteAllEnabled: true
      publicNetworkAccess: publicNetworkAccessEnabled ? 'Enabled' : 'Disabled'
      ipSecurityRestrictions: publicNetworkAccessEnabled && length(firewallRules) > 0 ? firewallRules : null
      http20Enabled: true
      minTlsVersion: '1.3'
      preWarmedInstanceCount: 1 // TODO: Add parameter
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsightsKey
        }
      ]
    }
    httpsOnly: true
    redundancyMode: 'None' // TODO: Add parameter
    storageAccountRequired: true
    keyVaultReferenceIdentity: keyVaultReferenceIdentity
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
    siteName: functionAppName
    allowedClientIds: entraIdAuthentication!.allowedClientIds
    allowedPrincipalIds: entraIdAuthentication!.allowedPrincipalIds
    requireAuthentication: entraIdAuthentication!.requireAuthentication
  }
}

module healthAlert '../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.functionAppHealth) {
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
  dependsOn: [
    functionApp
  ]
}

var unexpectedHttpStatusCodeMetrics = ['Http401', 'Http5xx']

module unexpectedHttpStatusCodeAlerts '../../public-api/components/alerts/staticMetricAlert.bicep' = [
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
    dependsOn: [
      functionApp
    ]
  }
]

var expectedHttpStatusCodeMetrics = ['Http403', 'Http4xx']

module expectedHttpStatusCodeAlerts '../../public-api/components/alerts/dynamicMetricAlert.bicep' = [
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
    dependsOn: [
      functionApp
    ]
  }
]

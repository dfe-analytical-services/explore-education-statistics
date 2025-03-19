import {
  FirewallRule
  IpRange
} from '../../common/types.bicep'

import {
  AzureFileShareMount
  EntraIdAuthentication
} from '../../public-api/types.bicep'

import { staticAverageLessThanHundred, staticMinGreaterThanZero } from '../../public-api/components/alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan } from '../../public-api/components/alerts/dynamicAlertConfig.bicep'
import { ResourceNames } from '../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('The IP address ranges that can access the Screener storage accounts.')
param storageFirewallRules IpRange[]

@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Screener Function App.')
param screenerAppRegistrationClientId string

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string

@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param functionAppImageName string

@description('The Docker image tag for the data screener. This value should represent a pipeline build number')
param screenerDockerImageTag string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.screenerFunction
  parent: vNet
}

resource privateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.screenerFunctionPrivateEndpoints
  parent: vNet
}

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: resourceNames.existingResources.adminApp
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId

resource screenerFunctionAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.screener.screenerFunctionIdentity
  location: location
}

module containerisedFunctionAppModule '../../common/components/containerisedFunctionApp.bicep' = {
  name: 'screenerContainerisedFunctionAppModuleDeploy'
  params: {
    operatingSystem: operatingSystem
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    functionAppName: resourceNames.screener.screenerFunction
    acrLoginServer: acrLoginServer
    functionAppImageName: functionAppImageName
    functionAppDockerImageTag: screenerDockerImageTag
    location: location
    applicationInsightsConnectionString: applicationInsightsConnectionString
    healthCheckPath: '/api/screen'
    appServicePlanName: resourceNames.screener.screenerFunction
    keyVaultName: keyVault.name
    // entraIdAuthentication: {
    //   appRegistrationClientId: screenerAppRegistrationClientId
    //   allowedClientIds: [
    //     adminAppClientId
    //     devopsServicePrincipalId
    //   ]
    //   allowedPrincipalIds: []
    //   requireAuthentication: true
    // }
    userAssignedManagedIdentityParams: {
      id: screenerFunctionAppManagedIdentity.id
      name: screenerFunctionAppManagedIdentity.name
      principalId: screenerFunctionAppManagedIdentity.properties.principalId
    }
    dockerPullManagedIdentityClientId: keyVault.getSecret('DOCKER-REGISTRY-SERVER-USERNAME')
    dockerPullManagedIdentitySecretValue: keyVault.getSecret('DOCKER-REGISTRY-SERVER-PASSWORD')
    deploymentStorageAccountName: resourceNames.screener.screenerFunctionStorageAccount
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    publicNetworkAccessEnabled: true
    privateEndpoints: {
      functionApp: privateEndpointSubnet.id
      storageAccounts: privateEndpointSubnet.id
    }
    subnetId: outboundVnetSubnet.id
    alerts: deployAlerts
      ? {
          cpuPercentage: true
          memoryPercentage: true
          storageAccountAvailability: true
          storageLatency: false
          alertsGroupName: resourceNames.existingResources.alertsGroup
          functionAppHealth: true
          httpErrors: true
          fileServiceAvailability: true
          fileServiceCapacity: true
          fileServiceLatency: true
        }
      : null
    tagValues: tagValues
  }
}

output functionAppUrl string = containerisedFunctionAppModule.outputs.url

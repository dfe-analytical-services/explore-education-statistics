import { IpRange } from '../common/types.bicep'
import { abbreviations } from '../common/abbreviations.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Screener Function App.')
param screenerAppRegistrationClientId string = ''

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string = ''

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('Specifies whether or not the Screener Function App already exists.')
param screenerFunctionAppExists bool = true

@description('The Docker image tag for the data screener. This value should represent a pipeline build number')
param screenerDockerImageTag string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

var maintenanceFirewallRules = [
  for maintenanceIpRange in maintenanceIpRanges: {
    name: maintenanceIpRange.name
    cidr: maintenanceIpRange.cidr
    tag: 'Default'
    priority: 100
  }
]

// Create a shared Application Insights resource for Search resources to use.
module applicationInsightsModule 'application/screenerApplicationInsights.bicep' = {
  name: 'screenerApplicationInsightsModule'
  params: {
    location: location
    resourcePrefix: screenerResourcePrefix
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module coreStorage 'application/shared/coreStorage.bicep' = {
  name: 'coreStorageApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module screenerFunctionAppModule 'application/screenerContainerisedFunctionApp.bicep' = {
  name: 'screenerFunctionApp'
  params: {
    location: location
    functionAppImageName: 'ees-screener-api'
    coreStorageAccessKeyName: coreStorage.outputs.coreStorageAccessKey
    coreStorageBlobEndpoint: coreStorage.outputs.coreStorageBlobEndpoint
    acrLoginServer: keyVault.getSecret('DOCKER-REGISTRY-SERVER-DOMAIN')
    screenerAppRegistrationClientId: screenerAppRegistrationClientId
    devopsServicePrincipalId: devopsServicePrincipalId
    screenerDockerImageTag: screenerDockerImageTag
    resourceNames: resourceNames
    functionAppExists: screenerFunctionAppExists
    functionAppFirewallRules: union(
      [
        {
          cidr: 'AzureCloud'
          tag: 'ServiceTag'
          priority: 101
          name: 'AzureCloud'
        }
      ],
      maintenanceFirewallRules
    )
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: applicationInsightsModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
    deployAlerts: deployAlerts
  }
}

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

// The resource prefix for resources created in the ARM template.
var legacyResourcePrefix = subscription

// The resource prefix for more recent resources created in Bicep templates.
var resourcePrefix = '${subscription}-ees'

// The resource prefix for anything specific to the Screener API.
var screenerResourcePrefix = '${subscription}-ees-sapi'

var resourceNames = {
  existingResources: {
    adminApp: '${legacyResourcePrefix}-as-ees-admin'
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    logAnalyticsWorkspace: '${resourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
    vNet: '${legacyResourcePrefix}-vnet-ees'
    alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
    subnets: {
      adminApp: '${legacyResourcePrefix}-snet-ees-admin'
      screenerFunction: '${screenerResourcePrefix}-snet-${abbreviations.webSitesFunctions}-screener'
      screenerFunctionPrivateEndpoints: '${screenerResourcePrefix}-snet-${abbreviations.storageStorageAccounts}-screener-pep'
    }
    coreStorageAccount: subscription == 's101t01' || subscription == 's101p02'
      ? '${legacyResourcePrefix}storageeescore'
      : '${legacyResourcePrefix}saeescore'
  }
  screener: {
    screenerFunction: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
    screenerFunctionStorageAccount: '${replace(screenerResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}fn'
  }
}

output screenerFunctionAppUrl string = screenerFunctionAppModule.outputs.functionAppUrl

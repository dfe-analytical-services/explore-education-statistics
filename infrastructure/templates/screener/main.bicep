import { IpRange } from '../common/types.bicep'
import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'

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

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string = ''

@description('Whether or not to include Data Dictionary checks in the Screener.')
param includeDataDictionaryChecks bool = false

@description('Whether or not to log screening results in the Screener API logs.')
param logScreeningResults bool = false

@description('Number of concurrent threads that can be used by Plumber to process background jobs.')
param concurrentRWorkers int = 4

@description('The minimum number of instances for the function app.')
param minimumInstanceCount int = 1

@description('The maximum number of instances for the function app.')
param maximumInstanceCount int = 1

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('The Docker image tag for the data screener. This value should represent a pipeline build number.')
param screenerDockerImageTag string = ''

@description('SKU for the Screener API Function App\'s App Service Plan.')
param screenerFunctionAppSku FunctionAppServicePlanSku = {
  name: 'EP1'
  tier: 'ElasticPremium'
  family: 'EP'
}

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.keyVault.keyVault
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
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module coreStorage 'application/shared/coreStorage.bicep' = {
  name: 'coreStorageApplicationModuleDeploy'
  params: {
    names: resourceNames.storageAccounts
  }
}

module screenerLogsStorageModule 'application/screenerLogsStorage.bicep' = {
  name: 'screenerLogsStorageModuleDeploy'
  params: {
    resourceNames: resourceNames
    storageFirewallRules: maintenanceIpRanges
    tagValues: tagValues
    deployAlerts: deployAlerts
  }
}

module screenerFunctionAppModule 'application/screenerContainerisedFunctionApp.bicep' = {
  name: 'screenerFunctionModuleDeploy'
  params: {
    location: location
    functionAppImageName: 'ees-screener-api'
    coreStorageBlobEndpoint: coreStorage.outputs.coreStorageBlobEndpoint
    acrLoginServer: keyVault.getSecret(resourceNames.keyVault.secrets.acr.dockerRegistryServerDomain)
    screenerAppRegistrationClientId: keyVault.getSecret(resourceNames.keyVault.secrets.screener.appRegistrationClientId)
    devopsServicePrincipalId: devopsServicePrincipalId
    includeDataDictionaryChecks: includeDataDictionaryChecks
    logScreeningResults: logScreeningResults
    concurrentRWorkers: concurrentRWorkers
    minimumInstanceCount: minimumInstanceCount
    maximumInstanceCount: maximumInstanceCount
    screenerDockerImageTag: screenerDockerImageTag
    resourceNames: resourceNames
    sku: screenerFunctionAppSku
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
  dependsOn: [
    screenerLogsStorageModule
  ]
}

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
})

output screenerFunctionAppUrl string = screenerFunctionAppModule.outputs.functionAppUrl

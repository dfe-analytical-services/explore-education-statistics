import { abbreviations } from '../common/abbreviations.bicep'
import { IpRange } from '../common/types.bicep'

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

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

@description('Specifies whether or not the Analytics Function App already exists.')
param analyticsFunctionAppExists bool = true

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('The cron schedule for the Public API queries consumer Function. Defaults to every hour.')
param publicApiQueryConsumerCron string = '* 0 * * * *'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var maintenanceFirewallRules = [
  for maintenanceIpRange in maintenanceIpRanges: {
    name: maintenanceIpRange.name
    cidr: maintenanceIpRange.cidr
    tag: 'Default'
    priority: 100
  }
]

var resourcePrefix = '${subscription}-ees'

var resourceNames = {
  existingResources: {
    keyVault: '${subscription}-kv-ees-01'
    vNet: '${subscription}-vnet-ees'
    alertsGroup: '${subscription}-ag-ees-alertedusers'
    subnets: {
      analyticsFunctionApp: '${resourcePrefix}-snet-${abbreviations.webSitesFunctions}-analytics'
      storagePrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.storageStorageAccounts}-anlyt-${abbreviations.privateEndpoints}'
    }
  }
}

// Create an Application Insights resource for Analytics resources to use.
module applicationInsightsModule 'application/analyticsApplicationInsights.bicep' = {
  name: 'analyticsApplicationInsightsModule'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module analyticsStorageModule 'application/analyticsStorage.bicep' = {
  name: 'analyticsStorageAccountApplicationModuleDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    resourceNames: resourceNames
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: true
    tagValues: tagValues
  }
}

module analyticsFunctionAppModule 'application/analyticsFunctionApp.bicep' = {
  name: 'analyticsFunctionAppModule'
  params: {
    location: location
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    functionAppExists: analyticsFunctionAppExists
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
    analyticsStorageAccountName: analyticsStorageModule.outputs.storageAccountName
    analyticsFileShareName: analyticsStorageModule.outputs.fileShareName
    publicApiQueryConsumerCron: publicApiQueryConsumerCron
    tagValues: tagValues
  }
}

output analyticsFunctionAppUrl string = analyticsFunctionAppModule.outputs.functionAppUrl

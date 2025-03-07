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

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var resourcePrefix = '${subscription}-ees'

var resourceNames = {
  existingResources: {
    keyVault: '${subscription}-kv-ees-01'
    vNet: '${subscription}-vnet-ees'
    alertsGroup: '${subscription}-ag-ees-alertedusers'
    subnets: {
      searchStoragePrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.storageStorageAccounts}-search-pep'
    }
  }
}

module searchServiceModule 'application/searchService.bicep' = {
  name: 'searchServiceModule'
  params: {
    location: location
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint

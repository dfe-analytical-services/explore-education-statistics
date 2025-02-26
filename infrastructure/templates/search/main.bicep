import { IpRange } from '../common/types.bicep'
import { abbreviations } from 'abbreviations.bicep'

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

@description('Does the Search Docs Function need creating or updating?')
param deploySearchDocsFunction bool = false

@description('Does the Search Service need creating or updating?')
param deploySearchService bool = false

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

//
// Define our resource prefix.
//

// The resource prefix for resources created in the ARM template.
var legacyResourcePrefix = subscription

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
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    vNet: '${legacyResourcePrefix}-vnet-ees'
    alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
    subnets: {
      searchDocsFunction: '${resourcePrefix}-snet-${abbreviations.webSitesFunctions}-searchdocs'
      searchDocsFunctionPrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.webSitesFunctions}-searchdocs-pep'
      searchDocsStoragePrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.storageStorageAccounts}-searchdocs-pep'
    }
  }
  search: {
    searchDocsFunction: '${resourcePrefix}-${abbreviations.webSitesFunctions}-searchdocs'
    searchDocsFunctionStorageAccount: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocsfn'
    searchDocsStorageAccount: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocs'
    searchService: '${resourcePrefix}-${abbreviations.searchSearchServices}'
  }
}

module searchDocsFunctionModule 'application/searchDocsFunction.bicep' = if (deploySearchDocsFunction) {
  name: 'searchDocsFunctionModule'
  params: {
    location: location
    resourceNames: resourceNames
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
    tagValues: tagValues
    deployAlerts: deployAlerts
  }
}

module searchServiceModule 'application/searchService.bicep' = if (deploySearchService) {
  name: 'searchServiceModule'
  params: {
    location: location
    resourceNames: resourceNames
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output searchEndpoint string = deploySearchService ? searchServiceModule.outputs.searchEndpoint : ''

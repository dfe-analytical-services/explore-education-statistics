import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'
import { IpRange } from '../common/types.bicep'
import { StorageAccountSku } from '../common/components/storage/types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

@description('SKU of the Search service\'s storage account and the Function App storage accounts, which sets their redundancy. Legacy accounts can\'t be converted to zone-redundant by a deploy, so only the param files in "parameters/elz" set a zone-redundant SKU.')
param storageAccountSku StorageAccountSku = 'Standard_LRS'

@description('Whether to deploy a user-assigned managed identity for each of this pipeline\'s apps, for them to use instead of system-assigned ones. Only the param files in "parameters/elz" turn this on.')
param deployAppIdentities bool = false

// TODO EES-7502 - remove the Key Vault alternative once every environment has moved to the ELZ.
@description('Whether the Search Docs Function App connects to the search storage account with its managed identity, given a keyless connection string, rather than with a connection string from Key Vault, which then isn\'t stored. Only the param files in "parameters/elz" turn this on.')
param identityBasedStorageConnections bool = false

// TODO EES-7502 - remove, leaving shared key access off, once every environment has moved to the ELZ.
@description('Whether the search storage account accepts its account keys. In ELZ everything connects to it with managed identities, so the param files in "parameters/elz" turn this off.')
param storageSharedKeyAccessEnabled bool = true

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

@description('The URL of the Content API.')
param contentApiUrl string

@description('The URL of the Data API.')
param dataApiUrl string

@description('Name of the \'Filter\' index in Azure AI Search.')
param searchServiceFilterIndexName string = ''

@description('Name of the \'Data set\' index in Azure AI Search.')
param searchServiceDataSetIndexName string = ''

@description('Name of the indexer associated with the \'Release\' index in Azure AI Search.')
param searchServiceReleaseIndexerName string = ''

@description('Name of the indexer associated with the \'Data set\' index in Azure AI Search.')
param searchServiceDataSetIndexerName string = ''

@description('Indicates whether API keys are permitted for authentication to Azure AI Search in addition to role-based access control (RBAC). Disabling API keys forces all clients to use RBAC only.')
param searchServiceLocalAuthEnabled bool = false

@description('Controls the availability of semantic ranking for all indexes. Set to \'free\' for limited query volume on the free plan, \'standard\' for unlimited volume on the standard pricing plan, or \'disabled\' to turn it off.')
@allowed([
  'disabled'
  'free'
  'standard'
])
param searchServiceSemanticRankerAvailability string

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

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

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
  elz: elz
})

module appIdentitiesModule '../common/components/managed-identity/user-assigned-identities.bicep' = if (deployAppIdentities) {
  name: 'appIdentitiesModuleDeploy'
  params: {
    identityNames: [
      resourceNames.nlSearch.identity
      resourceNames.searchDocs.identity
    ]
    location: location
    tagValues: tagValues
  }
}

module monitoringModule 'application/monitoring.bicep' = {
  name: 'monitoringModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module nlSearchFunctionAppModule 'application/nlSearchFunctionApp.bicep' = {
  name: 'nlSearchFunctionAppApplicationModuleDeploy'
  params: {
    useUserAssignedIdentity: deployAppIdentities
    storageAccountSku: storageAccountSku
    resourceNames: resourceNames
    dataApiUrl: dataApiUrl
    functionAppFirewallRules: []
    logAnalyticsWorkspaceId: monitoringModule.outputs.logAnalyticsWorkspaceId
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchServiceFilterIndexName: searchServiceFilterIndexName
    searchServiceDataSetIndexName: searchServiceDataSetIndexName
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: monitoringModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
  }
  dependsOn: [
    appIdentitiesModule
  ]
}

module searchDocsFunctionAppModule 'application/searchDocsFunctionApp.bicep' = {
  name: 'searchDocsFunctionAppApplicationModuleDeploy'
  params: {
    useUserAssignedIdentity: deployAppIdentities
    storageAccountSku: storageAccountSku
    resourceNames: resourceNames
    contentApiUrl: contentApiUrl
    dataSetDocumentsContainerName: searchServiceModule.outputs.searchStorageDocumentContainers.dataSetDocuments
    dataSetIndexerName: searchServiceDataSetIndexerName
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
    logAnalyticsWorkspaceId: monitoringModule.outputs.logAnalyticsWorkspaceId
    releaseIndexerName: searchServiceReleaseIndexerName
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchStorageAccountName: searchServiceModule.outputs.searchStorageAccountName
    searchStorageAccountConnectionStringSecretName: searchServiceModule.outputs.?searchStorageAccountConnectionStringSecretName
    identityBasedStorageConnection: identityBasedStorageConnections
    releaseDocumentsContainerName: searchServiceModule.outputs.searchStorageDocumentContainers.releaseDocuments
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: monitoringModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
  }
  dependsOn: [
    appIdentitiesModule
  ]
}

module searchDocsFunctionEventSubscriptionsModule 'application/searchDocsFunctionEventSubscriptions.bicep' = {
  name: 'searchDocsFunctionEventSubscriptionsModuleDeploy'
  params: {
    resourceNames: resourceNames
    searchDocsFunctionAppStorageAccountName: searchDocsFunctionAppModule.outputs.functionAppStorageAccountName
    storageQueueNames: searchDocsFunctionAppModule.outputs.storageQueueNames
  }
}

module searchServiceModule 'application/searchService.bicep' = {
  name: 'searchServiceApplicationModuleDeploy'
  params: {
    storageAccountSku: storageAccountSku
    location: location
    resourceNames: resourceNames
    searchServiceLocalAuthEnabled: searchServiceLocalAuthEnabled
    searchServiceIpRules: [] // No restrictions applied as the resource is intended to be publicly accessible.
    semanticRankerAvailability: searchServiceSemanticRankerAvailability
    storageIpRules: maintenanceIpRanges
    storeStorageAccountConnectionString: !identityBasedStorageConnections
    storageSharedKeyAccessEnabled: storageSharedKeyAccessEnabled
    logAnalyticsWorkspaceId: monitoringModule.outputs.logAnalyticsWorkspaceId
    tagValues: tagValues
  }
}

output searchDocsFunctionAppUrl string = searchDocsFunctionAppModule.outputs.functionAppUrl
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchStorageAccountManagedIdentityConnectionString string = searchServiceModule.outputs.searchStorageAccountManagedIdentityConnectionString
output releaseDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.releaseDocuments
output nlSearchFunctionAppUrl string = nlSearchFunctionAppModule.outputs.functionAppUrl
output dataSetDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.dataSetDocuments
output filterDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.filterDocuments

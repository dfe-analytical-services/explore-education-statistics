import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Whether to deploy the Search service configuration to create/update the data source, index and indexer.')
param deploySearchConfig bool

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('Specifies the name of the index to create/update.')
param indexName string

@description('Specifies the file containing the JSON definition of the index to create/update.')
param indexDefinitionFilename string = '${indexName}.json'

@description('Specifies the base path for the index definitions.')
param indexDefinitionsBasePath string = 'infrastructure/templates/search/application/indexes'

@description('Specifies the name of the indexer to create/update.')
param indexerName string = '${indexName}-indexer'

@description('The URL of the Public site.')
param publicSiteUrl string

@description('Name of the searchable documents container in the Search storage account.')
param searchableDocumentsContainerName string = 'searchable-documents'

@description('A list of IP network rules to allow access to the Search Service from specific public internet IP address ranges.')
param searchServiceIpRules IpRange[]

@description('A list of IP network rules to allow access to the Search storage account from specific public internet IP address ranges.')
param storageIpRules IpRange[]

@description('The branch, tag or commit of the source code from which the deployment is triggered.')
param githubSourceRef string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource searchStoragePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.searchStoragePrivateEndpoints
  parent: vNet
}

var searchServiceName = '${resourcePrefix}-${abbreviations.searchSearchServices}'

module searchServiceModule '../components/searchService.bicep' = {
  name: 'searchServiceModuleDeploy'
  params: {
    name: searchServiceName
    location: location
    ipRules: searchServiceIpRules
    publicNetworkAccess: 'Enabled'
    sku: 'basic'
    systemAssignedIdentity: true
    alerts: {
      searchLatency: true
      searchQueriesPerSecond: true
      throttledSearchQueriesPercentage: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    tagValues: tagValues
  }
}

module searchStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: 'searchStorageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}search'
    publicNetworkAccessEnabled: true
    firewallRules: storageIpRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVault.name
    alerts: {
      availability: true
      latency: false
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    privateEndpointSubnetIds: {
      blob: searchStoragePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module searchStorageAccountBlobServiceModule '../../common/components/blobService.bicep' = {
  name: 'searchStorageAccountBlobServiceModuleDeploy'
  params: {
    storageAccountName: searchStorageAccountModule.outputs.storageAccountName
    containerNames: [searchableDocumentsContainerName]
  }
}

module searchServiceBlobRoleAssignmentModule '../../common/components/storageAccountRoleAssignment.bicep' = {
  name: '${searchServiceName}BlobRoleAssignmentModuleDeploy'
  params: {
    principalIds: [searchServiceModule.outputs.searchServiceIdentityPrincipalId]
    storageAccountName: searchStorageAccountModule.outputs.storageAccountName
    role: 'Storage Blob Data Reader'
  }
}

var gitHubRawContentBaseUrl = 'https://raw.githubusercontent.com/dfe-analytical-services/explore-education-statistics'

module searchServiceConfigModule '../components/searchServiceConfig.bicep' = if (deploySearchConfig) {
  name: 'searchServiceConfigModuleDeploy'
  params: {
    dataSourceName: 'azureblob-${searchableDocumentsContainerName}-datasource'
    dataSourceConnectionString: 'ResourceId=${searchStorageAccountModule.outputs.storageAccountId};'
    dataSourceContainerName: searchableDocumentsContainerName
    dataSourceType: 'azureblob'
    indexDefinitionUri: '${gitHubRawContentBaseUrl}/${githubSourceRef}/${indexDefinitionsBasePath}/${indexDefinitionFilename}'
    indexCorsAllowedOrigins: [
      'http://localhost:3000'
      'https://localhost:3000'
      publicSiteUrl
    ]
    indexerName: indexerName
    indexerScheduleInterval: 'PT5M'
    searchServiceName: searchServiceModule.outputs.searchServiceName
    location: location
  }
  dependsOn: [searchStorageAccountBlobServiceModule] // Ensures the searchable documents container exists
}

output searchableDocumentsContainerName string = searchableDocumentsContainerName
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchServiceIndexName string = indexName
output searchServiceIndexerName string = indexerName
output searchServiceName string = searchServiceModule.outputs.searchServiceName
output searchStorageAccountConnectionStringSecretName string = searchStorageAccountModule.outputs.connectionStringSecretName
output searchStorageAccountName string = searchStorageAccountModule.outputs.storageAccountName

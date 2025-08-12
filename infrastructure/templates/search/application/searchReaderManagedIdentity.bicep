import { abbreviations } from '../../common/abbreviations.bicep'

@description('Specifies the name of the Search Service.')
param searchServiceName string

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchServiceName
}

// User assigned managed identity that has read-only access to the Search Service.
// Resources that need access to the Search Service for querying search indexes can be associated with this identity.
resource searchReaderIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: '${resourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.searchSearchServices}-reader'
  location: location
  tags: tagValues
}

module searchReaderIdentityRoleAssignmentModule '../components/searchServiceRoleAssignment.bicep' = {
  name: 'searchReaderIdentityRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: searchService.name
    principalIds: [searchReaderIdentity.properties.principalId]
    role: 'Search Index Data Reader'
  }
}

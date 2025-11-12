import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.publicApi.apiAppIdentity
  location: location
  tags: tagValues
}

// TODO - commenting out for now, as we can't apply roles to Container Registries in other Resource Groups.
// When we have a shared Resource Group for our ACR with special privileges, we can start to use the Container App
// Identity again to pull Docker images, but for now we continue to use our shared SPN.
// module apiContainerAppAcrPullRoleAssignmentModule '../../components/containerRegistryRoleAssignment.bicep' = {
//   name: '${resourceNames.publicApi.apiAppIdentity}AcrPullRoleAssignmentDeploy'
//   scope: resourceGroup(resourceNames.existingResources.acrResourceGroup)
//   params: {
//     role: 'AcrPull'
//     containerRegistryName: resourceNames.existingResources.acr
//     principalIds: [apiContainerAppManagedIdentity.properties.principalId]
//   }
// }

module apiContainerAppSearchServiceRoleAssignmentModule '../../../common/components/search/searchServiceRoleAssignment.bicep' = {
  name: '${resourceNames.publicApi.apiAppIdentity}'SearchServiceRoleAssignmentDeploy'
  params: {
    searchServiceName: resourceNames.sharedResources.searchService
    principalIds: [apiContainerAppManagedIdentity.properties.principalId]
    role: 'Search Index Data Reader'
  }
}

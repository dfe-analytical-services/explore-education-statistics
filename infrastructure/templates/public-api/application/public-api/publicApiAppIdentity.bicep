import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.publicApi.apiAppIdentity
  location: location
  tags: tagValues
}

module apiContainerAppAcrPullRoleAssignmentModule '../../components/containerRegistryRoleAssignment.bicep' = {
  name: '${resourceNames.publicApi.apiAppIdentity}AcrPullRoleAssignmentDeploy'
  scope: resourceGroup(resourceNames.existingResources.acrResourceGroup)
  params: {
    role: 'AcrPull'
    containerRegistryName: resourceNames.existingResources.acr
    principalIds: [apiContainerAppManagedIdentity.properties.principalId]
  }
}

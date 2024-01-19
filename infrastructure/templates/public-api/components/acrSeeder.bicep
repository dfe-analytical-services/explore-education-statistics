@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@minLength(5)
@maxLength(50)
@description('Name of the azure container registry (must be globally unique)')
param containerRegistryName string = 'eesapiacr'

@description('Specifies the base docker container image to deploy.')
param containerSeedImage string = 'mcr.microsoft.com/azuredocs/aci-helloworld'

//Variables 
var containerImportName = '${resourcePrefix}ImportContainerImage'
var userIdentityName = '${resourcePrefix}-id-seeder'
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

//Resources 

//Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userIdentityName
  location: location
}

@description('This allows the managed identity of the seeder to access the registry, note scope is applied to the wider ResourceGroup not the ACR')
resource managedIdentityRBAC 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, managedIdentity.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

//Registry Seeder
@description('This module seeds the ACR with the public version of the app')
module acrImportImage 'br/public:deployment-scripts/import-acr:3.0.1' = {
  name: containerImportName
  params: {
    useExistingManagedIdentity: false
    managedIdentityName: managedIdentity.name
    existingManagedIdentityResourceGroupName: resourceGroup().name
    existingManagedIdentitySubId: az.subscription().subscriptionId
    acrName: containerRegistryName
    location: location
    images: array(containerSeedImage)
  }
}

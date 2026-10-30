@description('Names of the user-assigned managed identities to create.')
param identityNames string[]

@description('Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource identities 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = [
  for name in identityNames: {
    name: name
    location: location
    tags: tagValues
  }
]

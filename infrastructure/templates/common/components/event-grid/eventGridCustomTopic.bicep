import { IpRange } from '../../types.bicep'

@description('The resource name.')
@minLength(3)
@maxLength(50)
param name string

@description('Location for all resources.')
param location string

@description('A list IP network rules to allow access to the Search Service from specific public internet IP address ranges. These rules are applied only when \'publicNetworkAccess\' is \'Enabled\'.')
param ipRules IpRange[] = []

@description('Specifies whether to enable local authentication. Microsoft Entra access authentication is always enabled.')
param localAuthenticationEnabled bool = false

@description('Specifies whether traffic is allowed over the public interface.')
@allowed([
  'Disabled'
  'Enabled'
])
param publicNetworkAccess string = 'Disabled'

@description('Indicates whether the resource should have a system-assigned managed identity.')
param systemAssignedIdentity bool = false

@description('The name of a user-assigned managed identity to assign to the resource.')
param userAssignedIdentityName string = ''

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var identityType = systemAssignedIdentity
  ? (!empty(userAssignedIdentityName) ? 'SystemAssigned, UserAssigned' : 'SystemAssigned')
  : (!empty(userAssignedIdentityName) ? 'UserAssigned' : 'None')

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (!empty(userAssignedIdentityName)) {
  name: userAssignedIdentityName
}

resource topic 'Microsoft.EventGrid/topics@2025-02-15' = {
  name: name
  location: location
  identity: {
    type: identityType
    userAssignedIdentities: !empty(userAssignedIdentityName) ? { '${userAssignedIdentity.id}': {} } : null
  }
  properties: {
    minimumTlsVersionAllowed: '1.2'
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: publicNetworkAccess
    inboundIpRules: [
      for ipRule in ipRules: {
        action: 'Allow'
        ipMask: ipRule.cidr
      }
    ]
    disableLocalAuth: !localAuthenticationEnabled
    dataResidencyBoundary: 'WithinRegion'
  }
  tags: tagValues
}

output name string = topic.name

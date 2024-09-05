@description('Specifies the type of zone to create')
@allowed([
  'sites'
  'postgres'
])
param zoneType string

@description('Specifies the name of the VNet that this DNS Zone will be attached to')
param vnetName string

var zoneTypeToNames = {
  sites: 'privatelink.azurewebsites.net'
  postgres: 'privatelink.postgres.database.azure.com'
}

var zoneName = zoneTypeToNames[zoneType]

resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vnetName
}

// A DNS zone in which internal DNS records can be managed.
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: zoneName
  location: 'global'
  properties: {}
}

// A link which makes the internal DNS records within the DNS zone available to other resources on the VNet.
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${vnet.name}-${zoneName}-vnetlink'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

output privateDnsZoneId string = privateDnsZone.id
output privateDnsZoneName string = privateDnsZone.name

output privateDnsZoneLinkId string = privateDnsZoneLink.id
output privateDnsZoneLinkName string = privateDnsZoneLink.name

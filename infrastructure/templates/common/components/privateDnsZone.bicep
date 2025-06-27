import { PrivateDnsZone } from '../types.bicep'
import { dnsZones } from '../dnsZones.bicep'

@description('Specifies the type of zone to create')
param zoneType PrivateDnsZone

@description('Specifies an optional name for the zone, if "custom" zoneType was chosen')
param customName string?

@description('Specifies the name of the VNet that this DNS Zone will be attached to')
param vnetName string

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

var zoneName = zoneType == 'custom' ? customName : dnsZones[zoneType].zoneName

resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vnetName
}

// A DNS zone in which internal DNS records can be managed.
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: zoneName
  location: 'global'
  properties: {}
  tags: tagValues
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
  tags: tagValues
}

output privateDnsZoneId string = privateDnsZone.id
output privateDnsZoneName string = privateDnsZone.name

output privateDnsZoneLinkId string = privateDnsZoneLink.id
output privateDnsZoneLinkName string = privateDnsZoneLink.name

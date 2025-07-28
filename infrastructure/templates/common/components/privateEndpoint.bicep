import { PrivateDnsZone } from '../types.bicep'
import { dnsZones } from '../dnsZones.bicep'

@description('Specifies the name of the service being connected via private endpoint')
@minLength(0)
param serviceName string

@description('Specifies the resource id of the service being connected via private endpoint')
@minLength(0)
param serviceId string

@description('Specifies an optional name prefix for the private endpoint prior to appending "-pep" to the end')
@minLength(0)
param privateEndpointNameOverride string?

@description('Specifies the resource id of the subnet with which the service will be attached to the VNet')
@minLength(0)
param subnetId string

@description('Specifies the location for this resource.')
param location string

@description('Specifies the type of service being attached to the private endpoint')
param serviceType PrivateDnsZone

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var privateEndpointName = '${privateEndpointNameOverride ?? serviceName}-pep'
var privateDnsZoneName = dnsZones[serviceType].zoneName

// A private endpoint that establishes a link between a VNet and an Azure service
// that supports Private Link. This takes the form of an IP address that is
// resolvable by a private DNS zone.
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2024-01-01' = {
  name: privateEndpointName
  location: location
  tags: tagValues
  properties: {
    privateLinkServiceConnections: [
      {
        name: privateEndpointName
        properties: {
          privateLinkServiceId: serviceId
          groupIds: [
            dnsZones[serviceType].dnsGroup
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    subnet: {
      id: subnetId
    }
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' existing = {
  name: privateDnsZoneName
}

// The private DNS zone group establishes a hard connection between the service being
// connected and the DNS records in the private DNS zone. It handles updates to DNS
// records automatically.
resource privateDnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2024-01-01' = {
  name: 'default'
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: replace(privateDnsZoneName, '.', '-')
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

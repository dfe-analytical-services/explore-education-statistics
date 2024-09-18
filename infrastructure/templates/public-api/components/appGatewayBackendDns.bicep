import { appGatewaySiteConfigType } from '../types.bicep'

@description('Specifies the VNet name that the DNS records will be made available to')
param vnetName string

@description('Specifies the Key Vault name that this App Gateway will be permitted to get and list certificates from')
param site appGatewaySiteConfigType

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

module privateDnsZoneModule './privateDnsZone.bicep' = {
  name: '${site.backendDomainName}Deploy'
  params: {
    vnetName: vnetName
    zoneType: 'custom'
    customName: site.backendDomainName
    tagValues: tagValues
  }
}

resource dnsWildcardARecord 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  name: '${site.backendDomainName}/*'
  properties: {
    ttl: 3600
    aRecords: [
      {
        ipv4Address: site.backendIpAddress
      }
    ]
  }
  dependsOn: [
    privateDnsZoneModule
  ]
}

resource dnsAtARecord 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  name: '${site.backendDomainName}/@'
  properties: {
    ttl: 3600
    aRecords: [
      {
        ipv4Address: site.backendIpAddress
      }
    ]
  }
  dependsOn: [
    privateDnsZoneModule
  ]
}

resource dnsAtSoaRecord 'Microsoft.Network/privateDnsZones/SOA@2024-06-01' = {
  name: '${site.backendDomainName}/@'
  properties: {
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
  dependsOn: [
    privateDnsZoneModule
  ]
}

@description('Name of the vnet the private DNS zone should be connected to')
param vnetName string

@description('Domain name to use in the DNS records')
param domain string

@description('The IP address to use in the DNS A records')
param ipAddress string

@description('Tags to assign to resources')
param tagValues object

module privateDnsZoneModule '../../common/components/privateDnsZone.bicep' = {
  name: '${domain}Deploy'
  params: {
    vnetName: vnetName
    zoneType: 'custom'
    customName: domain
    tagValues: tagValues
  }
}

resource dnsWildcardARecord 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  name: '${domain}/*'
  properties: {
    ttl: 3600
    aRecords: [
      {
        ipv4Address: ipAddress
      }
    ]
  }
  dependsOn: [
    privateDnsZoneModule
  ]
}

resource dnsAtARecord 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  name: '${domain}/@'
  properties: {
    ttl: 3600
    aRecords: [
      {
        ipv4Address: ipAddress
      }
    ]
  }
  dependsOn: [
    privateDnsZoneModule
  ]
}

resource dnsAtSoaRecord 'Microsoft.Network/privateDnsZones/SOA@2024-06-01' = {
  name: '${domain}/@'
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

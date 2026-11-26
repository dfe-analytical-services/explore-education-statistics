import { IpRange, SubnetReference } from '../../types.bicep'

@description('Name of the SQL Server that this networking configuration belongs to.')
param sqlServerName string

@description('Specifies the location for this resource.')
param location string

@description('Whether or not public access is enabled for the SQL Server. Firewall and VNet rules only apply when this is Enabled.')
param sqlServerPublicNetworkAccess 'Enabled' | 'Disabled'

@description('Firewall rules for the SQL Server, applied when public network access is enabled.')
param firewallRules IpRange[] = []

@description('Subnets to allow direct VNet access from, applied when public network access is enabled.')
param allowedSubnets SubnetReference[] = []

@description('Id of the subnet to deploy a private endpoint into. Omit to skip deploying a private endpoint.')
param privateEndpointSubnetId string?

@description('Specifies an optional name for the private endpoint\'s private link service connection.')
@minLength(0)
param privateLinkServiceConnectionNameOverride string?

@description('Whether to create DNS zone groups on the private endpoints, which keep their A records in the matching private DNS zones in this resource group. Off in the ELZ, where private DNS zones are managed centrally.')
param deployPrivateDnsZoneGroups bool = true

@description('Tags for the resources')
param tagValues object

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' existing = {
  name: sqlServerName
}

resource firewallRuleResources 'Microsoft.Sql/servers/firewallRules@2025-01-01' = [
  for rule in firewallRules: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: rule.name
    properties: {
      startIpAddress: parseCidr(rule.cidr).firstUsable
      endIpAddress: parseCidr(rule.cidr).lastUsable
    }
  }
]

resource virtualNetworkRuleResources 'Microsoft.Sql/servers/virtualNetworkRules@2025-01-01' = [
  for allowedSubnet in allowedSubnets: if (sqlServerPublicNetworkAccess == 'Enabled') {
    parent: sqlServer
    name: allowedSubnet.name
    properties: {
      virtualNetworkSubnetId: allowedSubnet.id
      ignoreMissingVnetServiceEndpoint: false
    }
  }
]

module privateEndpointModule '../privateEndpoint.bicep' = if (privateEndpointSubnetId != null) {
  name: '${sqlServerName}PrivateEndpointDeploy'
  params: {
    serviceId: sqlServer.id
    serviceName: sqlServerName
    serviceType: 'azureSql'
    deployPrivateDnsZoneGroup: deployPrivateDnsZoneGroups
    subnetId: privateEndpointSubnetId!
    location: location
    privateLinkServiceConnectionNameOverride: privateLinkServiceConnectionNameOverride
    tagValues: tagValues
  }
}

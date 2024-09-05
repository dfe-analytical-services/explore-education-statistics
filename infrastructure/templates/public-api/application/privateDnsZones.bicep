@description('Specifies the name of the VNet that the DNS Zones will be attached to')
@minLength(0)
param vnetName string

// Set up a Private DNS zone for handling private endpoints for PostgreSQL resources.
module postgreSqlPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'postgresPrivateDnsZoneDeploy'
  params: {
    zoneType: 'postgres'
    vnetName: vnetName
  }
}

// Set up a Private DNS zone for handling private endpoints for site resources
// (e.g. App Services, Function Apps, Container Apps).
module sitesPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'sitesPrivateDnsZoneDeploy'
  params: {
    zoneType: 'sites'
    vnetName: vnetName
  }
}

output postgreSqlPrivateDnsZoneId string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneId
output postgreSqlPrivateDnsZoneName string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneName

output sitesPrivateDnsZoneId string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneId
output sitesPrivateDnsZoneName string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneName

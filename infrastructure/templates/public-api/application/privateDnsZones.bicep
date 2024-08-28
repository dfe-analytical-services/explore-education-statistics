@description('Specifies the resource id of the VNet that the DNS Zones will be attached to')
@minLength(0)
param vnetId string

@description('Private DNS zone for handling private endpoints for PostgreSQL resources.')
module postgreSqlPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'postgresPrivateDnsZoneDeploy'
  params: {
    zoneType: 'postgresqlServer'
    vnetId: vnetId
  }
}

@description('Private DNS zone for handling private endpoints for site resources (e.g. App Services, Function Apps, Container Apps).')
module sitesPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'sitesPrivateDnsZoneDeploy'
  params: {
    zoneType: 'sites'
    vnetId: vnetId
  }
}

output postgreSqlPrivateDnsZoneId string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneId
output postgreSqlPrivateDnsZoneName string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneName

output sitesPrivateDnsZoneId string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneId
output sitesPrivateDnsZoneName string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneName

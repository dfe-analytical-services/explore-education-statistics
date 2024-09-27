import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

// Set up a Private DNS zone for handling private endpoints for PostgreSQL resources.
module postgreSqlPrivateDnsZoneModule '../../components/privateDnsZone.bicep' = {
  name: 'postgresPrivateDnsZoneDeploy'
  params: {
    zoneType: 'postgres'
    vnetName: resourceNames.existingResources.vNet
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for site resources
// (e.g. App Services, Function Apps, Container Apps).
module sitesPrivateDnsZoneModule '../../components/privateDnsZone.bicep' = {
  name: 'sitesPrivateDnsZoneDeploy'
  params: {
    zoneType: 'sites'
    vnetName: resourceNames.existingResources.vNet
    tagValues: tagValues
  }
}

output postgreSqlPrivateDnsZoneId string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneId
output postgreSqlPrivateDnsZoneName string = postgreSqlPrivateDnsZoneModule.outputs.privateDnsZoneName

output sitesPrivateDnsZoneId string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneId
output sitesPrivateDnsZoneName string = sitesPrivateDnsZoneModule.outputs.privateDnsZoneName

@description('Specifies the name of the virtual network that DNS zones will be attached to.')
param vnetName string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

// Set up a Private DNS zone for handling private endpoints for PostgreSQL resources.
module postgreSqlPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'postgresPrivateDnsZoneDeploy'
  params: {
    zoneType: 'postgres'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for site resources
// (e.g. App Services, Function Apps, Container Apps).
module sitesPrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'sitesPrivateDnsZoneDeploy'
  params: {
    zoneType: 'sites'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account File Services.
module fileServicePrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'fileServicePrivateDnsZoneDeploy'
  params: {
    zoneType: 'fileService'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Blob Storage.
module blobStoragePrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'blobStoragePrivateDnsZoneDeploy'
  params: {
    zoneType: 'blobStorage'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Queues.
module queuePrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'queuePrivateDnsZoneDeploy'
  params: {
    zoneType: 'queue'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Table Storage.
module tableStoragePrivateDnsZoneModule '../components/privateDnsZone.bicep' = {
  name: 'tableStoragePrivateDnsZoneDeploy'
  params: {
    zoneType: 'tableStorage'
    vnetName: vnetName
    tagValues: tagValues
  }
}

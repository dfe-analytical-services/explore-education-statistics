@description('Specifies the name of the virtual network that DNS zones will be attached to.')
param vnetName string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

// Set up a Private DNS zone for handling private endpoints for Azure SQL resources.
module azureSqlPrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'azureSqlPrivateDnsZoneDeploy'
  params: {
    zoneType: 'azureSql'
    customVNetLinkName: '${vnetName}-database-vnetlink'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for PostgreSQL resources.
module postgreSqlPrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'postgresPrivateDnsZoneDeploy'
  params: {
    zoneType: 'postgres'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for site resources
// (e.g. App Services, Function Apps, Container Apps).
module sitesPrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'sitesPrivateDnsZoneDeploy'
  params: {
    zoneType: 'sites'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Event Grid custom topic resources.
module eventGridTopicPrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'eventGridTopicPrivateDnsZoneDeploy'
  params: {
    zoneType: 'eventGridTopic'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account File Services.
module fileServicePrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'fileServicePrivateDnsZoneDeploy'
  params: {
    zoneType: 'fileService'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Blob Storage.
module blobStoragePrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'blobStoragePrivateDnsZoneDeploy'
  params: {
    zoneType: 'blobStorage'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Queues.
module queuePrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'queuePrivateDnsZoneDeploy'
  params: {
    zoneType: 'queue'
    vnetName: vnetName
    tagValues: tagValues
  }
}

// Set up a Private DNS zone for handling private endpoints for Storage Account Table Storage.
module tableStoragePrivateDnsZoneModule '../../../common/components/private-dns-zone/private-dns-zone.bicep' = {
  name: 'tableStoragePrivateDnsZoneDeploy'
  params: {
    zoneType: 'tableStorage'
    vnetName: vnetName
    tagValues: tagValues
  }
}

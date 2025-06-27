@export()
var dnsZones = {
  blobStorage: {
    zoneName: 'privatelink.blob.${environment().suffixes.storage}'
    dnsGroup: 'blob'
  }
  eventGridTopic: {
    zoneName: 'privatelink.eventgrid.azure.net'
    dnsGroup: 'topic'
  }
  fileService: {
    zoneName: 'privatelink.file.${environment().suffixes.storage}'
    dnsGroup: 'file'
  }
  postgres: {
    zoneName: 'privatelink.postgres.database.azure.com'
    dnsGroup: 'postgresqlServer'
  }
  queue: {
    zoneName: 'privatelink.queue.${environment().suffixes.storage}'
    dnsGroup: 'queue'
  }
  sites: {
    zoneName: 'privatelink.azurewebsites.net'
    dnsGroup: 'sites'
  }
  tableStorage: {
    zoneName: 'privatelink.table.${environment().suffixes.storage}'
    dnsGroup: 'table'
  }
}

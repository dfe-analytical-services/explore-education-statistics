@description('Name of the Data Factory instance that owns these private endpoints.')
param dataFactoryName string

@description('Name of the Core Azure SQL server.')
param coreSqlServerName string

var coreSqlServerPrivateEndpointName = '${coreSqlServerName}-pep'

resource coreSqlServerPrivateEndpoint 'Microsoft.DataFactory/factories/managedVirtualNetworks/managedPrivateEndpoints@2018-06-01' = {
  name: '${dataFactoryName}/default/${coreSqlServerPrivateEndpointName}'
  properties: {
    privateLinkResourceId: resourceId('Microsoft.Sql/servers', coreSqlServerName)
    groupId: 'sqlServer'
    fqdns: [
      '${coreSqlServerName}.${environment().suffixes.sqlServerHostname}'
    ]
  }
}

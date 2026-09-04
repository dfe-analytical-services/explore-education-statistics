@description('Name of the Data Factory instance that owns these linked services.')
param dataFactoryName string

var statisticsDatabaseLinkedServiceName = 'ls_sql_statistics'

resource statisticsDatabaseLinkedService 'Microsoft.DataFactory/factories/linkedServices@2018-06-01' = {
  name: '${dataFactoryName}/${statisticsDatabaseLinkedServiceName}'
  properties: {
    annotations: []
    type: 'AzureSqlDatabase'
    typeProperties: {
      connectionString: {
        type: 'AzureKeyVaultSecret'
        store: {
          referenceName: 'AzureKeyVault'
          type: 'LinkedServiceReference'
        }
        secretName: 'ees-sql-admin-datafactory-connectionstring'
      }
    }
    connectVia: {
      referenceName: 'vnetIntegrationRuntime'
      type: 'IntegrationRuntimeReference'
    }
  }
}

resource statisticsDatabaseDataset 'Microsoft.DataFactory/factories/datasets@2018-06-01' = {
  name: '${dataFactoryName}/ds_sql_statistics'
  properties: {
    linkedServiceName: {
      referenceName: statisticsDatabaseLinkedServiceName
      type: 'LinkedServiceReference'
    }
    parameters: {
      cw_table: {
        type: 'String'
      }
    }
    annotations: []
    type: 'AzureSqlTable'
    schema: []
    typeProperties: {
      schema: 'dbo'
      table: {
        value: '@dataset().cw_table'
        type: 'Expression'
      }
    }
  }
  dependsOn: [
    statisticsDatabaseLinkedService
  ]
}

output statisticsDbLinkedServiceName string = statisticsDatabaseLinkedServiceName

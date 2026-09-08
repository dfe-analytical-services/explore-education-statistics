import { AzureFileShareMount } from 'types.bicep'

@description('Name of the App Service or Function App to mount the file share on.')
param siteName string

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

resource fileShareMount 'Microsoft.Web/sites/config@2025-03-01' = if (length(azureFileShares) > 0) {
  name: '${siteName}/azurestorageaccounts'
  properties: reduce(
    azureFileShares,
    {},
    (cur, next) =>
      union(cur, {
        '${next.storageName}': {
          type: 'AzureFiles'
          shareName: next.fileShareName
          mountPath: next.mountPath
          accountName: next.storageAccountName
          accessKey: next.storageAccountKey
        }
      })
  )
}

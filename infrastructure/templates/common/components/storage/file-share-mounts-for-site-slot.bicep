import { AzureFileShareMount } from 'types.bicep'

@description('Name of the App Service or Function App that owns the swap slot.')
param siteName string

@description('Name of the swap slot to mount the file share on.')
param slotName string

@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

resource fileShareMount 'Microsoft.Web/sites/slots/config@2025-03-01' = if (length(azureFileShares) > 0) {
  name: '${siteName}/${slotName}/azurestorageaccounts'
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

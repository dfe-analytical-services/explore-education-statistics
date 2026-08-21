@description('Name of the App Service to mount the file share on.')
param appServiceName string

@description('Name of the Storage Account that the file share belongs to.')
param storageAccountName string

@description('Name of the File Share to mount.')
param fileShareName string

@description('Path on which to mount the File Share.')
param fileShareMountPath string

resource fileShareMount 'Microsoft.Web/sites/config@2025-03-01' = {
  name: '${appServiceName}/azurestorageaccounts'
  properties: {
    '${fileShareName}': {
      type: 'AzureFiles'
      accountName: storageAccountName
      accessKey: listKeys(resourceId('Microsoft.Storage/storageAccounts', storageAccountName), '2018-02-01').keys[0].value
      shareName: fileShareName
      mountPath: fileShareMountPath
      protocol: 'Smb'
    }
  }
}

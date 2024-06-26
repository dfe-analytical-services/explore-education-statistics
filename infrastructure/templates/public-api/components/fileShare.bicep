@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Size in GB of the file share')
param fileShareQuota int = 6

@description('Name of the file share')
param fileShareName string

@description('Type of the file share access tier')
@allowed(['Cool','Hot','Premium','TransactionOptimized'])
param fileShareAccessTier string = 'Hot'

@description('Name of the Storage Account')
param storageAccountName string

var shareName = '${resourcePrefix}-fs-${fileShareName}'

// Reference an existing Storage Account.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  name:  shareName
  parent: fileService
  properties: {
    accessTier: fileShareAccessTier
    shareQuota: fileShareQuota
  }
}

output fileShareName string = fileShare.name

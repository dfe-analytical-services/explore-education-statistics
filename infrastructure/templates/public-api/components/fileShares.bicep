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

// Variables and created data
var shareName = '${resourcePrefix}-fs-${fileShareName}'

//Resources 

//Use Storage Account created earlier
resource apiStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

// Fileservice
resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2022-09-01' = {
  name: 'default'
  parent: apiStorageAccount
}


// FileShare
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01' = {
  name:  shareName
  parent: fileService
  properties: {
    accessTier: fileShareAccessTier
    shareQuota: fileShareQuota
  }
}


//Outputs
output fileShareName string = fileShare.name

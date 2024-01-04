@description('Storage : Size in GB of the file share')
param fileShareQuota int = 6

@description('Storage : Name of the file share')
param fileShareName string = 'data'

@description('Storage : Type of the file share access tier')
@allowed(['Cool','Hot','Premium','TransactionOptimized']) //only Premium for FileShare
param fileShareAccessTier string = 'Hot'

@description('Storage : Name of the Storage Account')
param storageAccountName string

// Variables and created data



//Resources 

//Use Storage Account created earlier
resource ApiStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

// Fileservice
resource ApifileService 'Microsoft.Storage/storageAccounts/fileServices@2022-09-01' = {
  name: 'default'
  parent: ApiStorageAccount
}


// FileShare
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01' = {
  name:  fileShareName
  parent: ApifileService
  properties: {
    accessTier: fileShareAccessTier
    shareQuota: fileShareQuota
  }
}


//Outputs
output fileShareName string = fileShare.name

@export()
type AzureFileShareMount = {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}

@export()
type StorageAccountPrivateEndpoints = {
  file: string?
  blob: string?
  queue: string?
  table: string?
}

@export()
type StorageAccountConfig = {
  sku: 'Standard_LRS' | 'Premium_LRS' | 'Premium_ZRS'
  kind: 'StorageV2' | 'FileStorage'
  fileShare: {
    quotaGbs: int
    accessTier: 'Cool' | 'Hot' | 'TransactionOptimized' | 'Premium'
  }
}

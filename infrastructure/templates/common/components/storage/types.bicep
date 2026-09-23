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
type StorageAccountSku = 
  | 'Standard_LRS'
  | 'StandardV2_LRS'
  | 'Standard_GRS'
  | 'StandardV2_GRS'
  | 'Standard_RAGRS'
  | 'Standard_ZRS'
  | 'StandardV2_ZRS'
  | 'Premium_LRS'
  | 'Premium_ZRS'
  | 'Standard_GZRS'
  | 'StandardV2_GZRS'
  | 'Standard_RAGZRS'

@export()
type StorageAccountKind = 
  | 'StorageV2'
  | 'FileStorage'

@export()
type StorageAccountConfig = {
  sku: StorageAccountSku
  kind: StorageAccountKind
  fileShare: {
    quotaGbs: int
    accessTier: 'Cool' | 'Hot' | 'TransactionOptimized' | 'Premium'
  }
}

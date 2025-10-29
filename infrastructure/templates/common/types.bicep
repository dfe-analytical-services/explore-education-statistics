@export()
type AzureFileShareMount = {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}

@export()
type IpRange = {
  name: string
  cidr: string
}

@export()
type FirewallRule = {
  name: string
  cidr: string
  priority: int
  tag: string
}

@export()
type PrivateDnsZone =
  | 'blobStorage'
  | 'eventGridTopic'
  | 'fileService'
  | 'postgres'
  | 'queue'
  | 'sites'
  | 'tableStorage'
  | 'custom'

@export()
type StorageAccountConfig = {
  sku: 'Standard_LRS' | 'Premium_LRS' | 'Premium_ZRS'
  kind: 'StorageV2' | 'FileStorage'
  fileShare: {
    quotaGbs: int
    accessTier: 'Cool' | 'Hot' | 'TransactionOptimized' | 'Premium'
  }
}

@description('Name of the storage account.')
param storageAccountName string

@description('Retention of blobs in days.')
param blobDeleteRetentionDays int

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

module storageAccountModule '../../../common/components/storage/storage-account.bicep' = {
  name: 'loggingStorageAccountDeploy'
  params: {
    storageAccountName: storageAccountName
    kind: 'StorageV2'
    sku: 'Standard_GZRS'
    publicNetworkAccessEnabled: true
    alerts: deployAlerts ? {
      alertsGroupName: alertsGroupName
      availability: true
      latency: false
    } : null
    tagValues: tagValues
  }
}

module blobServiceModule '../../../common/components/blobService.bicep' = {
  name: 'loggingStorageAccountBlobServiceDeploy'
  params: {
    storageAccountName: storageAccountModule.outputs.storageAccountName
    deleteRetentionPolicy: blobDeleteRetentionDays
  }
}

output storageAccountName string = storageAccountName

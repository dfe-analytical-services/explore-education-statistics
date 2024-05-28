@description('Specifies the Web / Function App name that these settings belong to')
param appName string

@description('Specifies the names of slot settings (settings that stick to their slots rather than swap)')
param slotSpecificSettingKeys string[]

@description('Specifies a set of appsettings that are common to both the production and staging slots')
param commonSettings object

@description('Specifies a set of appsettings that are specific to the production slot')
param prodOnlySettings object

@description('Specifies a set of appsettings that are specific to the staging slot')
param stagingOnlySettings object

@description('Specifies the name of the staging slot')
param stagingSlotName string = 'staging'

@description('Specifies any existing appsettings from the staging slot')
param existingStagingAppSettings object

@description('Specifies any existing appsettings from the production slot')
param existingProductionAppSettings object

@description('Specifies additional Azure Storage Accounts to make available to the staging slot')
param azureFileShares {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}[] = []

@description('Set specific appsettings to be slot specific values')
resource functionSlotConfig 'Microsoft.Web/sites/config@2023-01-01' = {
  name: '${appName}/slotConfigNames'
  properties: {
    appSettingNames: slotSpecificSettingKeys
  }
}

// Combine common settings, slot-specific settings and any existing settings together.
// Existing settings take precedence over settings specified in the Bicep files so that
// infrastructure deploys do not reset appsettings back to original values and cause
// unwanted updates to production appsettings prior to a slot swap deploy process being
// ready to run.
//
// See https://blog.dotnetstudio.nl/posts/2021/04/merge-appsettings-with-bicep.
var combinedStagingSettings = union(commonSettings, stagingOnlySettings, existingStagingAppSettings)
var combinedProductionSettings = union(commonSettings, prodOnlySettings, existingProductionAppSettings)

@description('Set appsettings on the staging slot')
resource appStagingSlotSettings 'Microsoft.Web/sites/slots/config@2023-01-01' = {
  name: '${appName}/${stagingSlotName}/appsettings'
  properties: combinedStagingSettings
}

resource azureStorageAccounts 'Microsoft.Web/sites/slots/config@2021-01-15' = {
  name: '${appName}/${stagingSlotName}/azurestorageaccounts'
  properties: reduce(azureFileShares, {}, (cur, next) => union(cur, {
    '${next.storageName}': {
      type: 'AzureFiles'
      shareName: next.fileShareName
      mountPath: next.mountPath
      accountName: next.storageAccountName
      accessKey: next.storageAccountKey
    }
  }))
}

@description('Set appsettings on production slot')
resource appProductionSettings 'Microsoft.Web/sites/config@2023-01-01' = {
  name: '${appName}/appsettings'
  properties: combinedProductionSettings
}

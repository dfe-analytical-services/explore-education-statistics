param appName string
param location string
param slotSpecificSettingKeys string[]
param baseSettings object
param prodOnlySettings object
param stagingOnlySettings object
param appServiceExists boolean

var existingStagingAppSettings = appServiceExists ? list(resourceId('Microsoft.Web/sites/slots/config', appName, 'staging', 'appsettings'), '@2021-03-01').properties) : {}
var existingProductionAppSettings = appServiceExists ? list(resourceId('Microsoft.Web/sites/config', appName, 'appsettings'), '2021-03-01').properties) : {}

// Create a staging slot.
resource stagingSlot 'Microsoft.Web/sites/slots@2021-03-01' = {
  name: '${appName}/staging'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    httpsOnly: true
  }
}

@description('Set specific app settings to be slot specific values')
resource functionSlotConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${appName}/slotConfigNames'
  properties: {
    appSettingNames: slotSpecificSettingKeys
  }
}

@description('Set app settings on staging slot')
resource appStagingSlotSettings 'Microsoft.Web/sites/slots/config@2021-03-01' = {
  name: 'appsettings'
  parent: stagingSlot
  properties: union(existingStagingAppSettings, baseSettings, stagingOnlySettings)
}

@description('Set app settings on production slot')
resource appProductionSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${appName}/appsettings'
  properties: union(existingProductionAppSettings, baseSettings, prodOnlySettings)
}

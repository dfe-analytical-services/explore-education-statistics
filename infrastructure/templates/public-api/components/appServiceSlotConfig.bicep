param appName string
param location string
param slotSpecificSettingKeys string[]
param baseSettings object
param prodOnlySettings object
param stagingOnlySettings object
param existingStagingAppSettings object
param existingProductionAppSettings object

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

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
  tags: tagValues
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
  properties: union(baseSettings, stagingOnlySettings, existingStagingAppSettings)
}

@description('Set app settings on production slot')
resource appProductionSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${appName}/appsettings'
  properties: union(baseSettings, prodOnlySettings, existingProductionAppSettings)
}

output stagingSlotPrincipalId string = stagingSlot.identity.principalId
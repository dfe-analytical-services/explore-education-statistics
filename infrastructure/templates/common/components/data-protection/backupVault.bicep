@description('Name of the backup vault.')
param name string

@description('Redundancy type of the vault.')
@allowed([
  'LocallyRedundant'
  'ZoneRedundant'
  'GeoRedundant'
])
param vaultStorageRedundancy string = 'GeoRedundant'

@description('Whether backups can be restored in the paired region. Needs geo-redundant storage. Once protection has started, the vault can\'t then move off geo-redundant storage.')
param crossRegionRestoreEnabled bool = false

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource vault 'Microsoft.DataProtection/backupVaults@2026-06-01' = {
  name: name
  location: location
  identity: {
    type: 'systemAssigned'
  }
  properties: union({
    storageSettings: [
      {
        datastoreType: 'VaultStore'
        type: vaultStorageRedundancy
      }
    ]
  }, crossRegionRestoreEnabled
    ? {
        featureSettings: {
          crossRegionRestoreSettings: {
            state: 'Enabled'
          }
        }
      }
    : {})
  tags: tagValues
}

output principalId string = vault.identity.principalId

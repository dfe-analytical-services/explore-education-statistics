@description('Name of the backup vault.')
param name string

@description('Redundancy type of the vault.')
@allowed([
  'LocallyRedundant'
  'ZoneRedundant'
  'GeoRedundant'
])
param vaultStorageRedundancy string = 'GeoRedundant'

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource vault 'Microsoft.DataProtection/backupVaults@2022-05-01' = {
  name: name
  location: location
  identity: {
    type: 'systemAssigned'
  }
  properties: {
    storageSettings: [
      {
        datastoreType: 'VaultStore'
        type: vaultStorageRedundancy
      }
    ]
  }
  tags: tagValues
}

output principalId string = vault.identity.principalId

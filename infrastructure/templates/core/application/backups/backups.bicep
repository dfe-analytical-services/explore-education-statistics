@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param resourcePrefix string = resourceGroup().location

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

module backupVaultModule 'backupVault.bicep' = {
  name: 'backupVaultModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    tagValues: tagValues
  }
}

module backupBlobsPolicyModule 'backupVaultBlobsPolicy.bicep' = {
  name: 'backupVaultBlobsPolicyModuleDeploy'
  params: {
    vaultName: backupVaultModule.outputs.vaultName
    resourcePrefix: resourcePrefix
  }
}

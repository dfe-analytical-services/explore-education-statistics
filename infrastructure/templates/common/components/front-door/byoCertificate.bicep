@description('Resource prefix for legacy resources.')
param legacyResourcePrefix string

@description('Name of the Azure Front Door instance.')
param frontDoorName string

@description('Hostname used to reach Azure Front Door and its protected origin.')
param siteHostName string

@description('Name of the certificate.')
param certificateName string

var keyVaultName = '${legacyResourcePrefix}kv-ees-01'

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' existing = {
  name: frontDoorName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

module keyVaultRoleAssignmentModule '../../../common/components/key-vault/keyVaultRoleAssignment.bicep' = {
  name: '${frontDoorName}KeyVaultRoleAssignmentModuleDeploy'
  params: {
    principalIds: [frontDoor.identity.principalId]
    keyVaultName: keyVaultName
    role: 'Certificate User'
  }
}

resource certificateSecret 'Microsoft.Cdn/profiles/secrets@2025-04-15' = {
  parent: frontDoor
  name: '${certificateName}-latest'
  properties: {
    parameters: {
      type: 'CustomerCertificate'
      secretSource: {
        id: '${keyVault.id}/secrets/${certificateName}'
      }
      useLatestVersion: true
      subjectAlternativeNames: [
        siteHostName
      ]
    }
  }
  dependsOn: [
    keyVaultRoleAssignmentModule
  ]
}

output certificateSecretId string = certificateSecret.id

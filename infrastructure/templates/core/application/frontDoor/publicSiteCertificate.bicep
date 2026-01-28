@description('.')
param frontDoorName string

@description('.')
param keyVaultName string

@description('Resource prefix for all resources.')
param certificateName string

@description('URL of the public site.')
param publicSiteHostName string

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
        publicSiteHostName
      ]
    }
  }
  dependsOn: [
    keyVaultRoleAssignmentModule
  ]
}

output certificateSecretId string = certificateSecret.id

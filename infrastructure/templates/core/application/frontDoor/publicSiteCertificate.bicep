@description('Resource prefix for legacy resources.')
param legacyResourcePrefix string

@description('.')
param frontDoorName string

@description('URL of the public site.')
param publicSiteHostName string

var keyVaultName = '${legacyResourcePrefix}kv-ees-01'

// TODO EES-6883 - remove the "afd" from the line below once we're ready to
// switch DNS over to point at Azure Front Door rather than the Public Site
// App Service.  During testing we will host the public site through AFD on
// a temporary https://<env name>afd.explore-education-statistics.service.gov.uk
// URL with an associated certificate so as not to break the use of the environment
// for others.
var certificateName = '${legacyResourcePrefix}as-ees-public-site-afd-certificate'

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

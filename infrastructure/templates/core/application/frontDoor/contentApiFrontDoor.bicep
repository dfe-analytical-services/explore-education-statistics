import { abbreviations } from '../../../common/abbreviations.bicep'
import { FrontDoorCertificateType } from '../../../common/components/front-door/types.bicep'

@description('Environment subscription prefix.')
param subscription string

@description('Name of the existing Azure Front Door profile.')
param frontDoorProfileName string

@description('Name of the existing Azure Front Door endpoint.')
param frontDoorEndpointName string

@description('Name of the Key Vault containing the Content API certificate.')
param keyVaultName string

@description('Public hostname of the Content API.')
param contentApiHostName string

@description('App Service hostname used as the Content API origin.')
param contentApiOriginHostName string

@description('Name of the storage account containing published downloads.')
param publicStorageAccountName string

@description('Certificate type used by Azure Front Door.')
param certificateType FrontDoorCertificateType

var contentApiResourcePrefix = '${subscription}-ees-content'
var customDomainName = '${contentApiResourcePrefix}-${abbreviations.frontDoorDomains}'
var certificateName = '${subscription}-as-ees-content-certificate'
var wafPolicyName = '${replace(frontDoorProfileName, '-', '')}${abbreviations.frontDoorWafPolicies}'

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' existing = {
  name: frontDoorProfileName
}

resource endpoint 'Microsoft.Cdn/profiles/afdendpoints@2025-04-15' existing = {
  parent: frontDoor
  name: frontDoorEndpointName
}

resource originGroup 'Microsoft.Cdn/profiles/origingroups@2025-04-15' = {
  parent: frontDoor
  name: '${contentApiResourcePrefix}-${abbreviations.frontDoorOriginGroups}'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    sessionAffinityState: 'Disabled'
  }
}

resource origin 'Microsoft.Cdn/profiles/origingroups/origins@2025-04-15' = {
  parent: originGroup
  name: '${contentApiResourcePrefix}-${abbreviations.frontDoorOrigins}'
  properties: {
    hostName: contentApiOriginHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: contentApiOriginHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

module certificateModule '../../../common/components/front-door/byoCertificate.bicep' = if (certificateType == 'BringYourOwn') {
  name: '${contentApiResourcePrefix}CertificateModuleDeploy'
  params: {
    keyVaultName: keyVaultName
    frontDoorName: frontDoorProfileName
    siteHostName: contentApiHostName
    certificateName: certificateName
  }
}

resource customDomainWithCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'BringYourOwn') {
  parent: frontDoor
  name: customDomainName
  properties: {
    hostName: contentApiHostName
    tlsSettings: {
      certificateType: 'CustomerCertificate'
      minimumTlsVersion: 'TLS12'
      cipherSuiteSetType: 'TLS12_2023'
      secret: {
        id: certificateModule!.outputs.certificateSecretId
      }
    }
  }
}

resource customDomainWithManagedCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'Provisioned') {
  parent: frontDoor
  name: customDomainName
  properties: {
    hostName: contentApiHostName
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
      cipherSuiteSetType: 'TLS12_2023'
    }
  }
}

resource route 'Microsoft.Cdn/profiles/afdendpoints/routes@2025-04-15' = {
  parent: endpoint
  name: '${contentApiResourcePrefix}-${abbreviations.frontDoorRoutes}'
  properties: {
    customDomains: [
      {
        id: certificateType == 'BringYourOwn' ? customDomainWithCertificate.id : customDomainWithManagedCertificate.id
      }
    ]
    originGroup: {
      id: originGroup.id
    }
    ruleSets: []
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Disabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
  }
  dependsOn: [
    origin
  ]
}

resource publicStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: publicStorageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  parent: publicStorageAccount
  name: 'default'
}

resource downloadsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: blobService
  name: 'downloads'
}

var storageBlobDataReaderRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
)

resource blobDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(downloadsContainer.id, frontDoor.id, storageBlobDataReaderRoleId)
  scope: downloadsContainer
  properties: {
    roleDefinitionId: storageBlobDataReaderRoleId
    principalId: frontDoor.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource blobOriginGroup 'Microsoft.Cdn/profiles/origingroups@2026-04-01-preview' = {
  parent: frontDoor
  name: '${contentApiResourcePrefix}-downloads-${abbreviations.frontDoorOriginGroups}'
  properties: {
    authentication: {
      scope: 'https://storage.azure.com/.default'
      tokenDestinationHeader: 'Authorization'
      type: 'SystemAssignedIdentity'
    }
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    sessionAffinityState: 'Disabled'
  }
  dependsOn: [
    blobDataReaderRoleAssignment
  ]
}

var blobOriginHostName = '${publicStorageAccountName}.blob.${environment().suffixes.storage}'

resource blobOrigin 'Microsoft.Cdn/profiles/origingroups/origins@2026-04-01-preview' = {
  parent: blobOriginGroup
  name: '${contentApiResourcePrefix}-downloads-${abbreviations.frontDoorOrigins}'
  properties: {
    hostName: blobOriginHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: blobOriginHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

resource downloadsRoute 'Microsoft.Cdn/profiles/afdendpoints/routes@2026-04-01-preview' = {
  parent: endpoint
  name: '${contentApiResourcePrefix}-downloads-${abbreviations.frontDoorRoutes}'
  properties: {
    customDomains: [
      {
        id: certificateType == 'BringYourOwn' ? customDomainWithCertificate.id : customDomainWithManagedCertificate.id
      }
    ]
    originGroup: {
      id: blobOriginGroup.id
    }
    ruleSets: []
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/downloads/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Disabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
  }
  dependsOn: [
    blobOrigin
  ]
}

module wafSecurityPolicyModule '../../../common/components/front-door/wafSecurityPolicy.bicep' = {
  name: '${contentApiResourcePrefix}WafSecurityPolicyModuleDeploy'
  params: {
    securityPolicyName: '${replace(contentApiResourcePrefix, '-', '')}${abbreviations.frontDoorWafSecurityPolicies}'
    wafPolicyName: wafPolicyName
    customDomainName: customDomainName
    frontDoorProfileName: frontDoorProfileName
  }
  dependsOn: [
    customDomainWithCertificate
    customDomainWithManagedCertificate
  ]
}

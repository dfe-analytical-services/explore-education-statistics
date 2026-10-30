import { ResourceNames } from '../../../common/resource-names.bicep'

@description('Names of the Content API Front Door child resources.')
param contentApiNames ResourceNames.frontDoor.contentApi

@description('Name of the existing Azure Front Door profile.')
param frontDoorProfileName string

@description('Name of the existing Azure Front Door endpoint.')
param frontDoorEndpointName string

@description('Name of the Key Vault containing the Content API certificate.')
param keyVaultName string

@description('Name of the Content API certificate in Key Vault.')
param certificateName string

@description('Public hostname of the Content API.')
param contentApiHostName string

@description('App Service hostname used as the Content API origin.')
param contentApiOriginHostName string

@description('Name of the existing Publisher Function App granted permission to purge cached ZIPs.')
param publisherFunctionName string

@description('Whether to grant the Publisher Function App\'s system-assigned identity its roles here.')
param deploySystemAssignedIdentityRoleAssignments bool

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' existing = {
  name: frontDoorProfileName
}

resource endpoint 'Microsoft.Cdn/profiles/afdendpoints@2025-04-15' existing = {
  parent: frontDoor
  name: frontDoorEndpointName
}

resource publisherFunction 'Microsoft.Web/sites@2024-04-01' existing = {
  name: publisherFunctionName
}

// TODO EES-7502 - remove this grant, the publisherFunction lookup and the publisherFunctionName and
// deploySystemAssignedIdentityRoleAssignments params once every environment has moved to the ELZ, where the release
// pipeline grants this role to the Publisher's user-assigned identity instead.
module publisherFrontDoorRoleAssignment '../../../common/components/front-door/afd-endpoint-role-assignment.bicep' = if (deploySystemAssignedIdentityRoleAssignments) {
  name: 'contentApiPublisherRoleAssignmentModuleDeploy'
  params: {
    frontDoorProfileName: frontDoorProfileName
    frontDoorEndpointName: frontDoorEndpointName
    principalResourceId: publisherFunction.id
    principalId: publisherFunction.identity.principalId
  }
}

resource originGroup 'Microsoft.Cdn/profiles/origingroups@2025-04-15' = {
  parent: frontDoor
  name: contentApiNames.originGroup
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
  name: contentApiNames.origin
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

module certificateModule '../../../common/components/front-door/byoCertificate.bicep' = {
  name: 'contentApiCertificateModuleDeploy'
  params: {
    keyVaultName: keyVaultName
    frontDoorName: frontDoorProfileName
    siteHostName: contentApiHostName
    certificateName: certificateName
  }
}

resource customDomainWithCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = {
  parent: frontDoor
  name: contentApiNames.customDomain
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

resource route 'Microsoft.Cdn/profiles/afdendpoints/routes@2025-04-15' = {
  parent: endpoint
  name: contentApiNames.route
  properties: {
    customDomains: [
      {
        id: customDomainWithCertificate.id
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

resource allFilesZipCacheRoute 'Microsoft.Cdn/profiles/afdendpoints/routes@2025-04-15' = {
  parent: endpoint
  name: contentApiNames.allFilesCacheRoute
  properties: {
    customDomains: [
      {
        id: customDomainWithCertificate.id
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
      '/api/all-files/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Disabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
    cacheConfiguration: {
      queryStringCachingBehavior: 'IgnoreQueryString'
      compressionSettings: {
        isCompressionEnabled: false
        contentTypesToCompress: []
      }
    }
  }
  dependsOn: [
    origin
  ]
}

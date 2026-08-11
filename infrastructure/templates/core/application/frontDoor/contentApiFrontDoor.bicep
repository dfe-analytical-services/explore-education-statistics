import { abbreviations } from '../../../common/abbreviations.bicep'

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

var contentApiResourcePrefix = '${subscription}-ees-content'
var customDomainName = '${contentApiResourcePrefix}-${abbreviations.frontDoorDomains}'
var certificateName = '${subscription}-as-ees-content-afd-certificate'

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

module certificateModule '../../../common/components/front-door/byoCertificate.bicep' = {
  name: '${contentApiResourcePrefix}CertificateModuleDeploy'
  params: {
    keyVaultName: keyVaultName
    frontDoorName: frontDoorProfileName
    siteHostName: contentApiHostName
    certificateName: certificateName
  }
}

resource customDomainWithCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = {
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

resource route 'Microsoft.Cdn/profiles/afdendpoints/routes@2025-04-15' = {
  parent: endpoint
  name: '${contentApiResourcePrefix}-${abbreviations.frontDoorRoutes}'
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
  name: '${contentApiResourcePrefix}-all-files-cache-${abbreviations.frontDoorRoutes}'
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

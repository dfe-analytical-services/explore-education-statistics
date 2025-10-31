import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param resourcePrefix string = resourceGroup().location

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var frontDoorName = '${resourcePrefix}-${abbreviations.frontDoorProfiles}'

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' = {
  name: frontDoorName
  location: location
  tags: tagValues
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  kind: 'frontdoor'
  properties: {
    originResponseTimeoutSeconds: 60 // @MarkFix maybe want longer - for table tool exclusively?
  }
}

resource endpoints 'Microsoft.Cdn/profiles/afdendpoints@2025-04-15' = {
  parent: frontDoor
  name: '${resourcePrefix}-${abbreviations.frontDoorEndpoints}'
  location: location
  tags: tagValues
  properties: {
    enabledState: 'Enabled'
  }
}

resource originGroup 'Microsoft.Cdn/profiles/origingroups@2025-04-15' = {
  parent: frontDoor
  name: '${resourcePrefix}-${abbreviations.frontDoorOriginGroups}'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 100
    }
    sessionAffinityState: 'Disabled'
  }
}

resource origin 'Microsoft.Cdn/profiles/origingroups/origins@2025-04-15' = {
  parent: originGroup
  name: '${resourcePrefix}-${abbreviations.frontDoorOrigins}'
  properties: {
    hostName: '${resourcePrefix}-as-ees-public-site.azurewebsites.net'
    httpPort: 80
    httpsPort: 443
    originHostHeader: '${resourcePrefix}-as-ees-public-site.azurewebsites.net'
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
  dependsOn: [
    frontDoor
  ]
}

resource route 'Microsoft.Cdn/profiles/afdendpoints/routes@2025-04-15' = {
  parent: endpoints
  name: '${resourcePrefix}-${abbreviations.frontDoorRoutes}'
  properties: {
    cacheConfiguration: {
      compressionSettings: {
        isCompressionEnabled: true
        contentTypesToCompress: [
          'application/eot'
          'application/font'
          'application/font-sfnt'
          'application/javascript'
          'application/json'
          'application/opentype'
          'application/otf'
          'application/pkcs7-mime'
          'application/truetype'
          'application/ttf'
          'application/vnd.ms-fontobject'
          'application/xhtml+xml'
          'application/xml'
          'application/xml+rss'
          'application/x-font-opentype'
          'application/x-font-truetype'
          'application/x-font-ttf'
          'application/x-httpd-cgi'
          'application/x-javascript'
          'application/x-mpegurl'
          'application/x-opentype'
          'application/x-otf'
          'application/x-perl'
          'application/x-ttf'
          'font/eot'
          'font/ttf'
          'font/otf'
          'font/opentype'
          'image/svg+xml'
          'text/css'
          'text/csv'
          'text/html'
          'text/javascript'
          'text/js'
          'text/plain'
          'text/richtext'
          'text/tab-separated-values'
          'text/xml'
          'text/x-script'
          'text/x-component'
          'text/x-java-source'
        ]
      }
      queryStringCachingBehavior: 'IgnoreQueryString'
    }
    customDomains: []
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
    forwardingProtocol: 'MatchRequest'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
  }
  dependsOn: [
    frontDoor
  ]
}

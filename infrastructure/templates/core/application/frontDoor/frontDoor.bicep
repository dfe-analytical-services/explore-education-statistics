import { abbreviations } from '../../../common/abbreviations.bicep'
import { replaceMultiple } from '../../../common/functions.bicep'  

@description('Environment: Subscription name. Used as a prefix for created resources.')
@allowed([
  's101d01'
  's101t01'
  's101p02'
  's101p01'
  's180d01'
  's180t01'
  's180p01'
])
param subscription string

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Resource prefix for legacy resources.')
param legacyResourcePrefix string

@description('URL of the public site.')
param publicSiteUrl string

@description('Choose whether to use a manually-generated Key Vault certificate or a certificate provisioned by Azure Front Door.')
param certificateType 'Provisioned' | 'BringYourOwn' = 'BringYourOwn'

@description('The Id of the Log Analytics Workspace.')
param logAnalyticsWorkspaceId string

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var frontDoorName = '${resourcePrefix}-${abbreviations.frontDoorProfiles}'

// TODO EES-6883 - remove the "afd.explore-education" line below once we are ready to switch the public site DNS
// over to Azure Front Door properly.  In the meantime, we will host the site through AFD on a temporary
// https://<env name>afd.explore-education-statistics.service.gov.uk with an associated certificate so as
// not to break the use of the environment for others.
var publicSiteHostName = replaceMultiple(publicSiteUrl, {
  'https://': ''
  '.explore-education': 'afd.explore-education'
})

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' = {
  name: frontDoorName
  location: 'global' // CDN lives in a resource group, but must be global
  tags: tagValues
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    originResponseTimeoutSeconds: 220 // Timeout set to just under Azure's standard App Service timeout of 3.8 minutes.
  }
}

resource endpoints 'Microsoft.Cdn/profiles/afdendpoints@2025-04-15' = {
  parent: frontDoor
  name: '${resourcePrefix}-${abbreviations.frontDoorEndpoints}'
  location: 'global'
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
      probePath: '/api/health'
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
    hostName: '${subscription}-as-ees-public-site.azurewebsites.net'
    httpPort: 80
    httpsPort: 443
    originHostHeader: '${subscription}-as-ees-public-site.azurewebsites.net'
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

module publicSiteCertificateModule 'publicSiteCertificate.bicep' = if (certificateType == 'BringYourOwn') {
  name: 'publicSiteCertificateModuleDeploy'
  params: {
    legacyResourcePrefix: legacyResourcePrefix
    frontDoorName: frontDoorName
    publicSiteHostName: publicSiteHostName
  }
}

resource customDomainWithCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'BringYourOwn') {
  parent: frontDoor
  name: '${resourcePrefix}-public-site-${abbreviations.frontDoorDomains}'
  properties: {
    hostName: publicSiteHostName
    tlsSettings: {
      certificateType: 'CustomerCertificate'
      minimumTlsVersion: 'TLS12'
      cipherSuiteSetType: 'TLS12_2023'
      secret: {
        id: publicSiteCertificateModule!.outputs.certificateSecretId
      }
    }
  }
}

resource customDomainWithoutCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'Provisioned') {
  parent: frontDoor
  name: '${resourcePrefix}-public-site-${abbreviations.frontDoorDomains}'
  properties: {
    hostName: publicSiteHostName
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
      cipherSuiteSetType: 'TLS12_2023'
    }
  }
}

resource nextJsRuleSet 'Microsoft.Cdn/profiles/rulesets@2025-04-15' = {
  parent: frontDoor
  name: 'nextjsruleset'
}

/*
This rule prevents Next.JS prefetch request responses from being served up from the cache.
 
The problem is:
  
    - Next.JS sends prefetch requests AND getServerSideProps requests on the exact same URL, using HTTP headers
      to differentiate between the 2 behaviours.  The prefetch generally sends back an empty JSON body "{}",
      whereas the JSON request sends back JSON with server-side props included for the purposes of client-side
      rendering.
      
    - Azure Front Door has no notion of using HTTP headers as part of a cache key.  To Azure Front Door, these
      two requests look exactly the same.
      
    - If a prefetch is called first, it returns no-cache headers and so nothing is cached in AFD.  When a
      subsequent JSON request comes in on the same URL it is unaffected, as nothing has yet been cached for the
      URL.
      
    - If a JSON request is called first however, it sends back caching headers and so is cached.  When a
      subsequent prefetch request comes in on the same URL, the cached response from the original JSON request
      is mistakenly returned.  While not necessarily harmful, it is unexpected.
      
This rule tells Azure Front Door to not serve any response from the cache if it is a Next.JS prefetch.

It also adds a "X-NextJS-Prefetch-Response-Not-Served-From-Cache" HTTP header to the response to prove that is
being handled by this rule.
*/
resource doNotServeCachedContentForNextJsPrefetchesRule 'Microsoft.Cdn/profiles/rulesets/rules@2025-04-15' = {
  parent: nextJsRuleSet
  name: 'donotservecachedcontentfornextjsprefetches'
  properties: {
    order: 100
    conditions: [
      {
        name: 'RequestHeader'
        parameters: {
          typeName: 'DeliveryRuleRequestHeaderConditionParameters'
          operator: 'Equal'
          selector: 'x-middleware-prefetch'
          matchValues: [
            '1'
          ]
        }
      }
    ]
    actions: [
      {
        name: 'ModifyResponseHeader'
        parameters: {
          typeName: 'DeliveryRuleHeaderActionParameters'
          headerAction: 'Append'
          headerName: 'X-NextJS-Prefetch-Response-Not-Served-From-Cache'
          value: 'true'
        }
      }
      {
        name: 'RouteConfigurationOverride'
        parameters: {
          typeName: 'DeliveryRuleRouteConfigurationOverrideActionParameters'
        }
      }
    ]
    matchProcessingBehavior: 'Continue'
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
          // Below list generated by exporting new AFD route from Azure Portal
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

          // Our additions
          'font/woff'
          'font/woff2'
        ]
      }
      queryStringCachingBehavior: 'UseQueryString'
    }
    customDomains: [
      {
        id: certificateType == 'BringYourOwn' ? customDomainWithCertificate.id : customDomainWithoutCertificate.id
      }
    ]
    originGroup: {
      id: originGroup.id
    }
    ruleSets: [
      {
        id: nextJsRuleSet.id
      }
    ]
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
    origin
  ]
}

resource diagnosticSetting 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'afd-diagnostic-setting'
  scope: frontDoor
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

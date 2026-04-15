import { abbreviations } from '../../abbreviations.bicep'
import { staticAverageLessThanHundred, staticAverageGreaterThanZero } from '../../../public-api/components/alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan, dynamicAverageLessThan } from '../../../public-api/components/alerts/dynamicAlertConfig.bicep'
import { FrontDoorCertificateType } from 'types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Resource prefix for legacy resources.')
param legacyResourcePrefix string

@description('Name of the resource.')
param frontDoorProfileName string

@description('Hostname used to reach Azure Front Door and its protected origin.')
param siteHostName string

@description('Hostname of the protected origin behind Azure Front Door.')
param originHostName string

@description('Custom domain name.')
param customDomainName string

@description('Choose whether to use a manually-generated Key Vault certificate or a certificate provisioned by Azure Front Door.')
param certificateType FrontDoorCertificateType = 'BringYourOwn'

@description('Choose whether to use a manually-generated Key Vault certificate or a certificate provisioned by Azure Front Door.')
param certificateName string?

@description('Optional names of rulesets to attach to routes.')
param ruleSetNames string[]

@description('The Id of the Log Analytics Workspace.')
param logAnalyticsWorkspaceId string

@description('Whether to deploy a Web Application Firewall with this Front Door instance.')
param deployWaf bool

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param alerts {
  latency: bool
  originHealth: bool
  percentage4XX: bool
  percentage5XX: bool
  requestCount: bool
  cachedResponseRatio: bool
  wafRequestCounts: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' = {
  name: frontDoorProfileName
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
    hostName: originHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: originHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

module certificateModule 'byoCertificate.bicep' = if (certificateType == 'BringYourOwn') {
  name: 'certificateModuleDeploy'
  params: {
    legacyResourcePrefix: legacyResourcePrefix
    frontDoorName: frontDoorProfileName
    siteHostName: siteHostName
    certificateName: certificateName!
  }
}

resource customDomainWithCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'BringYourOwn') {
  parent: frontDoor
  name: customDomainName
  properties: {
    hostName: siteHostName
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

resource customDomainWithoutCertificate 'Microsoft.Cdn/profiles/customdomains@2025-04-15' = if (certificateType == 'Provisioned') {
  parent: frontDoor
  name: customDomainName
  properties: {
    hostName: siteHostName
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
      cipherSuiteSetType: 'TLS12_2023'
    }
  }
}

var wafPolicyName = '${replace(frontDoorProfileName, '-', '')}${abbreviations.frontDoorWafPolicies}'

module wafPolicyModule 'wafPolicy.bicep' = if (deployWaf) {
  name: '${frontDoorProfileName}WafPolicyModule'
  params: {
    policyName: wafPolicyName
    tagValues: tagValues
  }
}

module wafSecurityPolicyModule 'wafSecurityPolicy.bicep' = if (deployWaf) {
  name: '${frontDoorProfileName}WafSecurityPolicyModule'
  params: {
    securityPolicyName: '${replace(frontDoorProfileName, '-', '')}${abbreviations.frontDoorWafSecurityPolicies}'
    wafPolicyName: wafPolicyName
    customDomainName: customDomainName
    frontDoorProfileName: frontDoorProfileName
  }
  dependsOn: [
    wafPolicyModule
    customDomainWithCertificate
    customDomainWithoutCertificate
  ]
}

resource ruleSets 'Microsoft.Cdn/profiles/rulesets@2025-04-15' = [
  for ruleSetName in ruleSetNames: {
    parent: frontDoor
    name: ruleSetName
  }
]

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
      for (ruleSetName, index) in ruleSetNames: {
        id: ruleSets[index].id
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

module latencyAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.latency) {
  name: '${frontDoorProfileName}LatencyDeploy'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'TotalLatency'
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'response-time'
      threshold: '250'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module originHealthPercentageAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.originHealth) {
  name: '${frontDoorProfileName}OriginHealthPercentage'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'OriginHealthPercentage'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'origin-health-percentage'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module percentage4XXAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.percentage4XX) {
  name: '${frontDoorProfileName}Percentage4XX'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'Percentage4XX'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'percentage-4xx'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module percentage5XXAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.percentage5XX) {
  name: '${frontDoorProfileName}Percentage5XX'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'Percentage5XX'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'percentage-5xx'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module requestCountAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.requestCount) {
  name: '${frontDoorProfileName}RequestCount'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'RequestCount'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'request-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module cachedResponseRatioAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.cachedResponseRatio) {
  name: '${frontDoorProfileName}CachedResponseRatio'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'ByteHitRatio'
    }
    config: {
      ...dynamicAverageLessThan
      nameSuffix: 'cached-response-ratio'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module wafJsRequestCountAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.wafRequestCounts) {
  name: '${frontDoorProfileName}WafJsRequestCount'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'WebApplicationFirewallJsRequestCount'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'waf-js-request-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module wafCaptchaAlert '../../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.wafRequestCounts) {
  name: '${frontDoorProfileName}WafCaptchaRequestCount'
  params: {
    resourceName: frontDoorProfileName
    resourceMetric: {
      resourceType: 'Microsoft.Cdn/profiles'
      metric: 'WebApplicationFirewallCaptchaRequestCount'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'waf-captcha-request-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

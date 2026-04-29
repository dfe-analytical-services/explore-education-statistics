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

@description('The minimum average response time from the public site (via Azure Front Door) before latency alerts fire.')
param averagePublicSiteResponseTimeAlertThresholdMillis int

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param deployAlerts bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var frontDoorProfileName = '${resourcePrefix}-${abbreviations.frontDoorProfiles}'

// TODO EES-6883 - remove the "afd.explore-education" lines below once we are ready to switch the public site DNS
// over to Azure Front Door properly.  In the meantime, we will host the site through AFD on a temporary
// https://<env name>afd.explore-education-statistics.service.gov.uk with an associated certificate so as
// not to break the use of the environment for others.
//
// Note that this only applies to Prod and Pre-Prod now, as Dev and Test can now use their proper public site
// URLs to reach AFD.
var publicSiteHostName = subscription == 's101p01'
  
  // Handle the case for Prod to allow access via "https://afd.explore-education-statistics.service.gov.uk" during
  // testing.
  ? 'afd.explore-education-statistics.service.gov.uk'
  
  : replaceMultiple(publicSiteUrl, {
  
  // Handle the case for Pre-Production to allow access via
  // "https://pre-productionafd.explore-education-statistics.service.gov.uk" during testing.
  'pre-production.explore-education': 'pre-productionafd.explore-education'
  
  // Remove the "https://" from the URL to leave just the domain name.
  'https://': ''
})

// TODO EES-6883 - remove the "afd" from the line below once we're ready to
// switch DNS over to point at Azure Front Door rather than the Public Site
// App Service.  During testing we will host the public site through AFD on
// a temporary https://<env name>afd.explore-education-statistics.service.gov.uk
// URL with an associated certificate so as not to break the use of the environment
// for others.
//
// Note that Dev and Test now have the original public site URLs pointing towards
// their AFD instances now rather than the original App Service, so they can serve
// up the real certificate rather than the temporary testing one.  
var certificateName = subscription == 's101d01' || subscription == 's101t01'
    ? '${legacyResourcePrefix}as-ees-public-site-certificate'
    : '${legacyResourcePrefix}as-ees-public-site-afd-certificate'

var nextJsRuleSetName = 'nextjsruleset'

module frontDoorModule '../../../common/components/front-door/frontDoor.bicep' = {
  name: '${frontDoorProfileName}ModuleDeploy'
  params: {
    frontDoorProfileName: frontDoorProfileName
    resourcePrefix: resourcePrefix
    legacyResourcePrefix: legacyResourcePrefix
    siteHostName: publicSiteHostName
    originHostName: '${subscription}-as-ees-public-site.azurewebsites.net'
    customDomainName: '${resourcePrefix}-public-site-${abbreviations.frontDoorDomains}'
    certificateType: certificateType
    certificateName: certificateType == 'BringYourOwn' ? certificateName : null
    ruleSetNames: [nextJsRuleSetName]
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    deployWaf: true
    alerts: deployAlerts ? {
      latency: true
      originHealth: true
      requestCount: true
      percentage4XX: true
      percentage5XX: true
      cachedResponseRatio: true
      wafRequestCounts: true
      averageResponseTimeAlertThresholdMillis: averagePublicSiteResponseTimeAlertThresholdMillis
      alertsGroupName: '${subscription}-ag-ees-alertedusers'
    } : null
    tagValues: tagValues
  }
}

resource nextJsRuleSet 'Microsoft.Cdn/profiles/rulesets@2025-04-15' existing = {
  name: '${frontDoorProfileName}/${nextJsRuleSetName}'
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
}

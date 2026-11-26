import { FrontDoorCertificateType } from '../../../common/components/front-door/types.bicep'
import { ResourceNames } from '../../../common/resource-names.bicep'

@description('Names of the Front Door profile and its child resources.')
param frontDoorNames ResourceNames.frontDoor

@description('Name of the Key Vault certificate used by the public site custom domain.')
param publicSiteCertificateName string

@description('Name of the Key Vault certificate used by the Content API custom domain.')
param contentApiCertificateName string

@description('Name of the public site App Service, used as the public site origin.')
param publicSiteAppServiceName string

@description('Name of the Content API App Service, used as the Content API origin.')
param contentApiAppServiceName string

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('URL of the public site.')
param publicSiteUrl string

@description('URL of the Content API.')
param contentApiUrl string

@description('Choose whether to use a manually-generated Key Vault certificate or a certificate provisioned by Azure Front Door.')
param certificateType FrontDoorCertificateType = 'BringYourOwn'

@description('Name of the Key Vault instance in which to store certificates.')
param keyVaultName string

@description('Name of the existing Publisher Function App granted permission to purge cached ZIPs.')
param publisherFunctionName string

@description('Whether to grant the Publisher Function App\'s system-assigned identity its roles here.')
param deploySystemAssignedIdentityRoleAssignments bool

@description('The Id of the Log Analytics Workspace.')
param logAnalyticsWorkspaceId string

@description('The minimum average response time from the public site (via Azure Front Door) before latency alerts fire.')
param averagePublicSiteResponseTimeAlertThresholdMillis int

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param deployAlerts bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var frontDoorProfileName = frontDoorNames.frontDoorName

var publicSiteHostName = replace(publicSiteUrl, 'https://', '')
var contentApiHostName = replace(contentApiUrl, 'https://', '')

var publicSiteCustomDomainName = frontDoorNames.publicSite.customDomain
var contentApiCustomDomainName = frontDoorNames.contentApi.customDomain

var nextJsRuleSetName = 'nextjsruleset'

module frontDoorModule '../../../common/components/front-door/frontDoor.bicep' = {
  name: '${frontDoorProfileName}ModuleDeploy'
  params: {
    frontDoorProfileName: frontDoorProfileName
    endpointName: frontDoorNames.defaultEndpoint.endpointName
    originGroupName: frontDoorNames.publicSite.originGroup
    originName: frontDoorNames.publicSite.origin
    routeName: frontDoorNames.publicSite.route
    wafPolicyName: frontDoorNames.wafPolicy
    wafSecurityPolicyName: frontDoorNames.wafSecurityPolicy
    keyVaultName: keyVaultName
    siteHostName: publicSiteHostName
    originHostName: '${publicSiteAppServiceName}.azurewebsites.net'
    customDomainName: publicSiteCustomDomainName
    certificateType: certificateType
    certificateName: certificateType == 'BringYourOwn' ? publicSiteCertificateName : null
    ruleSetNames: [nextJsRuleSetName]
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    deployWaf: true
    deployWafDomainAssociation: false
    alerts: deployAlerts ? {
      latency: true
      originHealth: true
      requestCount: true
      percentage4XX: true
      percentage5XX: true
      cachedResponseRatio: true
      wafRequestCounts: true
      averageResponseTimeAlertThresholdMillis: averagePublicSiteResponseTimeAlertThresholdMillis
      alertsGroupName: alertsGroupName
    } : null
    tagValues: tagValues
  }
}

module contentApiFrontDoorModule 'content-api-front-door.bicep' = {
  name: '${frontDoorProfileName}ContentApiModuleDeploy'
  params: {
    contentApiNames: frontDoorNames.contentApi
    frontDoorProfileName: frontDoorProfileName
    frontDoorEndpointName: frontDoorNames.defaultEndpoint.endpointName
    keyVaultName: keyVaultName
    certificateName: contentApiCertificateName
    contentApiHostName: contentApiHostName
    contentApiOriginHostName: '${contentApiAppServiceName}.azurewebsites.net'
    publisherFunctionName: publisherFunctionName
    deploySystemAssignedIdentityRoleAssignments: deploySystemAssignedIdentityRoleAssignments
  }
  dependsOn: [
    frontDoorModule
  ]
}

module wafSecurityPolicyModule '../../../common/components/front-door/wafSecurityPolicy.bicep' = {
  name: '${frontDoorProfileName}WafSecurityPolicyModule'
  params: {
    securityPolicyName: frontDoorNames.wafSecurityPolicy
    wafPolicyName: frontDoorNames.wafPolicy
    customDomainNames: [
      publicSiteCustomDomainName
      contentApiCustomDomainName
    ]
    frontDoorProfileName: frontDoorProfileName
  }
  dependsOn: [
    frontDoorModule
    contentApiFrontDoorModule
  ]
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

import { ResourceNames } from '../../types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Common resource naming variables')
param resourceNames ResourceNames

@description('The location to create resources in')
param location string

@description('The FQDN for accessing the public API via Application Gateway. Used by FaUAPI to proxy traffic to the public API.')
param publicApiAppGatewayFqdn string

@description('The base public URL for accessing the public API and its documentation site. This is the base FaUAPI URL for the API.')
param publicApiPublicUrl string

@description('FQDN of the public site public URL that users use to access the site.')
param publicSiteFqdn string

@description('FQDN of the service hosting the public site, rather than the public URL as used by custom domains.')
param publicSiteInternalServiceFqdn string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

// The resource prefix for anything specific to the Public API.
var publicApiResourcePrefix = '${subscription}-ees-papi'

var docsRewriteSetName = '${publicApiResourcePrefix}-docs-rewrites'

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.appGateway
  parent: vNet
}

resource apiApp 'Microsoft.App/containerApps@2024-03-01' existing = {
  name: resourceNames.publicApi.apiApp
}

resource apiDocsSite 'Microsoft.Web/staticSites@2023-12-01' existing = {
  name: resourceNames.publicApi.docsApp
}

module globalWafPolicyModule '../../../common/components/application-gateway/appGatewayWafPolicy.bicep' = {
  name: 'globalWafPolicyModuleDeploy'
  params: {
    name: '${resourceNames.sharedResources.appGateway}-global-afwp'
    location: location
    tagValues: tagValues
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.existingResources.keyVault
}

module publicApiWafPolicyModule '../public-api/publicApiWafPolicy.bicep' = {
  name: 'publicApiWafPolicyModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    fuapiSecretValue: keyVault.getSecret('ees-publicapi-app-gateway-fuapi-header')
    tagValues: tagValues
  }
}

module appGatewayModule '../../../common/components/application-gateway/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: resourceNames.sharedResources.appGateway
    managedIdentityName: resourceNames.sharedResources.appGatewayIdentity
    keyVaultName: resourceNames.existingResources.keyVault
    subnetId: subnet.id
    frontendConfig: {
      name: '${publicApiResourcePrefix}-public-frontend-ip-config'
      publicIpAddressName: '${publicApiResourcePrefix}-pip'
    }
    sites: [
      {
        name: publicApiResourcePrefix
        certificateName: '${publicApiResourcePrefix}-certificate'
        fqdn: replace(publicApiAppGatewayFqdn, 'https://', '')
        wafPolicyName: publicApiWafPolicyModule.outputs.name
      }
      {
        name: '${resourceNames.existingResources.publicSiteAppService}-site'
        certificateName: '${resourceNames.existingResources.publicSiteAppService}-certificate'
        fqdn: publicSiteFqdn
        wafPolicyName: '${resourceNames.sharedResources.appGateway}-global-afwp'
      }
    ]
    backends: [
      {
        name: resourceNames.publicApi.apiApp
        fqdn: apiApp.properties.configuration.ingress.fqdn
        healthProbePath: '/health'
      }
      {
        name: resourceNames.publicApi.docsApp
        fqdn: apiDocsSite.properties.defaultHostname
        healthProbePath: '/'
      }
      {
        name: '${resourceNames.existingResources.publicSiteAppService}-backend'
        fqdn: publicSiteInternalServiceFqdn
        healthProbePath: '/api/health'
        healthProbeAdditionalStatusCodeMatches: ['405']
      }
    ]
    routes: [
      {
        name: publicApiResourcePrefix
        siteName: publicApiResourcePrefix
        defaultBackendName: resourceNames.publicApi.apiApp
        pathRules: [
          {
            name: 'docs-backend'
            paths: ['/docs/*']
            type: 'backend'
            backendName: resourceNames.publicApi.docsApp
            rewriteSetName: docsRewriteSetName
          }
          {
            // Redirect non-rooted URL (has no trailing slash) to the
            // rooted URL so that relative links in the docs site
            // can resolve correctly.
            name: 'docs-root-redirect'
            paths: ['/docs']
            type: 'redirect'
            redirectUrl: '${publicApiPublicUrl}/docs/'
            redirectType: 'Permanent'
            includePath: false
          }
        ]
      }
      {
        name: '${resourceNames.existingResources.publicSiteAppService}-route'
        siteName: '${resourceNames.existingResources.publicSiteAppService}-site'
        defaultBackendName: '${resourceNames.existingResources.publicSiteAppService}-backend'
      }
    ]
    rewrites: [
      {
        name: docsRewriteSetName
        rules: [
          {
            name: 'trim-docs-path-prefix'
            conditions: [
              {
                variable: 'var_uri_path'
                pattern: '^/docs/(.*)'
                ignoreCase: true
              }
            ]
            actionSet: {
              urlConfiguration: {
                modifiedPath: '/{var_uri_path_1}'
              }
            }
          }
          {
            name: 'replace-docs-backend-fqdn-with-public-docs-url'
            conditions: [
              {
                variable: 'http_resp_Location'
                pattern: 'https://${apiDocsSite.properties.defaultHostname}/(.*)'
                ignoreCase: true
              }
            ]
            actionSet: {
              responseHeaderConfigurations: [
                {
                  headerName: 'Location'
                  headerValue: '${publicApiPublicUrl}/docs/{http_resp_Location_1}'
                }
              ]
            }
          }
        ]
      }
    ]
    globalWafPolicyId: globalWafPolicyModule.outputs.id
    alerts: deployAlerts ? {
      health: true
      responseTime: true
      failedRequests: true
      responseStatuses: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

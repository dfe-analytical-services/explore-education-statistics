import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('The location to create resources in')
param location string

@description('')
param publicApiAppName string

@description('')
param publicApiDocsAppName string

@description('')
param publicSiteAppServiceName string

@description('')
param keyVaultName string

@description('')
param vnetName string

@description('FQDN of the public API listener in Application Gateway. This is the FQDN that FaUAPI proxies traffic to to access the public API from Application Gateway.')
param publicApiAppGatewayFqdn string

@description('The base public URL for accessing the public API and its documentation site. This is the base FaUAPI URL that users use to interact with the API.')
param publicApiPublicUrl string

@description('FQDN of the public site public URL that users use to access the site.')
param publicSiteFqdn string

@description('FQDN of the service hosting the public site, rather than the public URL as used by custom domains.')
param publicSiteInternalServiceFqdn string

@description('')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

var commonResourcePrefix = '${subscription}-ees'

var publicApiResourcePrefix = '${subscription}-ees-papi'

var appGatewayName = '${commonResourcePrefix}-${abbreviations.networkApplicationGateways}-01'
    
var appGatewayIdentityName = '${commonResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.networkApplicationGateways}-01'

var appGatewaySubnetName = '${commonResourcePrefix}-snet-${abbreviations.networkApplicationGateways}-01'

var docsRewriteSetName = '${publicApiResourcePrefix}-docs-rewrites'

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: appGatewaySubnetName
  parent: vNet
}

resource apiApp 'Microsoft.App/containerApps@2024-03-01' existing = {
  name: publicApiAppName
}

resource apiDocsSite 'Microsoft.Web/staticSites@2023-12-01' existing = {
  name: publicApiDocsAppName
}

module globalWafPolicyModule '../../../common/components/application-gateway/appGatewayWafPolicy.bicep' = {
  name: 'globalWafPolicyModuleDeploy'
  params: {
    name: '${appGatewayName}-global-afwp'
    location: location
    tagValues: tagValues
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: keyVaultName
}

module publicApiWafPolicyModule 'publicApiWafPolicy.bicep' = {
  name: 'publicApiWafPolicyModuleDeploy'
  params: {
    location: location
    appGatewayName: appGatewayName
    fuapiSecretValue: keyVault.getSecret('ees-publicapi-app-gateway-fuapi-header')
    tagValues: tagValues
  }
}

module appGatewayModule '../../../common/components/application-gateway/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: appGatewayName
    managedIdentityName: appGatewayIdentityName
    keyVaultName: keyVaultName
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
        name: '${publicSiteAppServiceName}-site'
        certificateName: '${publicSiteAppServiceName}-certificate'
        fqdn: publicSiteFqdn
        wafPolicyName: '${appGatewayName}-global-afwp'
      }
    ]
    backends: [
      {
        name: publicApiAppName
        fqdn: apiApp.properties.configuration.ingress.fqdn
        healthProbePath: '/health'
      }
      {
        name: publicApiDocsAppName
        fqdn: apiDocsSite.properties.defaultHostname
        healthProbePath: '/'
      }
      {
        name: '${publicSiteAppServiceName}-backend'
        fqdn: publicSiteInternalServiceFqdn
        healthProbePath: '/api/health'
        intervalSeconds: 60
        requestTimeout: 220 // Timeout set to just under Azure's standard App Service timeout of 3.8 minutes.
      }
    ]
    routes: [
      {
        name: publicApiResourcePrefix
        siteName: publicApiResourcePrefix
        defaultBackendName: publicApiAppName
        pathRules: [
          {
            name: 'docs-backend'
            paths: ['/docs/*']
            type: 'backend'
            backendName: publicApiDocsAppName
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
        name: '${publicSiteAppServiceName}-route'
        siteName: '${publicSiteAppServiceName}-site'
        defaultBackendName: '${publicSiteAppServiceName}-backend'
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
      alertsGroupName: alertsGroupName
    } : null
    tagValues: tagValues
  }
}

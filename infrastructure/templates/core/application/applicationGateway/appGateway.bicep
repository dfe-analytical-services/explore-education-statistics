import { ResourceNames } from '../../../common/resource-names.bicep'

@description('Names of the Application Gateway and its associated resources.')
param names ResourceNames.applicationGateway

@description('Name of the subnet that the Application Gateway is deployed into.')
param subnetName string

@description('Name of the Key Vault certificate used by the public API listener.')
param publicApiCertificateName string

@description('Name of the Key Vault certificate used by the public site listener.')
param publicSiteCertificateName string

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

@description('Name of the Action Group that receives alerts.')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Tags for the resources')
param tagValues object

var appGatewayName = names.applicationGateway

var docsRewriteSetName = names.publicApiDocsRewriteSet

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnetName
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
    name: names.globalWafPolicy
    location: location
    mode: 'Detection'
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
    policyName: names.publicApiWafPolicy
    fuapiSecretValue: keyVault.getSecret('ees-publicapi-app-gateway-fuapi-header')
    tagValues: tagValues
  }
}

module appGatewayModule '../../../common/components/application-gateway/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: appGatewayName
    managedIdentityName: names.managedIdentity
    keyVaultName: keyVaultName
    subnetId: subnet.id
    frontendConfig: {
      name: names.frontendIpConfig
      publicIpAddressName: names.publicIpAddress
    }
    sites: [
      {
        name: names.publicApiSite
        certificateName: publicApiCertificateName
        fqdn: replace(publicApiAppGatewayFqdn, 'https://', '')
        wafPolicyName: publicApiWafPolicyModule.outputs.name
      }
      {
        name: '${publicSiteAppServiceName}-site'
        certificateName: publicSiteCertificateName
        fqdn: publicSiteFqdn
        wafPolicyName: names.globalWafPolicy
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
        healthProbePath: '/health'
        intervalSeconds: 60
        requestTimeout: 220 // Timeout set to just under Azure's standard App Service timeout of 3.8 minutes.
      }
    ]
    routes: [
      {
        name: names.publicApiSite
        siteName: names.publicApiSite
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

import { ResourceNames } from '../../../common/resource-names.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Name of the subnet that the Application Gateway is deployed into.')
param subnetName string

@description('The location to create resources in')
param location string

@description('Name of the Key Vault holding the Application Gateway\'s certificates and the FUAPI header secret.')
param keyVaultName string

@description('Name of the VNet that the Application Gateway\'s subnet belongs to.')
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

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: subnetName
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
    name: resourceNames.applicationGateway.globalWafPolicy
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
    policyName: resourceNames.applicationGateway.sites.publicApi.wafPolicy
    fuapiSecretValue: keyVault.getSecret('ees-publicapi-app-gateway-fuapi-header')
    tagValues: tagValues
  }
}

module appGatewayModule '../../../common/components/application-gateway/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: resourceNames.applicationGateway.applicationGateway
    managedIdentityName: resourceNames.applicationGateway.managedIdentity
    keyVaultName: keyVaultName
    subnetId: subnet.id
    frontendConfig: {
      name: resourceNames.applicationGateway.frontendIpConfig
      publicIpAddressName: resourceNames.applicationGateway.publicIpAddress
    }
    sites: [
      {
        name: resourceNames.applicationGateway.sites.publicApi.site
        certificateName: resourceNames.keyVault.certificates.publicApiAppGateway
        fqdn: replace(publicApiAppGatewayFqdn, 'https://', '')
        wafPolicyName: publicApiWafPolicyModule.outputs.name
      }
      {
        name: resourceNames.applicationGateway.sites.publicSite.site
        certificateName: resourceNames.keyVault.certificates.publicSite
        fqdn: publicSiteFqdn
        wafPolicyName: resourceNames.applicationGateway.globalWafPolicy
      }
    ]
    backends: [
      {
        name: resourceNames.applicationGateway.sites.publicApi.backend
        fqdn: apiApp.properties.configuration.ingress.fqdn
        healthProbePath: '/health'
      }
      {
        name: resourceNames.applicationGateway.sites.publicApiDocs.backend
        fqdn: apiDocsSite.properties.defaultHostname
        healthProbePath: '/'
      }
      {
        name: resourceNames.applicationGateway.sites.publicSite.backend
        fqdn: publicSiteInternalServiceFqdn
        healthProbePath: '/health'
        intervalSeconds: 60
        requestTimeout: 220 // Timeout set to just under Azure's standard App Service timeout of 3.8 minutes.
      }
    ]
    routes: [
      {
        name: resourceNames.applicationGateway.sites.publicApi.route
        siteName: resourceNames.applicationGateway.sites.publicApi.site
        defaultBackendName: resourceNames.applicationGateway.sites.publicApi.backend
        pathRules: [
          {
            name: resourceNames.applicationGateway.sites.publicApiDocs.pathRules.backend
            paths: ['/docs/*']
            type: 'backend'
            backendName: resourceNames.applicationGateway.sites.publicApiDocs.backend
            rewriteSetName: resourceNames.applicationGateway.sites.publicApi.rewriteSet
          }
          {
            // Redirect non-rooted URL (has no trailing slash) to the
            // rooted URL so that relative links in the docs site
            // can resolve correctly.
            name: resourceNames.applicationGateway.sites.publicApiDocs.pathRules.rootRedirect
            paths: ['/docs']
            type: 'redirect'
            redirectUrl: '${publicApiPublicUrl}/docs/'
            redirectType: 'Permanent'
            includePath: false
          }
        ]
      }
      {
        name: resourceNames.applicationGateway.sites.publicSite.route
        siteName: resourceNames.applicationGateway.sites.publicSite.site
        defaultBackendName: resourceNames.applicationGateway.sites.publicSite.backend
      }
    ]
    rewrites: [
      {
        name: resourceNames.applicationGateway.sites.publicApi.rewriteSet
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

import { responseTimeConfig, dynamicTotalGreaterThan } from 'alerts/dynamicAlertConfig.bicep'
import { staticAverageGreaterThanZero } from 'alerts/staticAlertConfig.bicep'

import {
  AppGatewayBackend
  AppGatewayRewriteSet
  AppGatewayRoute
  AppGatewaySite
} from '../types.bicep'

@description('Name of the Key Vault name that the App Gateway will be permitted to get and list certificates from')
param keyVaultName string

@description('Location to create resources in')
param location string

@description('ID of a dedicated subnet to which this App Gateway will be connected')
param subnetId string

@description('Name of the App Gateway')
param appGatewayName string = ''

@description('Name of the user-assigned managed identity for the App Gateway')
param managedIdentityName string = ''

@description('Sites that the App Gateway can accept traffic from')
param sites AppGatewaySite[]

@description('Backends the App Gateway can direct traffic to')
param backends AppGatewayBackend[]

@description('Routes the App Gateway should direct traffic through')
param routes AppGatewayRoute[]

@description('Rules for how the App Gateway should rewrite URLs')
param rewrites AppGatewayRewriteSet[]

@description('Availability zones in the region that the resource should be accessible from. Defaults to all zones')
param availabilityZones ('1' | '2' | '3') [] = [
  '1'
  '2'
  '3'
]

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  health: bool
  responseTime: bool
  failedRequests: bool
  responseStatuses: bool
  alertsGroupName: string
}?

@description('Tags for the resources')
param tagValues object

// Create a user-assigned managed identity for the App Gateway. App Gateway does not support system-assigned identities.
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// Allow the managed identity to access certificates from Key Vault.
module keyVaultSecretsUserRoleAssignmentModule 'keyVaultRoleAssignment.bicep' = {
  name: '${appGatewayName}KeyVaultSecretsUserRoleAssignment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [managedIdentity.properties.principalId]
    role: 'Secrets User'
  }
}

module keyVaultCertificateUserRoleAssignmentModule 'keyVaultRoleAssignment.bicep' = {
  name: '${appGatewayName}KeyVaultCertificateUserRoleAssignment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [managedIdentity.properties.principalId]
    role: 'Certificate User'
  }
}

module keyVaultAccessPolicyModule 'keyVaultAccessPolicy.bicep' = {
  name: '${appGatewayName}KeyVaultAccessPolicy'
  params: {
    keyVaultName: keyVaultName
    principalIds: [managedIdentity.properties.principalId]
  }
}

// Create public IP addresses for every site we will expose through this App Gateway.
resource publicIPAddresses 'Microsoft.Network/publicIPAddresses@2024-01-01' = [for site in sites: {
  name: '${site.name}-pip'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    dnsSettings: {
      domainNameLabel: replace(site.fqdn, '.', '')
      fqdn: '${replace(site.fqdn, '.', '')}.${location}.cloudapp.azure.com'
    }
  }
  zones: availabilityZones
}]

// Add a Firewall Policy with OWASP and Bot rulesets, running in Prevention mode.
module wafPolicyModule 'appGatewayWafPolicy.bicep' = {
  name: 'wafPolicy'
  params: {
    name: '${appGatewayName}-afwp'
    location: location
    tagValues: tagValues
  }
}

module pathRulesModule './appGatewayPathRules.bicep' = [for route in routes: {
  name: '${route.name}-route-paths'
  params: {
    appGatewayName: appGatewayName
    pathRules: route.pathRules
  }
}]

var allPathRules = flatten(map(routes, route => route.pathRules))

var allRedirectRules = filter(allPathRules, rule => rule.type == 'redirect')

// Create the App Gateway.
resource appGateway 'Microsoft.Network/applicationGateways@2024-01-01' = {
  name: appGatewayName
  location: location
  tags: tagValues
  zones: availabilityZones
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
      family: 'Generation_1'
    }
    gatewayIPConfigurations: [
      {
        name: '${appGatewayName}-ip-config'
        properties: {
          subnet: {
            id: subnetId
          }
        }
      }
    ]
    sslCertificates: [for site in sites: {
      name: '${site.name}-cert'
      properties: {
        keyVaultSecretId: 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/secrets/${site.certificateName}'
      }
    }]
    frontendIPConfigurations: [for site in sites: {
      name: '${site.name}-public-frontend-ip-config'
      properties: {
        privateIPAllocationMethod: 'Dynamic'
        publicIPAddress: {
          id: resourceId('Microsoft.Network/publicIPAddresses', '${site.name}-pip')
        }
      }
    }]
    frontendPorts: [
      {
        name: 'port443'
        properties: {
          port: 443
        }
      }
    ]
    backendAddressPools: [for backend in backends: {
      name: '${backend.name}-backend-pool'
      properties: {
        backendAddresses: [
          {
            fqdn: backend.fqdn
          }
        ]
      }
    }]
    backendHttpSettingsCollection: [for backend in backends: {
      name: '${backend.name}-backend'
      properties: {
        port: 443
        protocol: 'Https'
        cookieBasedAffinity: 'Disabled'
        pickHostNameFromBackendAddress: true
        requestTimeout: 20
        probe: {
          id: resourceId('Microsoft.Network/applicationGateways/probes', appGatewayName, '${backend.name}-health-probe')
        }
      }
    }]
    httpListeners: [for site in sites: {
      name: '${site.name}-https-443-listener'
      properties: {
        frontendIPConfiguration: {
          id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', appGatewayName, '${site.name}-public-frontend-ip-config')
        }
        frontendPort: {
          id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', appGatewayName, 'port443')
        }
        protocol: 'Https'
        sslCertificate: {
          id: resourceId('Microsoft.Network/applicationGateways/sslCertificates', appGatewayName, '${site.name}-cert')
        }
        hostName: site.fqdn
        requireServerNameIndication: true
      }
    }]
    requestRoutingRules: [for (route, index) in routes: {
      name: '${route.name}-rule'
      properties: {
        ruleType: 'PathBasedRouting'
        priority: index + 1
        httpListener: {
          id: resourceId('Microsoft.Network/applicationGateways/httpListeners', appGatewayName, '${route.siteName}-https-443-listener')
        }
        urlPathMap: {
          id: resourceId('Microsoft.Network/applicationGateways/urlPathMaps', appGatewayName, '${route.name}-url-path-map')
        }
      }
    }]
    urlPathMaps: [for (route, index) in routes: {
      name: '${route.name}-url-path-map'
      properties: {
        defaultBackendAddressPool: {
          id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', appGatewayName, '${route.defaultBackendName}-backend-pool')
        }
        defaultBackendHttpSettings: {
          id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', appGatewayName, '${route.defaultBackendName}-backend')
        }
        pathRules: pathRulesModule[index].outputs.rules
      }
    }]
    redirectConfigurations: [for rule in allRedirectRules: {
      name: rule.name
      properties: {
        targetUrl: rule.redirectUrl
        redirectType: rule.redirectType
        includePath: rule.includePath != null ? rule.includePath : true
      }
    }]
    probes: [for backend in backends: {
      name: '${backend.name}-health-probe'
      properties: {
        protocol: 'Https'
        path: backend.healthProbePath
        interval: 30
        timeout: 30
        unhealthyThreshold: 3
        pickHostNameFromBackendHttpSettings: true
        minServers: 0
        match: {}
      }
    }]
    rewriteRuleSets: [for rewriteSet in rewrites: {
      name: rewriteSet.name
      properties: {
        rewriteRules: rewriteSet.rules
      }
    }]
    enableHttp2: true
    autoscaleConfiguration: {
      minCapacity: 0
      maxCapacity: 10
    }
    firewallPolicy: {
      id: wafPolicyModule.outputs.id
    }
  }
  dependsOn: [
    publicIPAddresses
    keyVaultSecretsUserRoleAssignmentModule
    keyVaultCertificateUserRoleAssignmentModule
    keyVaultAccessPolicyModule
  ]
}

module backendPoolsHealthAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.health) {
  name: '${appGatewayName}BackendHealthAlertModule'
  params: {
    resourceName: appGatewayName
    resourceMetric: {
      resourceType: 'Microsoft.Network/applicationGateways'
      metric: 'UnhealthyHostCount'
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'backend-pool-health'
      threshold: '0.05'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module responseTimeAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.responseTime) {
  name: '${appGatewayName}ResponseTimeDeploy'
  params: {
    resourceName: appGatewayName
    resourceMetric: {
      resourceType: 'Microsoft.Network/applicationGateways'
      metric: 'ApplicationGatewayTotalTime'
    }
    config: responseTimeConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module failedRequestsAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.failedRequests) {
  name: '${appGatewayName}FailedRequestsDeploy'
  params: {
    resourceName: appGatewayName
    resourceMetric: {
      resourceType: 'Microsoft.Network/applicationGateways'
      metric: 'FailedRequests'
      dimensions: [{
        name: 'BackendSettingsPool'
        values: map(backends, backend => backend.name)
      }]
    }
    config: {
      ...dynamicTotalGreaterThan
      nameSuffix: 'failed-requests'
      windowSize: 'PT30M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module responseStatusAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.responseStatuses) {
  name: '${appGatewayName}ResponseStatusDeploy'
  params: {
    resourceName: appGatewayName
    resourceMetric: {
      resourceType: 'Microsoft.Network/applicationGateways'
      metric: 'ResponseStatus'
      dimensions: [{
        name: 'HttpStatusGroup'
        values: ['4xx', '5xx']
      }]
    }
    config: {
      ...dynamicTotalGreaterThan
      nameSuffix: 'http-4xx-5xx'
      windowSize: 'PT30M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

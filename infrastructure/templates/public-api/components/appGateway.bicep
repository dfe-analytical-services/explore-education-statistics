@description('Specifies the Key Vault name that this App Gateway will be permitted to get and list certificates from')
param keyVaultName string

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources')
param location string

@description('Specifies the id of a dedicated subnet to which this App Gateway will be connected')
param subnetId string

@description('Specifies an optional suffix for the App Gateway name')
param instanceName string = ''

@description('Specifies a set of configurations for the App Gateway to use to direct traffic to backend resources')
param sites {
  resourceName: string
  publicHostName: string
  backendFqdn: string
  healthProbeRelativeUrl: string
  certificateKeyVaultSecretName: string
}[]

@description('Specifies a set of Azure availability zones for the region that this resource should be accessible from. Defaults to all zones')
@allowed([
  '1'
  '2'
  '3'
])
param availabilityZones string[] = [
  '1'
  '2'
  '3'
]

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

var appGatewayFullName = empty(instanceName)
  ? '${resourcePrefix}-ees-agw'
  : '${resourcePrefix}-ees-agw-${instanceName}'

var managedIdentityName = empty(instanceName)
  ? '${resourcePrefix}-ees-id-agw'
  : '${resourcePrefix}-ees-id-agw-${instanceName}'

// Create a user-assigned managed identity for the App Gateway. App Gateway does not support system-assigned identities.
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// Allow the managed identity to access certificates from Key Vault.
module managedIdentityKeyVaultAccessPolicy 'keyVaultAccessPolicy.bicep' = {
  name: '${appGatewayFullName}KeyVaultAccessPolicy'
  params: {
    keyVaultName: keyVaultName
    principalIds: [managedIdentity.properties.principalId]
  }
}

module appGatewayKeyVaultRoleAssignments 'keyVaultRoleAssignment.bicep' = {
  name: 'appGatewayKeyVaultRoleAssignment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [managedIdentity.properties.principalId]
    role: 'Secrets User'
  }
}

// Create public IP addresses for every site we will expose through this App Gateway.
resource publicIPAddresses 'Microsoft.Network/publicIPAddresses@2021-05-01' = [for site in sites: {
  name: '${site.resourceName}-pip'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    dnsSettings: {
      domainNameLabel: replace(site.publicHostName, '.', '')
      fqdn: '${replace(site.publicHostName, '.', '')}.${location}.cloudapp.azure.com'
    }
  }
}]

// Add a Firewall Policy with OWASP and Bot rulesets, running in Prevention mode.
module wafPolicy 'wafPolicy.bicep' = {
  name: 'wafPolicy'
  params: {
    name: '${appGatewayFullName}-waf-policy'
    location: location
    tagValues: tagValues
  }
}

// Create the App Gateway.
resource appGateway 'Microsoft.Network/applicationGateways@2023-11-01' = {
  name: appGatewayFullName
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
        name: '${appGatewayFullName}-ip-config'
        properties: {
          subnet: {
            id: subnetId
          }
        }
      }
    ]
    sslCertificates: [for site in sites: {
      name: '${site.resourceName}-cert'
      properties: {
        keyVaultSecretId: 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/secrets/${site.certificateKeyVaultSecretName}'
      }
    }]
    frontendIPConfigurations: [for site in sites: {
      name: '${site.resourceName}-public-frontend-ip-config'
      properties: {
        privateIPAllocationMethod: 'Dynamic'
        publicIPAddress: {
          id: resourceId('Microsoft.Network/publicIPAddresses', '${site.resourceName}-pip')
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
    backendAddressPools: [for site in sites: {
      name: '${site.resourceName}-backend-pool'
      properties: {
        backendAddresses: [
          {
            fqdn: site.backendFqdn
          }
        ]
      }
    }]
    backendHttpSettingsCollection: [for site in sites: {
      name: '${site.resourceName}-backend'
      properties: {
        port: 443
        protocol: 'Https'
        cookieBasedAffinity: 'Disabled'
        pickHostNameFromBackendAddress: true
        requestTimeout: 20
        probe: {
          id: resourceId('Microsoft.Network/applicationGateways/probes', appGatewayFullName, '${site.resourceName}-health-probe')
        }
      }
    }]
    httpListeners: [for site in sites: {
      name: '${site.resourceName}-listener'
      properties: {
        frontendIPConfiguration: {
          id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', appGatewayFullName, '${site.resourceName}-public-frontend-ip-config')
        }
        frontendPort: {
          id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', appGatewayFullName, 'port443')
        }
        protocol: 'Https'
        sslCertificate: {
          id: resourceId('Microsoft.Network/applicationGateways/sslCertificates', appGatewayFullName, '${site.resourceName}-cert')
        }
        hostName: site.publicHostName
        requireServerNameIndication: true
      }
    }]
    requestRoutingRules: [for site in sites: {
      name: '${site.resourceName}-routing-rule'
      properties: {
        ruleType: 'Basic'
        priority: 1
        httpListener: {
          id: resourceId('Microsoft.Network/applicationGateways/httpListeners', appGatewayFullName, '${site.resourceName}-listener')
        }
        backendAddressPool: {
          id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', appGatewayFullName, '${site.resourceName}-backend-pool')
        }
        backendHttpSettings: {
          id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', appGatewayFullName, '${site.resourceName}-backend')
        }
      }
    }]
    probes: [for site in sites: {
      name: '${site.resourceName}-health-probe'
      properties: {
        protocol: 'Https'
        path: site.healthProbeRelativeUrl
        interval: 30
        timeout: 30
        unhealthyThreshold: 3
        pickHostNameFromBackendHttpSettings: true
        minServers: 0
        match: {}
      }
    }]
    enableHttp2: true
    autoscaleConfiguration: {
      minCapacity: 0
      maxCapacity: 10
    }
    firewallPolicy: {
      id: wafPolicy.outputs.id
    }
  }
  dependsOn: [
    publicIPAddresses
  ]
}

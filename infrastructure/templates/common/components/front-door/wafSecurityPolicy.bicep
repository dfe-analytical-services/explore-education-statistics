@description('Name of the security policy.')
param securityPolicyName string

@description('Name of the WAF policy.')
param wafPolicyName string

@description('Name of the Azure Front Door profile.')
param frontDoorProfileName string

@description('Name of the custom domain to associate this policy with.')
param customDomainName string

resource wafPolicy 'Microsoft.Network/frontdoorwebapplicationfirewallpolicies@2025-10-01' existing = {
  name: wafPolicyName
}

resource frontDoorProfile 'Microsoft.Cdn/profiles@2025-04-15' existing = {
  name: frontDoorProfileName
}

resource customDomain 'Microsoft.Cdn/profiles/customdomains@2025-04-15' existing = {
  parent: frontDoorProfile
  name: customDomainName
}

resource securityPolicy 'Microsoft.Cdn/profiles/securitypolicies@2025-09-01-preview' = {
  parent: frontDoorProfile
  name: securityPolicyName
  properties: {
    parameters: {
      wafPolicy: {
        id: wafPolicy.id
      }
      type: 'WebApplicationFirewall'
      associations: [
        {
          domains: [
            {
              id: customDomain.id
            }
          ]
          patternsToMatch: [
            '/*'
          ]
        }
      ]
    }
  }
}

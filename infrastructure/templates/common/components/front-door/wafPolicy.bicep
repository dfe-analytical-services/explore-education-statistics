@description('Name of the policy.')
param policyName string

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

resource wafPolicy 'Microsoft.Network/frontdoorwebapplicationfirewallpolicies@2025-10-01' = {
  name: policyName
  location: 'Global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: 'Prevention'
      requestBodyCheck: 'Enabled'
    }
  }
  tags: tagValues
}

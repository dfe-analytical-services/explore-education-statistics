import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Secret header value from FUAPI.')
@secure()
param fuapiHeaderValue string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var policyName = '${resourceNames.sharedResources.appGateway}-public-api-afwp'

module wafPolicyModule '../../components/appGatewayWafPolicy.bicep' = {
  name: 'wafPolicy'
  params: {
    name: policyName
    location: location
    // Do not define any managed rulesets for this WAF policy. The global WAF policy
    // will cover these cases.
    managedRuleSets: []
    // Add a custom rule to only allow traffic that contains a correct secret 
    // header value that is included from FUAPI.
    customRules: [
      {
        name: 'fuapisecretheader'
        action: 'Block'
        matchConditions: [
          {
            type: 'RequestHeaders'
            selector: 'X-FUAPI-Secret'
            operator: 'Equal'
            operatorNegation: true
            matchValues: [
              fuapiHeaderValue
            ]
          }
        ]
      }
    ]
    tagValues: tagValues
  }
}

output id string = wafPolicyModule.outputs.id
output name string = policyName

@description('Specifies the location for all resources.')
param location string

@description('Specifies the name of the Application Gateway.')
param appGatewayName string

@description('Secret header value from FUAPI.')
@secure()
param fuapiSecretValue string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var policyName = '${appGatewayName}-public-api-afwp'

module wafPolicyModule '../../../common/components/application-gateway/appGatewayWafPolicy.bicep' = {
  name: 'publicApiWafPolicyDeploy'
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
        name: 'fuapisecretnotpresent'
        action: 'Block'
        matchConditions: [
          {
            type: 'RequestHeaders'
            selector: 'X-FUAPI-Secret'
            operator: 'Any'
            negateOperator: true
          }
        ]
      }
      {
        name: 'fuapisecretincorrect'
        action: 'Block'
        matchConditions: [
          {
            type: 'RequestHeaders'
            selector: 'X-FUAPI-Secret'
            operator: 'Equal'
            negateOperator: true
            matchValues: [
              fuapiSecretValue
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

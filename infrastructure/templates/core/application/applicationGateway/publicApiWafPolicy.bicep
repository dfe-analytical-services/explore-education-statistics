@description('Specifies the location for all resources.')
param location string

@description('Specifies the name of the WAF policy.')
param policyName string

@description('''
Secret header values from FaUAPI, as a JSON array of strings, for example ["value1", "value2"]. A request is allowed if
its header matches any one of them, so a new value can be added before FaUAPI switches to it and the old value removed
afterwards, allowing us to rotate secrets. This is a string rather than an array because Key Vault secrets can only be
passed into secure string parameters.
''')
@secure()
param fauapiSecretValues string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

module wafPolicyModule '../../../common/components/application-gateway/appGatewayWafPolicy.bicep' = {
  name: 'publicApiWafPolicyDeploy'
  params: {
    name: policyName
    location: location
    // Do not define any managed rulesets for this WAF policy. The global WAF policy
    // will cover these cases.
    managedRuleSets: []
    // Add a custom rule to only allow traffic that contains a correct secret 
    // header value that is included from FaUAPI.
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
            matchValues: json(fauapiSecretValues)
          }
        ]
      }
    ]
    tagValues: tagValues
  }
}

output id string = wafPolicyModule.outputs.id
output name string = policyName

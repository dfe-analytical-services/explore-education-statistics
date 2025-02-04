import { AppGatewayFirewallPolicyCustomRule } from '../types.bicep'

@description('Specifies the location for all resources')
param location string

@description('Specifies the name of the policy')
param name string

@description('A set of managed rulesets to include alongside the default ruleset')
param defaultRuleSet {
  ruleSetType: string
  ruleSetVersion: string
} = {
  ruleSetType: 'Microsoft_DefaultRuleSet'
  ruleSetVersion: '2.1'
}

@description('A set of managed rulesets to include alongside the default ruleset')
param managedRuleSets {
  ruleSetType: string
  ruleSetVersion: string
}[] = [
  {
    ruleSetType: 'Microsoft_BotManagerRuleSet'
    ruleSetVersion: '1.1'
  }
]

@description('A set of custom rules to include alongside the managed rulesets')
param customRules AppGatewayFirewallPolicyCustomRule[] = []

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

resource policy 'Microsoft.Network/ApplicationGatewayWebApplicationFirewallPolicies@2023-11-01' = {
  name: name
  location: location
  properties: {
    policySettings: {
      requestBodyCheck: true
      maxRequestBodySizeInKb: 128
      fileUploadLimitInMb: 100
      state: 'Enabled'
      mode: 'Prevention'
      requestBodyInspectLimitInKB: 128
      fileUploadEnforcement: true
      requestBodyEnforcement: true
    }
    managedRules: {
      managedRuleSets: [
        defaultRuleSet
        ...managedRuleSets
      ]
    }
    customRules: map(customRules, (rule, index) => {
      name: rule.name
      action: rule.action
      ruleType: 'MatchRule'
      priority: rule.?priority ?? 1 + (index * 5)
      matchConditions: map(rule.matchConditions, condition => {
        matchVariables: [{
          variableName: condition.type
          selector: condition.?selector
        }]
        operator: condition.operator
        negationConditon: condition.negateOperator
        matchValues: condition.?matchValues ?? []
      })

    })
  }
  tags: tagValues
}

output id string = policy.id

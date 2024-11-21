import { AppGatewayPathRule } from '../types.bicep'

@description('Name of the App Gateway')
param appGatewayName string

@description('Routes the App Gateway should direct traffic through')
param pathRules AppGatewayPathRule[]

var rules = [for rule in pathRules: {
  name: rule.name
  properties: {
    paths: rule.paths
    backendAddressPool: rule.type == 'backend' ? {
      id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', appGatewayName, '${rule.backendName}-backend-pool')
    } : null
    backendHttpSettings: rule.type == 'backend' ? {
      id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', appGatewayName, '${rule.backendName}-backend')
    } : null
    redirectConfiguration: rule.type == 'redirect' ? {
      id: resourceId('Microsoft.Network/applicationGateways/redirectConfigurations', appGatewayName, rule.name)
    } : null
    rewriteRuleSet: rule.type == 'backend' && rule.rewriteSetName != null ? {
      id: resourceId('Microsoft.Network/applicationGateways/rewriteRuleSets', appGatewayName, rule.rewriteSetName)
    } : null
  }
}]

output rules object[] = rules

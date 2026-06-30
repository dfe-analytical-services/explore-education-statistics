@export()
type AppGatewayFrontendConfig = {
  @description('Name of the frontend config')
  name: string

  @description('Name of the public IP address to associate with this frontend config')
  publicIpAddressName: string
}

@export()
type AppGatewaySite = {
  @description('Name of the site')
  name: string

  @description('Name of the certificate that applies for this site')
  certificateName: string

  @description('FQDN of the site')
  fqdn: string

  @description('''
  Optional WAF policy name to apply to this listener, in conjunction with 
  any global App Gateway WAF policies
  ''')
  wafPolicyName: string?
}

@export()
type AppGatewayBackend = {
  @description('Name of the backend')
  name: string

  @description('FQDN of the backend')
  fqdn: string

  @description('Path that the health probe should periodically ping e.g. /')
  healthProbePath: string

  @description('Additional health probe HTTP status codes that indicate success')
  healthProbeAdditionalStatusCodeMatches: string[]?

  @description('Interval in seconds that the health probe is called. Defaults to 30 seconds if not set.')
  intervalSeconds: int?
}

@export()
type AppGatewayRoute = {
  @description('Name of the route')
  name: string

  @description('Name of the site that will receive incoming traffic for this route')
  siteName: string

  @description('Name of the backend that the route will direct traffic to by default if no path rule applies')
  defaultBackendName: string

  @description('A set of path rules to apply')
  pathRules: AppGatewayPathRule[]?
}

@export()
@discriminator('type')
type AppGatewayPathRule = AppGatewayBackendPathRule | AppGatewayRedirectPathRule

@export()
type AppGatewayBackendPathRule = {
  type: 'backend'

  @description('Name of the rule')
  name: string

  @description('Request paths that apply for this rule')
  paths: string[]

  @description('Name of the backend to direct traffic to')
  backendName: string?

  @description('The name of the rewrite set that applies for this path')
  rewriteSetName: string?
}

@export()
type AppGatewayRedirectPathRule = {
  type: 'redirect'

  @description('Name of the rule')
  name: string

  @description('Request paths that apply for this rule')
  paths: string[]

  @description('URL to redirect traffic to')
  redirectUrl: string?

  @description('Type of redirect')
  redirectType: 'Temporary' | 'Permanent'

  @description('Append the request path to the redirected URL. Defaults to true')
  includePath: bool?
}

@export()
type AppGatewayRewriteSet = {
  @description('Name of the rewrite set')
  name: string

  @description('The rewrite rules in the rewrite set')
  rules: AppGatewayRewriteRule[]
}

@export()
type AppGatewayRewriteRule = {
  @description('Name of the rewrite rule')
  name: string

  @description('Conditions that need to be met for rewrite rule to trigger')
  conditions: AppGatewayRewriteCondition[]

  @description(' that should be performed when the rewrite rule triggers')
  actionSet: AppGatewayRewriteActionSet
}

@export()
type AppGatewayRewriteCondition = {
  @description('The server variable to check - see https://learn.microsoft.com/en-us/azure/application-gateway/rewrite-http-headers-url#server-variables')
  variable: string

  @description('A string or regex to compare with the variable')
  pattern: string

  @description('Negate the condition check. Defaults to false')
  negate: bool?

  @description('Ignore case when checking the variable. Defaults to true')
  ignoreCase: bool?
}

@export()
type AppGatewayRewriteActionSet = {
  @description('Rewrite config for the URL')
  urlConfiguration: AppGatewayRewriteUrlConfig?

  @description('Rewrite config for the URL')
  responseHeaderConfigurations: AppGatewayRewriteHeaderConfig[]?
}

@export()
type AppGatewayRewriteUrlConfig = {
  @description('The path after it has been rewritten. Can contain variables from other parts of the request')
  modifiedPath: string?

  @description('The query string after it has been rewritten. Can contain variables from other parts of the original request')
  modifiedQueryString: string?

  @description('Re-evaluate any associated URL path maps with the rewritten path. Defaults to false')
  reroute: bool?
}

@export()
type AppGatewayRewriteHeaderConfig = {
  @description('The HTTP header to inspect')
  headerName: string

  @description('The header value after it has been rewritten. Can contain variables from other parts of the original header')
  headerValue: string
}

// This is not an exhaustive list. For a list of available operators, see:
// https://learn.microsoft.com/en-us/azure/templates/microsoft.network/applicationgatewaywebapplicationfirewallpolicies?pivots=deployment-language-bicep#matchcondition
type AppGatewayFirewallPolicyCustomRuleOperator = 
  | 'Equal'
  | 'BeginsWith'
  | 'Any'

@export()
type AppGatewayFirewallPolicyCustomRuleMatchCondition = {

  // For a full list of options, see:
  // https://learn.microsoft.com/en-us/azure/templates/microsoft.network/applicationgatewaywebapplicationfirewallpolicies?pivots=deployment-language-bicep#matchvariable
  @description('Type of match condition - selects an HTTP header')
  type: 'RequestHeaders' | 'RequestUri'

  @description('Name of the variable type to inspect e.g. name of the HTTP header when type is RequestHeaders')
  selector: string?

  @description('Operator for the match test')
  operator: AppGatewayFirewallPolicyCustomRuleOperator

  @description('Whether or not to negate the operator')
  negateOperator: bool

  @description('Array of possible values to match')
  matchValues: string[]?
}

@export()
type AppGatewayFirewallPolicyCustomRule = {
  @description('Name of the rule')
  name: string

  @description('Optional priority of the rule, from 1 to 100. If none specified, the array order of custom rules will define the priority')
  priority: int?

  @description('Whether to deny or allow access based on this rule')
  action: 'Allow' | 'Block'

  @description('A set of match conditions that apply to this rule. They are combined with the AND operator')
  matchConditions: AppGatewayFirewallPolicyCustomRuleMatchCondition[]
}

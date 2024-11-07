@export()
type ResourceNames = {
  existingResources: {
    adminApp: string
    publisherFunction: string
    keyVault: string
    vNet: string
    alertsGroup: string
    acr: string
    acrResourceGroup: string
    coreStorageAccount: string
    subnets: {
      dataProcessor: string
      dataProcessorPrivateEndpoints: string
      containerAppEnvironment: string
      psqlFlexibleServer: string
      appGateway: string
      adminApp: string
      publisherFunction: string
    }
  }
  sharedResources: {
    appGateway: string
    appGatewayIdentity: string
    containerAppEnvironment: string
    logAnalyticsWorkspace: string
    postgreSqlFlexibleServer: string
  }
  publicApi: {
    apiApp: string
    apiAppIdentity: string
    appInsights: string
    dataProcessor: string
    dataProcessorIdentity: string
    dataProcessorPlan: string
    dataProcessorStorageAccountsPrefix: string
    docsApp: string
    publicApiStorageAccount: string
    publicApiFileshare: string
  }
}

@export()
type FirewallRule = {
  name: string
  cidr: string
}

@export()
type AzureFileshareMount = {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}

@export()
type EntraIdAuthentication = {
  appRegistrationClientId: string
  allowedClientIds: string[]
  allowedPrincipalIds: string[]
  requireAuthentication: bool
}

@export()
type AppGatewaySite = {
  @description('Name of the site')
  name: string

  @description('Name of the certificate that applies for this site')
  certificateName: string

  @description('FQDN of the site')
  fqdn: string
}

@export()
type AppGatewayBackend = {
  @description('Name of the backend')
  name: string

  @description('FQDN of the backend')
  fqdn: string

  @description('Path that the health probe should periodically ping e.g. /')
  healthProbePath: string
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
  pathRules: AppGatewayPathRule[]
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
  urlConfiguration: AppGatewayRewriteUrlConfig
}

@export()
type AppGatewayRewriteUrlConfig = {
  @description('The path after it has been rewritten. Can contain variables from other parts of the request')
  modifiedPath: string?

  @description('The query string after it has been rewritten. Can contain variables from other parts of the request')
  modifiedQueryString: string?

  @description('Re-evaluate any associated URL path maps with the rewritten path. Defaults to false')
  reroute: bool?
}

@export()
type PrincipalNameAndId = {
  principalName: string
  objectId: string
}

@export()
type PrivateDnsZone = 'sites' | 'postgres' | 'custom'

@export()
type ContainerRegistryRole = 'AcrPull'

@export()
type KeyVaultRole = 'Secrets User' | 'Certificate User'

@export()
type StaticWebAppSku = 'Free' | 'Standard'

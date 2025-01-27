@description('Specifies the App Registration Client Id that represents the resource being protected')
param clientId string

@description('Specifies the name of the site that is being protected')
param containerAppName string

@description('Specifies an optional set of App Registration Client Ids that represent resources that are allowed to access this resource')
param allowedClientIds string[] = []

@description('Specifies an optional set of Principal Ids of Managed Identities that are allowed to access this resource')
param allowedPrincipalIds string[] = []

@description('Specifies whether all calls to this resource should be authenticated or not.  Defaults to true')
param requireAuthentication bool = true

resource containerAppConfig 'Microsoft.App/containerApps/authConfigs@2024-03-01' = {
  name: '${containerAppName}/current'  
  properties: {
    platform: {
      enabled: true
    }
    globalValidation: {
      unauthenticatedClientAction: requireAuthentication ? 'Return401' : 'AllowAnonymous'
    }
    httpSettings: {
      requireHttps: true
    }
    identityProviders: {
      azureActiveDirectory:{
        enabled: true
        isAutoProvisioned: true 
        registration: {
          clientId: clientId
          openIdIssuer: 'https://sts.windows.net/${tenant().tenantId}/v2.0'
        }
        validation: {
          allowedAudiences: [
            'api://${clientId}'
          ]
          defaultAuthorizationPolicy: {
            allowedApplications: union(
              [clientId],
              allowedClientIds
            )
            allowedPrincipals: {
              identities: allowedPrincipalIds
            }
          }
        }
      }
    }
  }
}

output configName string = containerAppConfig.name

@description('Specifies the App Registration Client Id that represents the resource being protected')
param clientId string

@description('Specifies the name of the site that is being protected')
param siteName string

@description('Specifies an optional name of the staging slot of the site that is being protected, if a staging slot exists')
param stagingSlotName string?

@description('Specifies an optional set of App Registration Client Ids that represent resources that are allowed to access this resource')
param allowedClientIds string[] = []

@description('Specifies an optional set of Principal Ids of Managed Identities that are allowed to access this resource')
param allowedPrincipalIds string[] = []

@description('Specifies whether all calls to this resource should be authenticated or not.  Defaults to true')
param requireAuthentication bool = true

var properties = {
  globalValidation: {
    requireAuthentication: requireAuthentication
    unauthenticatedClientAction: requireAuthentication ? 'Return401' : null
  }
  httpSettings: {
    requireHttps: true
  }
  identityProviders: {
    azureActiveDirectory: {
      enabled: true
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
  platform: {
    enabled: true
    runtimeVersion: '~1'
  }
}

resource siteAuthSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  name: '${siteName}/authsettingsV2'
  properties: properties
}

resource stagingSlotAuthSettings 'Microsoft.Web/sites/slots/config@2023-12-01' = if (!empty(stagingSlotName)) {
  name: '${siteName}/${stagingSlotName}/authsettingsV2'
  properties: properties
}

output sitePropertiesName string = siteAuthSettings.name
output stagingSlotPropertiesName string = stagingSlotAuthSettings.name

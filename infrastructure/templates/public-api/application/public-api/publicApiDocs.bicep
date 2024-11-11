import { ResourceNames, StaticWebAppSku } from '../../types.bicep'

@description('Common resource naming variables')
param resourceNames ResourceNames

@description('Static Web App SKU to use')
param appSku StaticWebAppSku = 'Free'

@description('Tags for the resources')
param tagValues object

module publicApiDocsApp  '../../components/staticWebApp.bicep' = {
  name: 'publicApiDocsAppDeploy'
  params: {
    name: resourceNames.publicApi.docsApp
    tagValues: tagValues
    sku: appSku
  }
}

output appFqdn string = publicApiDocsApp.outputs.fqdn
output healthProbePath string = '/'

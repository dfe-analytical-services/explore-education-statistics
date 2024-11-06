import { resourceNamesType, staticWebAppSkuType } from '../../types.bicep'

@description('Common resource naming variables')
param resourceNames resourceNamesType

@description('Static Web App SKU to use')
param appSku staticWebAppSkuType = 'Free'

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

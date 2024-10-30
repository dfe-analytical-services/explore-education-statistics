import { resourceNamesType, staticWebAppSkuType } from '../../types.bicep'

@description('Common resource naming variables.')
param resourceNames resourceNamesType

@description('Static Web App SKU to use.')
param appSku staticWebAppSkuType = 'Free'

@description('A set of tags for the resource.')
param tagValues object

module publicApiDocs  '../../components/staticWebApp.bicep' = {
  name: 'publicApiDocs'
  params: {
    name: resourceNames.publicApi.docsApp
    tagValues: tagValues
    sku: appSku
  }
}

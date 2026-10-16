import { ResourceNames } from '../../../common/resource-names.bicep'
import { StaticWebAppSku } from '../../types.bicep'

@description('Names of the Public API resources.')
param names ResourceNames.publicApi

@description('Static Web App SKU to use')
param appSku StaticWebAppSku = 'Free'

@description('Tags for the resources')
param tagValues object

module publicApiDocsApp  '../../components/staticWebApp.bicep' = {
  name: 'publicApiDocsAppDeploy'
  params: {
    name: names.docsApp
    tagValues: tagValues
    sku: appSku
  }
}

output appFqdn string = publicApiDocsApp.outputs.fqdn
output healthProbePath string = '/'

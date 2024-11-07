import { staticWebAppSkuType } from '../types.bicep'

@description('Name of the resource.')
param name string

@description('Location for an optional Azure Functions API backend (only certain regions allowed).')
@allowed(['centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2'])
param location string = 'westeurope'

@description('Static Web App SKU to use.')
param sku staticWebAppSkuType = 'Free'

@description('A set of tags for the resource.')
param tagValues object

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  tags: tagValues
  sku: {
    name: sku
    size: sku
  }
  properties: {}
}

output fqdn string = staticWebApp.properties.defaultHostname

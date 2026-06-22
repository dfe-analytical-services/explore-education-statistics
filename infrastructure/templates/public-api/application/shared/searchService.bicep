@description('The name of the existing Azure AI Search service.')
param name string

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: name
}

output searchServiceEndpoint string = searchService.properties.endpoint

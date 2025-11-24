@description('The name of the existing Search service.')
param name string

resource searchService 'Microsoft.Search/searchServices@2025-02-01-preview' existing = {
  name: name
}

output searchServiceEndpoint string = searchService.properties.endpoint

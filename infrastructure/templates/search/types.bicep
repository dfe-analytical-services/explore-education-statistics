@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      searchDocsFunction: string
      searchDocsFunctionPrivateEndpoints: string
      searchDocsStoragePrivateEndpoints: string
    }
  }
  search: {
    applicationInsights: string
    searchDocsFunction: string
    searchDocsFunctionStorageAccount: string
    searchDocsStorageAccount: string
    searchService: string
  }
}

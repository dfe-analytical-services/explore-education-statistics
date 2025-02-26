@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      searchDocsFunctionPrivateEndpoints: string
      searchDocsStoragePrivateEndpoints: string
    }
  }
  search: {
    searchDocsFunction: string
    searchDocsStorageAccount: string
    searchService: string
  }
}

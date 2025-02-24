@export()
type ResourceNames = {
  existingResources: {
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      searchDocsStoragePrivateEndpoints: string
    }
  }
  search: {
    searchDocsStorageAccount: string
    searchService: string
  }
}

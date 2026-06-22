@export()
type ResourceNames = {
  existingResources: {
    logAnalyticsWorkspace: string
    keyVault: string
    vNet: string
    alertsGroup: string
    subnets: {
      nlSearchFunctionApp: string
      nlSearchFunctionAppPrivateEndpoints: string
      searchDocsFunctionApp: string
      searchDocsFunctionAppPrivateEndpoints: string
      searchStoragePrivateEndpoints: string
    }
  }
}

@export()
@sealed()
type SearchStorageQueueNames = {
  @description('Queue name for when a publication is archived.')
  publicationArchived: string
  @description('Queue name for when a publication is changed.')
  publicationChanged: string
  @description('Queue name for when a publication is deleted.')
  publicationDeleted: string
  @description('Queue name for when the latest published release of a publication changes due to reordering.')
  publicationLatestPublishedReleaseReordered: string
  @description('Queue name for when an archived publication is restored.')
  publicationRestored: string
  @description('Queue name for when a searchable document requires a refresh.')
  refreshSearchableDocument: string
  @description('Queue name for when a release slug is changed.')
  releaseSlugChanged: string
  @description('Queue name for when a release version is published.')
  releaseVersionPublished: string
  @description('Queue name for removing searchable documents associated with a publication.')
  removePublicationSearchableDocuments: string
  @description('Queue name for removing a single searchable document.')
  removeSearchableDocument: string
  @description('Queue name for when a searchable document is created.')
  searchableDocumentCreated: string
  @description('Queue name for when a theme is updated.')
  themeUpdated: string
}

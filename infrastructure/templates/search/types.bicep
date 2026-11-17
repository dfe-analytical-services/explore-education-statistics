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
  @description('Queue name for when a release document requires a refresh.')
  refreshReleaseDocument: string
  @description('Queue name for when a release document is created.')
  releaseDocumentCreated: string
  @description('Queue name for when a release slug is changed.')
  releaseSlugChanged: string
  @description('Queue name for when a release version is published.')
  releaseVersionPublished: string
  @description('Queue name for removing release documents associated with a publication.')
  removePublicationReleaseDocuments: string
  @description('Queue name for removing a single release document.')
  removeReleaseDocument: string
  @description('Queue name for when a theme is updated.')
  themeUpdated: string
}

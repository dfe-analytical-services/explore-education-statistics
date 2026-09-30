import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import { NavRouteProps } from '@admin/routes/types';

export type ReleaseRouteParams = {
  publicationId: string;
  releaseVersionId: string;
};

export type ReleaseDataBlockRouteParams = ReleaseRouteParams & {
  dataBlockVersionId: string;
};

export type ReleaseAncillaryFileRouteParams = ReleaseRouteParams & {
  fileId: string;
};

export type ReleaseDataFileReplaceRouteParams = ReleaseRouteParams & {
  fileId: string;
};

export type ReleaseFootnoteRouteParams = ReleaseRouteParams & {
  footnoteId: string;
};

export type ReleaseDataSetRouteParams = ReleaseRouteParams & {
  dataSetId: string;
};

export type ReleaseDataSetPreviewTokenRouteParams =
  ReleaseDataSetRouteParams & {
    previewTokenId: string;
  };

export type ReleaseDataSetChangelogRouteParams = ReleaseDataSetRouteParams & {
  dataSetVersionId: string;
};

export type ReleaseRouteProps = NavRouteProps;

export const releaseSummaryRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/summary',
  path: 'summary',
  title: 'Summary',
};

export const releaseSummaryEditRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/summary/edit',
  path: 'summary/edit',
  title: 'Edit summary',
};

export const releaseDataRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/data',
  path: 'data',
  title: 'Data and files',
};

export const releaseAncillaryFilesRoute: ReleaseRouteProps = {
  fullPath: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.fileUploads.id}`,
  path: `data#${releaseDataPageTabs.fileUploads.id}`,
  title: 'Data and files',
};

export const releaseAncillaryFileRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/ancillary/:fileId',
  path: 'ancillary/:fileId',
  title: 'Ancillary file',
};

export const releaseDataFileReplaceRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replace',
  path: 'data/:fileId/replace',
  title: 'Replace data file',
};

export const releaseDataFileReplacementCompleteRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replacement-complete',
  path: 'data/:fileId/replacement-complete',
  title: 'Replacement complete',
};

export const releaseApiDataSetsRoute: ReleaseRouteProps = {
  fullPath: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.apiDataSets.id}`,
  path: `data#${releaseDataPageTabs.apiDataSets.id}`,
  title: 'API data sets',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetDetailsRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId',
  path: 'api-data-sets/:dataSetId',
  title: 'API data set details',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetFiltersMappingRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/filters-mapping',
  path: 'api-data-sets/:dataSetId/filters-mapping',
  title: 'API data set filters mapping',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetIndicatorsMappingRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/indicators-mapping',
  path: 'api-data-sets/:dataSetId/indicators-mapping',
  title: 'API data set indicators mapping',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetLocationsMappingRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/locations-mapping',
  path: 'api-data-sets/:dataSetId/locations-mapping',
  title: 'API data set locations mapping',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetPreviewRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview',
  path: 'api-data-sets/:dataSetId/preview',
  title: 'Preview API data set',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetPreviewTokenRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens/:previewTokenId',
  path: 'api-data-sets/:dataSetId/preview-tokens/:previewTokenId',
  title: 'API data set preview token',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetPreviewTokenLogRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens',
  path: 'api-data-sets/:dataSetId/preview-tokens',
  title: 'View API data set token log',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetVersionHistoryRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/versions',
  path: 'api-data-sets/:dataSetId/versions',
  title: 'API data set version history',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseApiDataSetChangelogRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/changelog/:dataSetVersionId',
  path: 'api-data-sets/:dataSetId/changelog/:dataSetVersionId',
  title: 'View API data set token log',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseFootnotesRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/footnotes',
  path: 'footnotes',
  title: 'Footnotes',
};

export const releaseFootnotesCreateRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/create-footnote',
  path: 'create-footnote',
  title: 'Create footnote',
};

export const releaseFootnotesEditRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/footnotes/:footnoteId',
  path: 'footnotes/:footnoteId',
  title: 'Edit footnote',
};

export const releaseDataBlocksRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/data-blocks',
  path: 'data-blocks',
  title: 'Data blocks',
};

export const releaseTableToolRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/data-blocks/table-tool',
  path: 'data-blocks/table-tool',
  title: 'Table tool',
};

export const releaseDataBlockCreateRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/data-blocks/create',
  path: 'data-blocks/create',
  title: 'Create data block',
};

export const releaseDataBlockEditRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/data-blocks/:dataBlockVersionId',
  path: 'data-blocks/:dataBlockVersionId',
  title: 'Edit data block',
};

export const releaseContentRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/content',
  path: 'content',
  title: 'Content',
};

export const releaseChecklistRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/checklist',
  path: 'checklist',
  title: 'Publishing checklist',
};

export const releaseStatusRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/status',
  path: 'status',
  title: 'Sign off',
};

export const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease-access',
  path: 'prerelease-access',
  title: 'Pre-release access',
};

/**
 * The routes shown in the release nav bar, in the order they are stepped
 * through by the previous/next links. Filtered by the user's permissions in
 * `ReleasePageContainer`.
 */
export const releaseNavRoutes: ReleaseRouteProps[] = [
  releaseSummaryRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releaseDataBlocksRoute,
  releaseContentRoute,
  releaseChecklistRoute,
  releaseStatusRoute,
  releasePreReleaseAccessRoute,
];

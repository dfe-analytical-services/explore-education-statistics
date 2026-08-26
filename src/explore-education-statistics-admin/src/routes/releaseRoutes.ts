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
  path: '/publication/:publicationId/release/:releaseVersionId/summary',
  title: 'Summary',
};

export const releaseSummaryEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/summary/edit',
  title: 'Edit summary',
};

export const releaseDataRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data',
  title: 'Data and files',
};

export const releaseAncillaryFilesRoute: ReleaseRouteProps = {
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.fileUploads.id}`,
  title: 'Data and files',
};

export const releaseAncillaryFileRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/ancillary/:fileId',
  title: 'Ancillary file',
};

export const releaseDataFileReplaceRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replace',
  title: 'Replace data file',
};

export const releaseDataFileReplacementCompleteRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replacement-complete',
  title: 'Replacement complete',
};

export const releaseApiDataSetsRoute: ReleaseRouteProps = {
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.apiDataSets.id}`,
  title: 'API data sets',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetDetailsRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId',
  title: 'API data set details',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetFiltersMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/filters-mapping',
  title: 'API data set filters mapping',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetIndicatorsMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/indicators-mapping',
  title: 'API data set indicators mapping',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetLocationsMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/locations-mapping',
  title: 'API data set locations mapping',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview',
  title: 'Preview API data set',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewTokenRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens/:previewTokenId',
  title: 'API data set preview token',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewTokenLogRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens',
  title: 'View API data set token log',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetVersionHistoryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/versions',
  title: 'API data set version history',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetChangelogRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/changelog/:dataSetVersionId',
  title: 'View API data set token log',
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseFootnotesRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/footnotes',
  title: 'Footnotes',
};

export const releaseFootnotesCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/create-footnote',
  title: 'Create footnote',
};

export const releaseFootnotesEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/footnotes/:footnoteId',
  title: 'Edit footnote',
};

export const releaseDataBlocksRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks',
  title: 'Data blocks',
};

export const releaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/table-tool',
  title: 'Table tool',
};

export const releaseDataBlockCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/create',
  title: 'Create data block',
};

export const releaseDataBlockEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/:dataBlockVersionId',
  title: 'Edit data block',
};

export const releaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/content',
  title: 'Content',
};

export const releaseChecklistRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/checklist',
  title: 'Publishing checklist',
};

export const releaseStatusRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/status',
  title: 'Sign off',
};

export const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease-access',
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

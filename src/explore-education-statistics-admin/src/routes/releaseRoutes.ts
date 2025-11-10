import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import ReleaseApiDataSetFiltersMappingPage from '@admin/pages/release/data/ReleaseApiDataSetFiltersMappingPage';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ReleaseApiDataSetPreviewPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewPage';
import ReleaseApiDataSetPreviewTokenPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenPage';
import ReleaseApiDataSetPreviewTokenLogPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenLogPage';
import ReleaseApiDataSetVersionHistoryPage from '@admin/pages/release/data/ReleaseApiDataSetVersionHistoryPage';
import ReleaseApiDataSetChangelogPage from '@admin/pages/release/data/ReleaseApiDataSetChangelogPage';
import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import ReleaseDataFilePage from '@admin/pages/release/data/ReleaseDataFilePage';
import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import ReleaseDataBlockCreatePage from '@admin/pages/release/datablocks/ReleaseDataBlockCreatePage';
import ReleaseDataBlockEditPage from '@admin/pages/release/datablocks/ReleaseDataBlockEditPage';
import ReleaseDataBlocksPage from '@admin/pages/release/datablocks/ReleaseDataBlocksPage';
import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import ReleaseFootnoteCreatePage from '@admin/pages/release/footnotes/ReleaseFootnoteCreatePage';
import ReleaseFootnoteEditPage from '@admin/pages/release/footnotes/ReleaseFootnoteEditPage';
import ReleaseFootnotesPage from '@admin/pages/release/footnotes/ReleaseFootnotesPage';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import ReleasePublishStatusPage from '@admin/pages/release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/ReleaseSummaryPage';

export type ReleaseRouteParams = {
  publicationId: string;
  releaseVersionId: string;
};

export type ReleaseDataBlockRouteParams = ReleaseRouteParams & {
  dataBlockId: string;
};

export type ReleaseDataFileRouteParams = ReleaseRouteParams & {
  fileId: string;
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

export interface ReleaseRouteProps extends ProtectedRouteProps {
  title: string;
  path: string;
}

export const releaseSummaryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/summary',
  title: 'Summary',
  component: ReleaseSummaryPage,
};

export const releaseSummaryEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/summary/edit',
  title: 'Edit summary',
  component: ReleaseSummaryEditPage,
};

export const releaseDataRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data',
  title: 'Data and files',
  component: ReleaseDataPage,
};

export const releaseAncillaryFilesRoute: ReleaseRouteProps = {
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.fileUploads.id}`,
  title: 'Data and files',
  component: ReleaseDataPage,
};

export const releaseAncillaryFileRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/ancillary/:fileId',
  title: 'Ancillary file',
  component: ReleaseAncillaryFilePage,
};

export const releaseDataFileRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data/:fileId',
  title: 'Data file',
  component: ReleaseDataFilePage,
};

export const releaseDataFileReplaceRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replace',
  title: 'Replace data file',
  component: ReleaseDataFileReplacePage,
};

export const releaseDataFileReplacementCompleteRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data/:fileId/replacement-complete',
  title: 'Replacement complete',
  component: ReleaseDataFileReplacementCompletePage,
};

export const releaseApiDataSetsRoute: ReleaseRouteProps = {
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabs.apiDataSets.id}`,
  title: 'API data sets',
  component: ReleaseDataPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetDetailsRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId',
  title: 'API data set details',
  component: ReleaseApiDataSetDetailsPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetFiltersMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/filters-mapping',
  title: 'API data set filters mapping',
  component: ReleaseApiDataSetFiltersMappingPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetLocationsMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/locations-mapping',
  title: 'API data set locations mapping',
  component: ReleaseApiDataSetLocationsMappingPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview',
  title: 'Preview API data set',
  component: ReleaseApiDataSetPreviewPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewTokenRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens/:previewTokenId',
  title: 'API data set preview token',
  component: ReleaseApiDataSetPreviewTokenPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetPreviewTokenLogRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/preview-tokens',
  title: 'View API data set token log',
  component: ReleaseApiDataSetPreviewTokenLogPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetVersionHistoryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/versions',
  title: 'API data set version history',
  component: ReleaseApiDataSetVersionHistoryPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseApiDataSetChangelogRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/api-data-sets/:dataSetId/changelog/:dataSetVersionId',
  title: 'View API data set token log',
  component: ReleaseApiDataSetChangelogPage,
  protectionAction: permissions => permissions.isBauUser,
};

export const releaseFootnotesRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/footnotes',
  title: 'Footnotes',
  component: ReleaseFootnotesPage,
};

export const releaseFootnotesCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/create-footnote',
  title: 'Create footnote',
  component: ReleaseFootnoteCreatePage,
};

export const releaseFootnotesEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/footnotes/:footnoteId',
  title: 'Edit footnote',
  component: ReleaseFootnoteEditPage,
};

export const releaseDataBlocksRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks',
  title: 'Data blocks',
  component: ReleaseDataBlocksPage,
};

export const releaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/table-tool',
  title: 'Table tool',
  component: ReleaseTableToolPage,
};

export const releaseDataBlockCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/create',
  title: 'Create data block',
  component: ReleaseDataBlockCreatePage,
};

export const releaseDataBlockEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-blocks/:dataBlockId',
  title: 'Edit data block',
  component: ReleaseDataBlockEditPage,
};

export const releaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/content',
  title: 'Content',
  component: ReleaseContentPage,
};

export const releaseStatusRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/status',
  title: 'Sign off',
  component: ReleasePublishStatusPage,
};

export const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease-access',
  title: 'Pre-release access',
  component: ReleasePreReleaseAccessPage,
};

import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import ReleaseDataFilePage from '@admin/pages/release/data/ReleaseDataFilePage';
import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
import releaseDataPageTabIds from '@admin/pages/release/data/utils/releaseDataPageTabIds';
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
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabIds.fileUploads}`,
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
  path: `/publication/:publicationId/release/:releaseVersionId/data#${releaseDataPageTabIds.apiDataSets}`,
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

export const releaseApiDataSetLocationsMappingRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/api-data-sets/:dataSetId/locations-mapping',
  title: 'API data set locations mapping',
  component: ReleaseApiDataSetLocationsMappingPage,
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

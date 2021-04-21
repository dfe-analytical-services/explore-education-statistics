import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import ReleaseDataFilePage from '@admin/pages/release/data/ReleaseDataFilePage';
import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
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
import { RouteProps } from 'react-router';

export type ReleaseRouteParams = {
  publicationId: string;
  releaseId: string;
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

export interface ReleaseRouteProps extends RouteProps {
  title: string;
  path: string;
}

export const releaseSummaryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/summary',
  title: 'Summary',
  component: ReleaseSummaryPage,
};

export const releaseSummaryEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/summary/edit',
  title: 'Edit summary',
  component: ReleaseSummaryEditPage,
};

export const releaseDataRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data',
  title: 'Data and files',
  component: ReleaseDataPage,
};

export const releaseDataAncillaryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data#file-uploads',
  title: 'Data and files',
  component: ReleaseDataPage,
};

export const releaseAncillaryFileRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/ancillary/:fileId',
  title: 'Ancillary file',
  component: ReleaseAncillaryFilePage,
};

export const releaseDataFileRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data/:fileId',
  title: 'Data file',
  component: ReleaseDataFilePage,
};

export const releaseDataFileReplaceRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data/:fileId/replace',
  title: 'Replace data file',
  component: ReleaseDataFileReplacePage,
};

export const releaseDataFileReplacementCompleteRoute: ReleaseRouteProps = {
  path:
    '/publication/:publicationId/release/:releaseId/data/:fileId/replacement-complete',
  title: 'Replacement complete',
  component: ReleaseDataFileReplacementCompletePage,
};

export const releaseFootnotesRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/footnotes',
  title: 'Footnotes',
  component: ReleaseFootnotesPage,
};

export const releaseFootnotesCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/create-footnote',
  title: 'Create footnote',
  component: ReleaseFootnoteCreatePage,
};

export const releaseFootnotesEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/footnotes/:footnoteId',
  title: 'Edit footnote',
  component: ReleaseFootnoteEditPage,
};

export const releaseDataBlocksRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data-blocks',
  title: 'Data blocks',
  component: ReleaseDataBlocksPage,
};

export const releaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data-blocks/table-tool',
  title: 'Table tool',
  component: ReleaseTableToolPage,
};

export const releaseDataBlockCreateRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data-blocks/create',
  title: 'Create data block',
  component: ReleaseDataBlockCreatePage,
};

export const releaseDataBlockEditRoute: ReleaseRouteProps = {
  path:
    '/publication/:publicationId/release/:releaseId/data-blocks/:dataBlockId',
  title: 'Edit data block',
  component: ReleaseDataBlockEditPage,
};

export const releaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/content',
  title: 'Content',
  component: ReleaseContentPage,
};

export const releaseStatusRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/status',
  title: 'Sign off',
  component: ReleasePublishStatusPage,
};

export const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease-access',
  title: 'Pre-release access',
  component: ReleasePreReleaseAccessPage,
};

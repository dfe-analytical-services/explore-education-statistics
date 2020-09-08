import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
import ReleaseDataBlocksPage from '@admin/pages/release/datablocks/ReleaseDataBlocksPage';
import ReleasePreReleaseAccessPage from '@admin/pages/release/ReleasePreReleaseAccessPage';
import ReleasePublishStatusPage from '@admin/pages/release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/ReleaseSummaryPage';
import { RouteProps } from 'react-router';

export type ReleaseRouteParams = {
  publicationId: string;
  releaseId: string;
};

export type ReleaseDataBlocksRouteParams = ReleaseRouteParams & {
  dataBlockId?: string;
};

export interface ReleaseRouteProps extends RouteProps {
  title: string;
  path: string;
}

export const releaseSummaryRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/summary',
  title: 'Release summary',
  component: ReleaseSummaryPage,
};

export const releaseSummaryEditRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/summary/edit',
  title: 'Release summary',
  component: ReleaseSummaryEditPage,
};

export const releaseDataRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data',
  title: 'Manage data',
  component: ReleaseDataPage,
};

export const releaseDataBlocksRoute: ReleaseRouteProps = {
  path:
    '/publication/:publicationId/release/:releaseId/datablocks/:dataBlockId?',
  title: 'Manage data blocks',
  component: ReleaseDataBlocksPage,
};

export const releaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/content',
  title: 'Manage content',
  component: ReleaseContentPage,
};

export const releaseStatusRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/status',
  title: 'Release status',
  component: ReleasePublishStatusPage,
};

export const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease-access',
  title: 'Pre-release access',
  component: ReleasePreReleaseAccessPage,
};

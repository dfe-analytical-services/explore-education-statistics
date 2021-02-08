import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import {
  ReleaseRouteParams,
  ReleaseRouteProps,
} from '@admin/routes/releaseRoutes';

export type PreReleaseTableToolRouteParams = ReleaseRouteParams & {
  dataBlockId?: string;
};

export const preReleaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/content',
  title: 'Content',
  component: PreReleaseContentPage,
  exact: true,
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  path:
    '/publication/:publicationId/release/:releaseId/prerelease/table-tool/:dataBlockId?',
  title: 'Table tool',
  component: PreReleaseTableToolPage,
  exact: true,
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
];

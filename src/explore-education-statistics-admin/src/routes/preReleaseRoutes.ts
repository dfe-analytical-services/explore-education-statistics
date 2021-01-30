import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import { ReleaseRouteProps } from '@admin/routes/releaseRoutes';

export const preReleaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/content',
  title: 'Content',
  component: PreReleaseContentPage,
  exact: true,
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/table-tool',
  title: 'Table tool',
  component: PreReleaseTableToolPage,
  exact: true,
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
];

import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import { ReleaseRouteProps } from '@admin/routes/releaseRoutes';

export const preReleaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/content',
  title: 'Content',
  component: PreReleaseContentPage,
  exact: true,
};

export const preReleaseNavRoutes = [preReleaseContentRoute];

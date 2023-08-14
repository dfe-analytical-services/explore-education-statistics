import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import PreReleaseMethodologiesPage from '@admin/pages/release/pre-release/PreReleaseMethodologiesPage';
import PreReleaseMethodologyPage from '@admin/pages/release/pre-release/PreReleaseMethodologyPage';
import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import {
  ReleaseRouteParams,
  ReleaseRouteProps,
} from '@admin/routes/releaseRoutes';

export type PreReleaseTableToolRouteParams = ReleaseRouteParams & {
  dataBlockId?: string;
};

export type PreReleaseMethodologyRouteParams = ReleaseRouteParams & {
  methodologyId: string;
};

export const preReleaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/content',
  title: 'Content',
  component: PreReleaseContentPage,
  exact: true,
};

export const preReleaseMethodologiesRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/methodologies',
  title: 'Methodologies',
  component: PreReleaseMethodologiesPage,
  exact: true,
};

export const preReleaseMethodologyRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/methodologies/:methodologyId',
  title: 'Methodology',
  component: PreReleaseMethodologyPage,
  exact: true,
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease/table-tool/:dataBlockId?',
  title: 'Table tool',
  component: PreReleaseTableToolPage,
  exact: true,
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseMethodologiesRoute,
];

export const preReleaseRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseMethodologiesRoute,
  preReleaseMethodologyRoute,
];

import {
  ReleaseRouteParams,
  ReleaseRouteProps,
} from '@admin/routes/releaseRoutes';

export type PreReleaseTableToolRouteParams = ReleaseRouteParams & {
  dataBlockVersionId?: string;
};

export type PreReleaseMethodologyRouteParams = ReleaseRouteParams & {
  methodologyId: string;
};

export const preReleaseContentRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease/content',
  title: 'Content',
  exact: true,
};

export const preReleaseMethodologiesRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies',
  title: 'Methodologies',
  exact: true,
};

export const preReleaseMethodologyRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies/:methodologyId',
  title: 'Methodology',
  exact: true,
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease/table-tool/:dataBlockVersionId?',
  title: 'Table tool',
  exact: true,
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseMethodologiesRoute,
];

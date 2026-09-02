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
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/content',
  path: 'publication/:publicationId/release/:releaseVersionId/prerelease/content',
  title: 'Content',
};

export const preReleaseMethodologiesRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies',
  path: 'publication/:publicationId/release/:releaseVersionId/prerelease/methodologies',
  title: 'Methodologies',
};

export const preReleaseMethodologyRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies/:methodologyId',
  path: 'publication/:publicationId/release/:releaseVersionId/prerelease/methodologies/:methodologyId',
  title: 'Methodology',
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/prerelease/table-tool/:dataBlockVersionId?',
  path: 'publication/:publicationId/release/:releaseVersionId/prerelease/table-tool/:dataBlockVersionId?',
  title: 'Table tool',
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseMethodologiesRoute,
];

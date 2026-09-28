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
  path: 'content',
  title: 'Content',
};

export const preReleaseMethodologiesRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies',
  path: 'methodologies',
  title: 'Methodologies',
};

export const preReleaseMethodologyRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/methodologies/:methodologyId',
  path: 'methodologies/:methodologyId',
  title: 'Methodology',
};

export const preReleaseTableToolRoute: ReleaseRouteProps = {
  fullPath:
    '/publication/:publicationId/release/:releaseVersionId/prerelease/table-tool/:dataBlockVersionId?',
  path: 'table-tool/:dataBlockVersionId?',
  title: 'Table tool',
};

export const preReleaseNavRoutes = [
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseMethodologiesRoute,
];

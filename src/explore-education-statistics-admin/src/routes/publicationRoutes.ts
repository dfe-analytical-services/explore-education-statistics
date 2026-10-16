import { NavRouteProps } from '@admin/routes/types';

export type PublicationRouteParams = {
  publicationId: string;
};

export type PublicationEditReleaseSeriesLegacyLinkRouteParams = {
  publicationId: string;
  releaseSeriesItemId: string;
};

export type PublicationTeamRouteParams = {
  publicationId: string;
};

export type PublicationRouteProps = NavRouteProps;

export const publicationReleasesRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/releases',
  path: 'releases',
  title: 'Releases',
};

export const publicationMethodologiesRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/methodologies',
  path: 'methodologies',
  title: 'Methodologies',
};

export const publicationAdoptMethodologyRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/methodologies/adopt',
  path: 'methodologies/adopt',
  title: 'Adopt a methodology',
};

export const publicationExternalMethodologyRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/methodologies/external',
  path: 'methodologies/external',
  title: 'External methodology',
};

export const publicationDetailsRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/details',
  path: 'details',
  title: 'Details',
};

export const publicationContactRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/contact',
  path: 'contact',
  title: 'Contact',
};

export const publicationTeamAccessRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/team',
  path: 'team',
  title: 'Team access',
};

export const publicationReleaseSeriesRoute: PublicationRouteProps = {
  fullPath: '/publication/:publicationId/releases/order',
  path: 'releases/order',
  title: 'Release order',
};

export const publicationCreateReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    fullPath: '/publication/:publicationId/releases/legacy/create',
    path: 'releases/legacy/create',
    title: 'Create legacy release',
  };

export const publicationEditReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    fullPath:
      '/publication/:publicationId/releases/legacy/:releaseSeriesItemId/edit',
    path: 'releases/legacy/:releaseSeriesItemId/edit',
    title: 'Edit legacy release',
  };

/**
 * The routes shown in the publication nav bar. Filtered by the publication's
 * own permissions in `PublicationPageContainer`.
 */
export const publicationNavRoutes: PublicationRouteProps[] = [
  publicationReleasesRoute,
  publicationMethodologiesRoute,
  publicationDetailsRoute,
  publicationContactRoute,
  publicationTeamAccessRoute,
  publicationReleaseSeriesRoute,
];

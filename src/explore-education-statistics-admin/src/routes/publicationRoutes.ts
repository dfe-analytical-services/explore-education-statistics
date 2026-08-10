import { RouteProps } from 'react-router';

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

export interface PublicationRouteProps extends RouteProps {
  title: string;
  path: string;
}

export const publicationReleasesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/releases',
  title: 'Releases',
};

export const publicationMethodologiesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies',
  title: 'Methodologies',
};

export const publicationAdoptMethodologyRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies/adopt',
  title: 'Adopt a methodology',
};

export const publicationExternalMethodologyRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies/external',
  title: 'External methodology',
};

export const publicationDetailsRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/details',
  title: 'Details',
};

export const publicationContactRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/contact',
  title: 'Contact',
};

export const publicationTeamAccessRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/team',
  title: 'Team access',
};

export const publicationReleaseSeriesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/releases/order',
  title: 'Release order',
};

export const publicationCreateReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    path: '/publication/:publicationId/releases/legacy/create',
    title: 'Create legacy release',
  };

export const publicationEditReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    path: '/publication/:publicationId/releases/legacy/:releaseSeriesItemId/edit',
    title: 'Edit legacy release',
  };

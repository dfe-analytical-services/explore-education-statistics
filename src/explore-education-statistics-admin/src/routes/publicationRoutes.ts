import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import PublicationManageReleaseContributorsPage from '@admin/pages/publication/PublicationManageReleaseContributorsPage';
import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import PublicationAdoptMethodologyPage from '@admin/pages/publication/PublicationAdoptMethodologyPage';
import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import PublicationMethodologiesPage from '@admin/pages/publication/PublicationMethodologiesPage';
import PublicationInviteUsersPage from '@admin/pages/publication/PublicationInviteUsersPage';
import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import PublicationReleaseSeriesPage from '@admin/pages/publication/PublicationReleaseSeriesPage';
import PublicationCreateReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationCreateReleaseSeriesLegacyLinkPage';
import PublicationEditReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationEditReleaseSeriesLegacyLinkPage';
import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
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
  releaseVersionId?: string;
};

export type PublicationManageTeamRouteParams = {
  publicationId: string;
  releaseVersionId: string;
};

export interface PublicationRouteProps extends RouteProps {
  title: string;
  path: string;
}

export const publicationReleasesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/releases',
  title: 'Releases',
  component: PublicationReleasesPage,
};

export const publicationMethodologiesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies',
  title: 'Methodologies',
  component: PublicationMethodologiesPage,
};

export const publicationAdoptMethodologyRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies/adopt',
  title: 'Adopt a methodology',
  component: PublicationAdoptMethodologyPage,
};

export const publicationExternalMethodologyRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/methodologies/external',
  title: 'External methodology',
  component: PublicationExternalMethodologyPage,
};

export const publicationDetailsRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/details',
  title: 'Details',
  component: PublicationDetailsPage,
};

export const publicationContactRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/contact',
  title: 'Contact',
  component: PublicationContactPage,
};

export const publicationTeamAccessRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/team/:releaseVersionId?',
  title: 'Team access',
  component: PublicationTeamAccessPage,
};

export const publicationManageReleaseContributorsPageRoute: PublicationRouteProps =
  {
    path: '/publication/:publicationId/team/:releaseVersionId/manage-contributors',
    title: 'Add contributors',
    component: PublicationManageReleaseContributorsPage,
  };

export const publicationInviteUsersPageRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/team/:releaseVersionId/invite-users',
  title: 'Invite users',
  component: PublicationInviteUsersPage,
};

export const publicationReleaseSeriesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/releases/order',
  title: 'Release order',
  component: PublicationReleaseSeriesPage,
};

export const publicationCreateReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    path: '/publication/:publicationId/releases/legacy/create',
    title: 'Create legacy release',
    component: PublicationCreateReleaseSeriesLegacyLinkPage,
  };

export const publicationEditReleaseSeriesLegacyLinkRoute: PublicationRouteProps =
  {
    path: '/publication/:publicationId/releases/legacy/:releaseSeriesItemId/edit',
    title: 'Edit legacy release',
    component: PublicationEditReleaseSeriesLegacyLinkPage,
  };

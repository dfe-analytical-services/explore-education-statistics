import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import PublicationAdoptMethodologyPage from '@admin/pages/publication/PublicationAdoptMethodologyPage';
import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import PublicationMethodologiesPage from '@admin/pages/publication/PublicationMethodologiesPage';
import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import PublicationLegacyReleasesPage from '@admin/pages/publication/PublicationLegacyReleasesPage';
import PublicationLegacyReleaseCreatePage from '@admin/pages/publication/PublicationLegacyReleaseCreatePage';
import PublicationLegacyReleaseEditPage from '@admin/pages/publication/PublicationLegacyReleaseEditPage';
import { RouteProps } from 'react-router';

export type PublicationRouteParams = {
  publicationId: string;
};

export type PublicationEditLegacyReleaseRouteParams = {
  publicationId: string;
  legacyReleaseId: string;
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

export const publicationLegacyReleasesRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/legacy',
  title: 'Legacy releases',
  component: PublicationLegacyReleasesPage,
};

export const publicationCreateLegacyReleaseRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/legacy/create',
  title: 'Create legacy release',
  component: PublicationLegacyReleaseCreatePage,
};

export const publicationEditLegacyReleaseRoute: PublicationRouteProps = {
  path: '/publication/:publicationId/legacy/:legacyReleaseId/edit',
  title: 'Edit legacy release',
  component: PublicationLegacyReleaseEditPage,
};

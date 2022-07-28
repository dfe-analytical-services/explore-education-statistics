import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import { RouteProps } from 'react-router';

export type PublicationRouteParams = {
  publicationId: string;
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

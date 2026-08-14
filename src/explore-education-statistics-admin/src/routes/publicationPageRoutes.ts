import PublicationAdoptMethodologyPage from '@admin/pages/publication/PublicationAdoptMethodologyPage';
import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import PublicationCreateReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationCreateReleaseSeriesLegacyLinkPage';
import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import PublicationEditReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationEditReleaseSeriesLegacyLinkPage';
import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import PublicationMethodologiesPage from '@admin/pages/publication/PublicationMethodologiesPage';
import PublicationReleaseSeriesPage from '@admin/pages/publication/PublicationReleaseSeriesPage';
import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import {
  publicationAdoptMethodologyRoute,
  publicationContactRoute,
  publicationCreateReleaseSeriesLegacyLinkRoute,
  publicationDetailsRoute,
  publicationEditReleaseSeriesLegacyLinkRoute,
  publicationExternalMethodologyRoute,
  publicationMethodologiesRoute,
  publicationReleaseSeriesRoute,
  publicationReleasesRoute,
  publicationTeamAccessRoute,
} from '@admin/routes/publicationRoutes';
import { NavRouteProps } from '@admin/routes/types';

const publicationPageRoutes: NavRouteProps[] = [
  {
    ...publicationReleasesRoute,
    component: PublicationReleasesPage,
  },
  {
    ...publicationMethodologiesRoute,
    component: PublicationMethodologiesPage,
  },
  {
    ...publicationDetailsRoute,
    component: PublicationDetailsPage,
  },
  {
    ...publicationContactRoute,
    component: PublicationContactPage,
  },
  {
    ...publicationTeamAccessRoute,
    component: PublicationTeamAccessPage,
  },
  {
    ...publicationReleaseSeriesRoute,
    component: PublicationReleaseSeriesPage,
  },
  {
    ...publicationAdoptMethodologyRoute,
    component: PublicationAdoptMethodologyPage,
  },
  {
    ...publicationExternalMethodologyRoute,
    component: PublicationExternalMethodologyPage,
  },
  {
    ...publicationCreateReleaseSeriesLegacyLinkRoute,
    component: PublicationCreateReleaseSeriesLegacyLinkPage,
  },
  {
    ...publicationEditReleaseSeriesLegacyLinkRoute,
    component: PublicationEditReleaseSeriesLegacyLinkPage,
  },
];

export default publicationPageRoutes;

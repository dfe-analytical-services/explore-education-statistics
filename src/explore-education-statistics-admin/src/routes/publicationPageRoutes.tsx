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
    element: <PublicationReleasesPage />,
  },
  {
    ...publicationMethodologiesRoute,
    element: <PublicationMethodologiesPage />,
  },
  {
    ...publicationDetailsRoute,
    element: <PublicationDetailsPage />,
  },
  {
    ...publicationContactRoute,
    element: <PublicationContactPage />,
  },
  {
    ...publicationTeamAccessRoute,
    element: <PublicationTeamAccessPage />,
  },
  {
    ...publicationReleaseSeriesRoute,
    element: <PublicationReleaseSeriesPage />,
  },
  {
    ...publicationAdoptMethodologyRoute,
    element: <PublicationAdoptMethodologyPage />,
  },
  {
    ...publicationExternalMethodologyRoute,
    element: <PublicationExternalMethodologyPage />,
  },
  {
    ...publicationCreateReleaseSeriesLegacyLinkRoute,
    element: <PublicationCreateReleaseSeriesLegacyLinkPage />,
  },
  {
    ...publicationEditReleaseSeriesLegacyLinkRoute,
    element: <PublicationEditReleaseSeriesLegacyLinkPage />,
  },
];

export default publicationPageRoutes;

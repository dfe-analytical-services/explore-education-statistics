import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';

import BauImportsPage from '@admin/pages/bau/BauImportsPage';
import BoundaryDataPage from '@admin/pages/bau/BoundaryDataPage';
import BoundaryLevelEditPage from '@admin/pages/bau/BoundaryLevelEditPage';
import BoundaryDataUploadPage from '@admin/pages/bau/BoundaryDataUploadPage';
import GlossaryPage from '@admin/pages/bau/GlossaryPage';
import PageFeedbackPage from '@admin/pages/bau/PageFeedbackPage';
import ServiceAnnouncementPage from '@admin/pages/bau/ServiceAnnouncementPage';
import BauUsersPage from '@admin/pages/bau/BauUsersPage';
import UserInvitePage from '@admin/pages/users/UserInvitePage';
import InvitedUsersPage from '@admin/pages/users/InvitedUsersPage';
import PreReleaseUsersPage from '@admin/pages/users/PreReleaseUsersPage';
import ManageUserPage from '@admin/pages/users/ManageUserPage';

import { ProtectedRouteProps } from './types';
import {
  administrationBoundaryDataEditRoute,
  administrationBoundaryDataRoute,
  administrationBoundaryDataUploadRoute,
  administrationFeedbackRoute,
  administrationGlossaryRoute,
  administrationImportsRoute,
  administrationIndexRoute,
  administrationInvitedUsersRoute,
  administrationPreReleaseUsersRoute,
  administrationServiceAnnouncementRoute,
  administrationUserInviteRoute,
  administrationUserManageRoute,
  administrationUsersRoute,
} from './administrationRoutes';

const administrationPageRoutes: Record<string, ProtectedRouteProps> = {
  administrationIndexRoute: {
    ...administrationIndexRoute,
    element: <BauDashboardPage />,
  },

  administrationImportsRoute: {
    ...administrationImportsRoute,
    element: <BauImportsPage />,
  },

  administrationBoundaryDataRoute: {
    ...administrationBoundaryDataRoute,
    element: <BoundaryDataPage />,
  },

  administrationBoundaryDataEditRoute: {
    ...administrationBoundaryDataEditRoute,
    element: <BoundaryLevelEditPage />,
  },

  administrationBoundaryDataUploadRoute: {
    ...administrationBoundaryDataUploadRoute,
    element: <BoundaryDataUploadPage />,
  },

  administrationGlossaryRoute: {
    ...administrationGlossaryRoute,
    element: <GlossaryPage />,
  },

  administrationFeedbackRoute: {
    ...administrationFeedbackRoute,
    element: <PageFeedbackPage />,
  },

  administrationServiceAnnouncementRoute: {
    ...administrationServiceAnnouncementRoute,
    element: <ServiceAnnouncementPage />,
  },

  administrationUsersRoute: {
    ...administrationUsersRoute,
    element: <BauUsersPage />,
  },

  administrationUserInviteRoute: {
    ...administrationUserInviteRoute,
    element: <UserInvitePage />,
  },

  administrationInvitedUsersRoute: {
    ...administrationInvitedUsersRoute,
    element: <InvitedUsersPage />,
  },

  administrationPreReleaseUsersRoute: {
    ...administrationPreReleaseUsersRoute,
    element: <PreReleaseUsersPage />,
  },

  administrationUserManageRoute: {
    ...administrationUserManageRoute,
    element: <ManageUserPage />,
  },
};

export default administrationPageRoutes;

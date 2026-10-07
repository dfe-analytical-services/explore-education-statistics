import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';
import BauUsersPage from '@admin/pages/bau/BauUsersPage';
import BauImportsPage from '@admin/pages/bau/BauImportsPage';
import InvitedUsersPage from '@admin/pages/users/InvitedUsersPage';
import ManageUserPage from '@admin/pages/users/ManageUserPage';
import PreReleaseUsersPage from '@admin/pages/users/PreReleaseUsersPage';
import UserInvitePage from '@admin/pages/users/UserInvitePage';
import BoundaryDataPage from '@admin/pages/bau/BoundaryDataPage';
import BoundaryLevelEditPage from '@admin/pages/bau/BoundaryLevelEditPage';
import BoundaryDataUploadPage from '@admin/pages/bau/BoundaryDataUploadPage';
import GlossaryPage from '@admin/pages/bau/GlossaryPage';
import PageFeedbackPage from '@admin/pages/bau/PageFeedbackPage';
import ServiceAnnouncementPage from '@admin/pages/bau/ServiceAnnouncementPage';
import { ProtectedRouteProps } from './types';

export const administrationIndexRoute: ProtectedRouteProps = {
  fullPath: '/administration',
  path: 'administration',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationImportsRoute: ProtectedRouteProps = {
  fullPath: '/administration/imports',
  path: 'administration/imports',
  protectionAction: permissions => permissions.canAccessAllImports,
};

export const administrationBoundaryDataRoute: ProtectedRouteProps = {
  fullPath: '/administration/boundary-data',
  path: 'administration/boundary-data',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationBoundaryDataEditRoute: ProtectedRouteProps = {
  fullPath: '/administration/boundary-data/boundary-level/:id',
  path: 'administration/boundary-data/boundary-level/:id',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationGlossaryRoute: ProtectedRouteProps = {
  fullPath: '/administration/glossary',
  path: 'administration/glossary',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationBoundaryDataUploadRoute: ProtectedRouteProps = {
  fullPath: '/administration/boundary-data/upload',
  path: 'administration/boundary-data/upload',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationFeedbackRoute: ProtectedRouteProps = {
  fullPath: '/administration/feedback',
  path: 'administration/feedback',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationServiceAnnouncementRoute: ProtectedRouteProps = {
  fullPath: '/administration/service-announcement',
  path: 'administration/service-announcement',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationUsersRoute: ProtectedRouteProps = {
  fullPath: '/administration/users',
  path: 'administration/users',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationUserInviteRoute: ProtectedRouteProps = {
  fullPath: '/administration/users/invites/create',
  path: 'administration/users/invites/create',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationInvitedUsersRoute: ProtectedRouteProps = {
  fullPath: '/administration/users/invites',
  path: 'administration/users/invites',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationPreReleaseUsersRoute: ProtectedRouteProps = {
  fullPath: '/administration/users/pre-release',
  path: 'administration/users/pre-release',
  protectionAction: permissions => permissions.isBauUser,
};

export const administrationUserManageRoute: ProtectedRouteProps = {
  fullPath: '/administration/users/:userId',
  path: 'administration/users/:userId',
  protectionAction: permissions => permissions.isBauUser,
};

const administrationRoutes: Record<string, ProtectedRouteProps> = {
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

export default administrationRoutes;

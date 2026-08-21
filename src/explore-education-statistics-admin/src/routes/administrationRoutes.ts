import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
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

export const administrationIndexRoute: ProtectedRouteProps = {
  path: '/administration',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationImportsRoute: ProtectedRouteProps = {
  path: '/administration/imports',
  protectionAction: permissions => permissions.canAccessAllImports,
  exact: true,
};

export const administrationBoundaryDataRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationBoundaryDataEditRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data/boundary-level/:id',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationGlossaryRoute: ProtectedRouteProps = {
  path: '/administration/glossary',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationBoundaryDataUploadRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data/upload',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationFeedbackRoute: ProtectedRouteProps = {
  path: '/administration/feedback',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationServiceAnnouncementRoute: ProtectedRouteProps = {
  path: '/administration/service-announcement',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUsersRoute: ProtectedRouteProps = {
  path: '/administration/users',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUserInviteRoute: ProtectedRouteProps = {
  path: '/administration/users/invites/create',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationInvitedUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/invites',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationPreReleaseUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/pre-release',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUserManageRoute: ProtectedRouteProps = {
  path: '/administration/users/:userId',
  protectionAction: permissions => permissions.isBauUser,
};

const administrationRoutes = {
  administrationIndexRoute: {
    ...administrationIndexRoute,
    component: BauDashboardPage,
  },

  administrationImportsRoute: {
    ...administrationImportsRoute,
    component: BauImportsPage,
  },

  administrationBoundaryDataRoute: {
    ...administrationBoundaryDataRoute,
    component: BoundaryDataPage,
  },

  administrationBoundaryDataEditRoute: {
    ...administrationBoundaryDataEditRoute,
    component: BoundaryLevelEditPage,
  },

  administrationBoundaryDataUploadRoute: {
    ...administrationBoundaryDataUploadRoute,
    component: BoundaryDataUploadPage,
  },

  administrationGlossaryRoute: {
    ...administrationGlossaryRoute,
    component: GlossaryPage,
  },

  administrationFeedbackRoute: {
    ...administrationFeedbackRoute,
    component: PageFeedbackPage,
  },

  administrationServiceAnnouncementRoute: {
    ...administrationServiceAnnouncementRoute,
    component: ServiceAnnouncementPage,
  },

  administrationUsersRoute: {
    ...administrationUsersRoute,
    component: BauUsersPage,
  },

  administrationUserInviteRoute: {
    ...administrationUserInviteRoute,
    component: UserInvitePage,
  },

  administrationInvitedUsersRoute: {
    ...administrationInvitedUsersRoute,
    component: InvitedUsersPage,
  },

  administrationPreReleaseUsersRoute: {
    ...administrationPreReleaseUsersRoute,
    component: PreReleaseUsersPage,
  },

  administrationUserManageRoute: {
    ...administrationUserManageRoute,
    component: ManageUserPage,
  },
};

export default administrationRoutes;

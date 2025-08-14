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

export const administrationIndexRoute: ProtectedRouteProps = {
  path: '/administration',
  component: BauDashboardPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationImportsRoute: ProtectedRouteProps = {
  path: '/administration/imports',
  component: BauImportsPage,
  protectionAction: permissions => permissions.canAccessAllImports,
  exact: true,
};

export const administrationBoundaryDataRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data',
  component: BoundaryDataPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationBoundaryDataEditRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data/boundary-level/:id',
  component: BoundaryLevelEditPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationGlossaryRoute: ProtectedRouteProps = {
  path: '/administration/glossary',
  component: GlossaryPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationBoundaryDataUploadRoute: ProtectedRouteProps = {
  path: '/administration/boundary-data/upload',
  component: BoundaryDataUploadPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationFeedbackRoute: ProtectedRouteProps = {
  path: '/administration/feedback',
  component: PageFeedbackPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUsersRoute: ProtectedRouteProps = {
  path: '/administration/users',
  component: BauUsersPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUserInviteRoute: ProtectedRouteProps = {
  path: '/administration/users/invites/create',
  component: UserInvitePage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationInvitedUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/invites',
  component: InvitedUsersPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationPreReleaseUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/pre-release',
  component: PreReleaseUsersPage,
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const administrationUserManageRoute: ProtectedRouteProps = {
  path: '/administration/users/:userId',
  component: ManageUserPage,
  protectionAction: permissions => permissions.isBauUser,
};

const administrationRoutes = {
  administrationIndexRoute,
  administrationImportsRoute,
  administrationBoundaryDataRoute,
  administrationBoundaryDataEditRoute,
  administrationBoundaryDataUploadRoute,
  administrationGlossaryRoute,
  administrationFeedbackRoute,
  administrationUsersRoute,
  administrationUserInviteRoute,
  administrationInvitedUsersRoute,
  administrationPreReleaseUsersRoute,
  administrationUserManageRoute,
};

export default administrationRoutes;

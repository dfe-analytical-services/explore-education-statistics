import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';
import BauMethodologyPage from '@admin/pages/bau/BauMethodologyPage';
import BauUsersPage from '@admin/pages/bau/BauUsersPage';
import ContactsPage from '@admin/pages/contacts/ContactsPage';
import InvitedUsersPage from '@admin/pages/users/InvitedUsersPage';
import ManageUserPage from '@admin/pages/users/ManageUserPage';
import PreReleaseUsersPage from '@admin/pages/users/PreReleaseUsersPage';
import UserInvitePage from '@admin/pages/users/UserInvitePage';

export const administrationIndexRoute: ProtectedRouteProps = {
  path: '/administration',
  component: BauDashboardPage,
  protectionAction: user =>
    user.permissions.canAccessUserAdministrationPages ||
    user.permissions.canAccessMethodologyAdministrationPages,
  exact: true,
};

export const administrationMethodologyRoute: ProtectedRouteProps = {
  path: '/administration/methodology',
  component: BauMethodologyPage,
  protectionAction: user =>
    user.permissions.canAccessMethodologyAdministrationPages,
  exact: true,
};

export const administrationContactsRoute: ProtectedRouteProps = {
  path: '/administration/contacts',
  component: ContactsPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const administrationUsersRoute: ProtectedRouteProps = {
  path: '/administration/users',
  component: BauUsersPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const administrationUserInviteRoute: ProtectedRouteProps = {
  path: '/administration/users/invites/create',
  component: UserInvitePage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const administrationInvitedUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/invites',
  component: InvitedUsersPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const administrationPreReleaseUsersRoute: ProtectedRouteProps = {
  path: '/administration/users/pre-release',
  component: PreReleaseUsersPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const administrationUserManageRoute: ProtectedRouteProps = {
  path: '/administration/users/:userId',
  component: ManageUserPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
};

const administrationRoutes = {
  administrationIndexRoute,
  administrationMethodologyRoute,
  administrationContactsRoute,
  administrationUsersRoute,
  administrationUserInviteRoute,
  administrationInvitedUsersRoute,
  administrationPreReleaseUsersRoute,
  administrationUserManageRoute,
};

export default administrationRoutes;

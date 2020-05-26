import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';
import BauMethodologyPage from '@admin/pages/bau/BauMethodologyPage';
import BauUsersPage from '@admin/pages/bau/BauUsersPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import AdminDocumentationConfigureCharts from '@admin/pages/documentation/DocumentationConfigureCharts';
import AdminDocumentationCreateNewPublication from '@admin/pages/documentation/DocumentationCreateNewPublication';
import AdminDocumentationCreateNewRelease from '@admin/pages/documentation/DocumentationCreateNewRelease';
import AdminDocumentationContentDesignStandards from '@admin/pages/documentation/DocumentationDesignStandards';
import AdminDocumentationEditRelease from '@admin/pages/documentation/DocumentationEditRelease';
import AdminDocumentationGlossary from '@admin/pages/documentation/DocumentationGlossary';
import AdminDocumentationHome from '@admin/pages/documentation/DocumentationHome';
import AdminDocumentationManageContent from '@admin/pages/documentation/DocumentationManageContent';
import AdminDocumentationManageData from '@admin/pages/documentation/DocumentationManageData';
import AdminDocumentationManageDataBlocks from '@admin/pages/documentation/DocumentationManageDataBlocks';
import AdminDocumentationStyle from '@admin/pages/documentation/DocumentationStyle';
import AdminDocumentationUsingDashboard from '@admin/pages/documentation/DocumentationUsingDashboard';
import MethodologyCreatePage from '@admin/pages/methodology/MethodologyCreatePage';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import MethodologiesPage from '@admin/pages/methodology/MethodologiesPage';
import CreateReleasePage from '@admin/pages/release/create-release/CreateReleasePage';
import ManageReleasePageContainer from '@admin/pages/release/ManageReleasePageContainer';
import PreReleasePage from '@admin/pages/release/prerelease/PreReleasePage';
import ThemeTopicWrapper, {
  themeTopicPath,
} from '@admin/pages/theme/ThemeTopicWrapper';
import InvitedUsersPage from '@admin/pages/users/InvitedUsersPage';
import UserInvitePage from '@admin/pages/users/UserInvitePage';
import ManageUserPage from '@admin/pages/users/ManageUserPage';
import PreReleaseUsersPage from '@admin/pages/users/PreReleaseUsersPage';
import { User } from '@admin/services/sign-in/types';
import { Dictionary } from '@admin/types';
import { generatePath, RouteProps } from 'react-router';

interface ProtectedRouteProps extends RouteProps {
  protectedAction?: (user: User) => boolean;
}

export const generateAdminDashboardThemeTopicLink: (
  themeId: string,
  topicId: string,
) => string = (themeId, topicId) =>
  generatePath('/dashboard/:themeId/:topicId', {
    themeId,
    topicId,
  });

const appRouteList: Dictionary<ProtectedRouteProps> = {
  home: {
    path: '/',
    component: AdminDashboardPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  adminDashboard: {
    path: '/dashboard',
    component: AdminDashboardPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  adminDashboardThemeTopic: {
    path: '/dashboard/:themeId/:topicId',
    component: AdminDashboardPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
  },
  themeTopicWrapper: {
    path: themeTopicPath,
    component: ThemeTopicWrapper,
  },
  administration: {
    path: '/administration',
    component: BauDashboardPage,
    protectedAction: user =>
      user.permissions.canAccessUserAdministrationPages ||
      user.permissions.canAccessMethodologyAdministrationPages,
    exact: true,
  },
  administrationMethodology: {
    path: '/administration/methodology',
    component: BauMethodologyPage,
    protectedAction: user =>
      user.permissions.canAccessMethodologyAdministrationPages,
    exact: true,
  },
  administrationUsers: {
    path: '/administration/users',
    component: BauUsersPage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  administrationUserInvite: {
    path: '/administration/users/invites/create',
    component: UserInvitePage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  administrationInvitedUsers: {
    path: '/administration/users/invites',
    component: InvitedUsersPage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  administrationPreReleaseUsers: {
    path: '/administration/users/pre-release',
    component: PreReleaseUsersPage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  administrationUserManage: {
    path: '/administration/users/:userId',
    component: ManageUserPage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
  },
  methodologies: {
    path: '/methodologies',
    component: MethodologiesPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  methodologyCreate: {
    path: '/methodologies/create',
    component: MethodologyCreatePage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  methodology: {
    path: '/methodologies/:methodologyId',
    component: MethodologyPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
  },
  createRelease: {
    path: '/publication/:publicationId/create-release',
    component: CreateReleasePage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  prereleaseView: {
    path: '/publication/:publicationId/release/:releaseId/prerelease',
    component: PreReleasePage,
    protectedAction: user => user.permissions.canAccessPrereleasePages,
    exact: true,
  },
  manageRelease: {
    path: '/publication/:publicationId/release/:releaseId',
    protectedAction: user => user.permissions.canAccessAnalystPages,
    component: ManageReleasePageContainer,
  },
  documentation: {
    path: '/documentation',
    component: AdminDocumentationHome,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationContentStandards: {
    path: '/documentation/content-design-standards-guide',
    component: AdminDocumentationContentDesignStandards,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationGlossary: {
    path: '/documentation/glossary',
    component: AdminDocumentationGlossary,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationStyleGuide: {
    path: '/documentation/style-guide',
    component: AdminDocumentationStyle,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationUsingDashboard: {
    path: '/documentation/using-dashboard',
    component: AdminDocumentationUsingDashboard,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationCreateRelease: {
    path: '/documentation/create-new-release',
    component: AdminDocumentationCreateNewRelease,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationCreatePublication: {
    path: '/documentation/create-new-publication',
    component: AdminDocumentationCreateNewPublication,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationEditRelease: {
    path: '/documentation/edit-release',
    component: AdminDocumentationEditRelease,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationManageContent: {
    path: '/documentation/manage-content',
    component: AdminDocumentationManageContent,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationManageData: {
    path: '/documentation/manage-data',
    component: AdminDocumentationManageData,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationManageDataBlock: {
    path: '/documentation/manage-data-block',
    component: AdminDocumentationManageDataBlocks,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  documentationConfigureCharts: {
    path: '/documentation/configure-charts',
    component: AdminDocumentationConfigureCharts,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  contactUs: {
    path: '/contact-us',
    component: ContactUsPage,
    exact: true,
  },
};

export default appRouteList;

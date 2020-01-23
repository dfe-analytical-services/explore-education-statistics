import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';
import BauMethodologyPage from '@admin/pages/bau/BauMethodologyPage';
import BauUsersPage from '@admin/pages/bau/BauUsersPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import CreatePublicationPage from '@admin/pages/create-publication/CreatePublicationPage';
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
import CreateMethodologyPage from '@admin/pages/methodology/CreateMethodologyPage';
import EditMethodologyPage from '@admin/pages/methodology/EditMethodologyPage';
import ListMethodologyPages from '@admin/pages/methodology/ListMethodologyPages';
import CreateReleasePage from '@admin/pages/release/create-release/CreateReleasePage';
import ManageReleasePageContainer from '@admin/pages/release/ManageReleasePageContainer';
import PrereleasePage from '@admin/pages/release/prerelease/PrereleasePage';
import PendingInvitesPage from '@admin/pages/users/PendingInvitesPage';
import UserInvitePage from '@admin/pages/users/UserInvitePage';
import publicationRoutes from '@admin/routes/edit-publication/routes';
import { User } from '@admin/services/sign-in/types';
import { Dictionary } from '@admin/types';
import { RouteProps } from 'react-router';

interface ProtectedRouteProps extends RouteProps {
  protectedAction?: (user: User) => boolean;
}

export const generateAdminDashboardThemeTopicLink: (
  themeId: string,
  topicId: string,
) => string = (themeId, topicId) => `/dashboard/${themeId}/${topicId}`;

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
    path: '/administration/users/invite',
    component: UserInvitePage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  administrationPendingUsers: {
    path: '/administration/users/pending',
    component: PendingInvitesPage,
    protectedAction: user => user.permissions.canAccessUserAdministrationPages,
    exact: true,
  },
  methodology: {
    path: '/methodology',
    component: ListMethodologyPages,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  methodologyCreate: {
    path: '/methodology/create',
    component: CreateMethodologyPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  editMethodology: {
    path: '/methodology/:methodologyId',
    component: EditMethodologyPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  createPublication: {
    path: publicationRoutes.createPublication.route,
    component: CreatePublicationPage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  createRelease: {
    path: '/publication/:publicationId/create-release',
    component: CreateReleasePage,
    protectedAction: user => user.permissions.canAccessAnalystPages,
    exact: true,
  },
  prereleaseView: {
    path: '/publication/:publicationId/release/:releaseId/prerelease',
    component: PrereleasePage,
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
  contactUs: {
    path: '/contact-us',
    component: ContactUsPage,
    exact: true,
  },
};

export default appRouteList;

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
import permissionService from '@admin/services/permissions/service';
import { Dictionary } from '@admin/types';
import { RouteProps } from 'react-router';

interface ProtectedRouteProps extends RouteProps {
  protectedAction?: () => Promise<boolean>;
}

export const generateAdminDashboardThemeTopicLink: (
  themeId: string,
  topicId: string,
) => string = (themeId, topicId) => `/dashboard/${themeId}/${topicId}`;

const appRouteList: Dictionary<ProtectedRouteProps> = {
  home: {
    path: '/',
    component: AdminDashboardPage,
    exact: true,
  },
  adminDashboard: {
    path: '/dashboard',
    component: AdminDashboardPage,
    exact: true,
  },
  adminDashboardThemeTopic: {
    path: '/dashboard/:themeId/:topicId',
    component: AdminDashboardPage,
  },
  administration: {
    path: '/administration',
    component: BauDashboardPage,
    protectedAction: () => permissionService.canAccessAdministration(),
    exact: true,
  },
  administrationMethodology: {
    path: '/administration/methodology',
    component: BauMethodologyPage,
    protectedAction: () => permissionService.canManageAllMethodologies(),
    exact: true,
  },
  administrationUsers: {
    path: '/administration/users',
    component: BauUsersPage,
    protectedAction: () => permissionService.canManageAllUsers(),
    exact: true,
  },
  administrationUserInvite: {
    path: '/administration/users/invite',
    component: UserInvitePage,
    protectedAction: () => permissionService.canManageAllUsers(),
    exact: true,
  },
  administrationPendingUsers: {
    path: '/administration/users/pending',
    component: PendingInvitesPage,
    protectedAction: () => permissionService.canManageAllUsers(),
    exact: true,
  },
  methodology: {
    path: '/methodology',
    component: ListMethodologyPages,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  methodologyCreate: {
    path: '/methodology/create',
    component: CreateMethodologyPage,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  editMethodology: {
    path: '/methodology/:methodologyId',
    component: EditMethodologyPage,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  createPublication: {
    path: publicationRoutes.createPublication.route,
    component: CreatePublicationPage,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  createRelease: {
    path: '/publication/:publicationId/create-release',
    component: CreateReleasePage,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  prereleaseView: {
    path: '/publication/:publicationId/release/:releaseId/prerelease',
    component: PrereleasePage,
    protectedAction: () => permissionService.canAccessPrereleasePages(),
    exact: true,
  },
  manageRelease: {
    path: '/publication/:publicationId/release/:releaseId',
    protectedAction: () => permissionService.canAccessAnalystPages(),
    component: ManageReleasePageContainer,
  },
  documentation: {
    path: '/documentation',
    component: AdminDocumentationHome,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationContentStandards: {
    path: '/documentation/content-design-standards-guide',
    component: AdminDocumentationContentDesignStandards,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationGlossary: {
    path: '/documentation/glossary',
    component: AdminDocumentationGlossary,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationStyleGuide: {
    path: '/documentation/style-guide',
    component: AdminDocumentationStyle,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationUsingDashboard: {
    path: '/documentation/using-dashboard',
    component: AdminDocumentationUsingDashboard,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationCreateRelease: {
    path: '/documentation/create-new-release',
    component: AdminDocumentationCreateNewRelease,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationCreatePublication: {
    path: '/documentation/create-new-publication',
    component: AdminDocumentationCreateNewPublication,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationEditRelease: {
    path: '/documentation/edit-release',
    component: AdminDocumentationEditRelease,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationManageContent: {
    path: '/documentation/manage-content',
    component: AdminDocumentationManageContent,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationManageData: {
    path: '/documentation/manage-data',
    component: AdminDocumentationManageData,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  documentationManageDataBlock: {
    path: '/documentation/manage-data-block',
    component: AdminDocumentationManageDataBlocks,
    protectedAction: () => permissionService.canAccessAnalystPages(),
    exact: true,
  },
  contactUs: {
    path: '/contact-us',
    component: ContactUsPage,
    exact: true,
  },
};

export default appRouteList;

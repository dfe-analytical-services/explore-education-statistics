import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import BauDashboardPage from '@admin/pages/bau/BauDashboardPage';
import BauMethodologyPage from '@admin/pages/bau/BauMethodologyPage';
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
import publicationRoutes from '@admin/routes/edit-publication/routes';
import { Dictionary } from '@admin/types';
import { RouteProps } from 'react-router';

export const generateAdminDashboardThemeTopicLink: (
  themeId: string,
  topicId: string,
) => string = (themeId, topicId) => `/dashboard/${themeId}/${topicId}`;

const appRouteList: Dictionary<RouteProps> = {
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
    exact: true,
  },
  administrationMethodology: {
    path: '/administration/methodology',
    component: BauMethodologyPage,
    exact: true,
  },
  methodology: {
    path: '/methodology',
    component: ListMethodologyPages,
    exact: true,
  },
  methodologyCreate: {
    path: '/methodology/create',
    component: CreateMethodologyPage,
    exact: true,
  },
  editMethodology: {
    path: '/methodology/:methodologyId',
    component: EditMethodologyPage,
    exact: true,
  },
  createPublication: {
    path: publicationRoutes.createPublication.route,
    component: CreatePublicationPage,
    exact: true,
  },
  createRelease: {
    path: '/publication/:publicationId/create-release',
    component: CreateReleasePage,
    exact: true,
  },
  manageRelease: {
    path: '/publication/:publicationId/release/:releaseId',
    component: ManageReleasePageContainer,
  },
  documentation: {
    path: '/documentation',
    component: AdminDocumentationHome,
    exact: true,
  },
  documentationContentStandards: {
    path: '/documentation/content-design-standards-guide',
    component: AdminDocumentationContentDesignStandards,
    exact: true,
  },
  documentationGlossary: {
    path: '/documentation/glossary',
    component: AdminDocumentationGlossary,
    exact: true,
  },
  documentationStyleGuide: {
    path: '/documentation/style-guide',
    component: AdminDocumentationStyle,
    exact: true,
  },
  documentationUsingDashboard: {
    path: '/documentation/using-dashboard',
    component: AdminDocumentationUsingDashboard,
    exact: true,
  },
  documentationCreateRelease: {
    path: '/documentation/create-new-release',
    component: AdminDocumentationCreateNewRelease,
    exact: true,
  },
  documentationCreatePublication: {
    path: '/documentation/create-new-publication',
    component: AdminDocumentationCreateNewPublication,
    exact: true,
  },
  documentationEditRelease: {
    path: '/documentation/edit-release',
    component: AdminDocumentationEditRelease,
    exact: true,
  },
  documentationManageContent: {
    path: '/documentation/manage-content',
    component: AdminDocumentationManageContent,
    exact: true,
  },
  documentationManageData: {
    path: '/documentation/manage-data',
    component: AdminDocumentationManageData,
    exact: true,
  },
  documentationManageDataBlock: {
    path: '/documentation/manage-data-block',
    component: AdminDocumentationManageDataBlocks,
    exact: true,
  },
};

export default appRouteList;

import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
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

export const documentationIndexRoute: ProtectedRouteProps = {
  path: '/documentation',
  component: AdminDocumentationHome,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationContentStandardsRoute: ProtectedRouteProps = {
  path: '/documentation/content-design-standards-guide',
  component: AdminDocumentationContentDesignStandards,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationGlossaryRoute: ProtectedRouteProps = {
  path: '/documentation/glossary',
  component: AdminDocumentationGlossary,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationStyleGuideRoute: ProtectedRouteProps = {
  path: '/documentation/style-guide',
  component: AdminDocumentationStyle,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationUsingDashboardRoute: ProtectedRouteProps = {
  path: '/documentation/using-dashboard',
  component: AdminDocumentationUsingDashboard,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationCreateReleaseRoute: ProtectedRouteProps = {
  path: '/documentation/create-new-release',
  component: AdminDocumentationCreateNewRelease,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationCreatePublicationRoute: ProtectedRouteProps = {
  path: '/documentation/create-new-publication',
  component: AdminDocumentationCreateNewPublication,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationEditReleaseRoute: ProtectedRouteProps = {
  path: '/documentation/edit-release',
  component: AdminDocumentationEditRelease,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageContentRoute: ProtectedRouteProps = {
  path: '/documentation/manage-content',
  component: AdminDocumentationManageContent,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageDataRoute: ProtectedRouteProps = {
  path: '/documentation/manage-data',
  component: AdminDocumentationManageData,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageDataBlockRoute: ProtectedRouteProps = {
  path: '/documentation/manage-data-block',
  component: AdminDocumentationManageDataBlocks,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationConfigureChartsRoute: ProtectedRouteProps = {
  path: '/documentation/configure-charts',
  component: AdminDocumentationConfigureCharts,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

const documentationRoutes = {
  documentationIndexRoute,
  documentationContentStandardsRoute,
  documentationGlossaryRoute,
  documentationStyleGuideRoute,
  documentationUsingDashboardRoute,
  documentationCreateReleaseRoute,
  documentationCreatePublicationRoute,
  documentationEditReleaseRoute,
  documentationManageContentRoute,
  documentationManageDataRoute,
  documentationManageDataBlockRoute,
  documentationConfigureChartsRoute,
};

export default documentationRoutes;

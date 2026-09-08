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
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationContentStandardsRoute: ProtectedRouteProps = {
  path: '/documentation/content-design-standards-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationGlossaryRoute: ProtectedRouteProps = {
  path: '/documentation/glossary',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationStyleGuideRoute: ProtectedRouteProps = {
  path: '/documentation/style-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationUsingDashboardRoute: ProtectedRouteProps = {
  path: '/documentation/using-dashboard',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationCreateReleaseRoute: ProtectedRouteProps = {
  path: '/documentation/create-new-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationCreatePublicationRoute: ProtectedRouteProps = {
  path: '/documentation/create-new-publication',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationEditReleaseRoute: ProtectedRouteProps = {
  path: '/documentation/edit-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageContentRoute: ProtectedRouteProps = {
  path: '/documentation/manage-content',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageDataRoute: ProtectedRouteProps = {
  path: '/documentation/manage-data',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationManageDataBlockRoute: ProtectedRouteProps = {
  path: '/documentation/manage-data-block',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const documentationConfigureChartsRoute: ProtectedRouteProps = {
  path: '/documentation/configure-charts',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

const documentationRoutes = {
  documentationIndexRoute: {
    ...documentationIndexRoute,
    component: AdminDocumentationHome,
  },

  documentationContentStandardsRoute: {
    ...documentationContentStandardsRoute,
    component: AdminDocumentationContentDesignStandards,
  },

  documentationGlossaryRoute: {
    ...documentationGlossaryRoute,
    component: AdminDocumentationGlossary,
  },

  documentationStyleGuideRoute: {
    ...documentationStyleGuideRoute,
    component: AdminDocumentationStyle,
  },

  documentationUsingDashboardRoute: {
    ...documentationUsingDashboardRoute,
    component: AdminDocumentationUsingDashboard,
  },

  documentationCreateReleaseRoute: {
    ...documentationCreateReleaseRoute,
    component: AdminDocumentationCreateNewRelease,
  },

  documentationCreatePublicationRoute: {
    ...documentationCreatePublicationRoute,
    component: AdminDocumentationCreateNewPublication,
  },

  documentationEditReleaseRoute: {
    ...documentationEditReleaseRoute,
    component: AdminDocumentationEditRelease,
  },

  documentationManageContentRoute: {
    ...documentationManageContentRoute,
    component: AdminDocumentationManageContent,
  },

  documentationManageDataRoute: {
    ...documentationManageDataRoute,
    component: AdminDocumentationManageData,
  },

  documentationManageDataBlockRoute: {
    ...documentationManageDataBlockRoute,
    component: AdminDocumentationManageDataBlocks,
  },

  documentationConfigureChartsRoute: {
    ...documentationConfigureChartsRoute,
    component: AdminDocumentationConfigureCharts,
  },
};

export default documentationRoutes;

import { ProtectedRouteProps } from './types';

export const documentationIndexRoute: ProtectedRouteProps = {
  fullPath: '/documentation',
  path: 'documentation',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationContentStandardsRoute: ProtectedRouteProps = {
  fullPath: '/documentation/content-design-standards-guide',
  path: 'documentation/content-design-standards-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationGlossaryRoute: ProtectedRouteProps = {
  fullPath: '/documentation/glossary',
  path: 'documentation/glossary',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationStyleGuideRoute: ProtectedRouteProps = {
  fullPath: '/documentation/style-guide',
  path: 'documentation/style-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationUsingDashboardRoute: ProtectedRouteProps = {
  fullPath: '/documentation/using-dashboard',
  path: 'documentation/using-dashboard',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationCreateReleaseRoute: ProtectedRouteProps = {
  fullPath: '/documentation/create-new-release',
  path: 'documentation/create-new-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationCreatePublicationRoute: ProtectedRouteProps = {
  fullPath: '/documentation/create-new-publication',
  path: 'documentation/create-new-publication',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationEditReleaseRoute: ProtectedRouteProps = {
  fullPath: '/documentation/edit-release',
  path: 'documentation/edit-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationManageContentRoute: ProtectedRouteProps = {
  fullPath: '/documentation/manage-content',
  path: 'documentation/manage-content',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationManageDataRoute: ProtectedRouteProps = {
  fullPath: '/documentation/manage-data',
  path: 'documentation/manage-data',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationManageDataBlockRoute: ProtectedRouteProps = {
  fullPath: '/documentation/manage-data-block',
  path: 'documentation/manage-data-block',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const documentationConfigureChartsRoute: ProtectedRouteProps = {
  fullPath: '/documentation/configure-charts',
  path: 'documentation/configure-charts',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

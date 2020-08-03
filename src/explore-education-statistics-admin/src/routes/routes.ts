import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import LegacyReleasesPageContainer from '@admin/pages/legacy-releases/LegacyReleasesPageContainer';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import MethodologiesPage from '@admin/pages/methodology/MethodologiesPage';
import MethodologyCreatePage from '@admin/pages/methodology/MethodologyCreatePage';
import PublicationCreatePage from '@admin/pages/publication/PublicationCreatePage';
import PreReleasePage from '@admin/pages/release/PreReleasePage';
import ReleaseCreatePage from '@admin/pages/release/ReleaseCreatePage';
import ReleasePageContainer from '@admin/pages/release/ReleasePageContainer';
import administrationRoutes from '@admin/routes/administrationRoutes';
import documentationRoutes from '@admin/routes/documentationRoutes';

export type PublicationRouteParams = {
  publicationId: string;
};

export const homeRoute: ProtectedRouteProps = {
  path: '/',
  component: AdminDashboardPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const dashboardRoute: ProtectedRouteProps = {
  path: '/dashboard',
  component: AdminDashboardPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const contactUsRoute: ProtectedRouteProps = {
  path: '/contact-us',
  component: ContactUsPage,
  exact: true,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/topics/:topicId/publications/create',
  component: PublicationCreatePage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const methodologiesIndexRoute: ProtectedRouteProps = {
  path: '/methodologies',
  component: MethodologiesPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const methodologyCreateRoute: ProtectedRouteProps = {
  path: '/methodologies/create',
  component: MethodologyCreatePage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const methodologyRoute: ProtectedRouteProps = {
  path: '/methodologies/:methodologyId',
  component: MethodologyPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
};

export const releaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId',
  protectionAction: user => user.permissions.canAccessAnalystPages,
  component: ReleasePageContainer,
};

export const releaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/create-release',
  component: ReleaseCreatePage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const legacyReleasesRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases',
  component: LegacyReleasesPageContainer,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
};

export const preReleaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease',
  component: PreReleasePage,
  protectionAction: user => user.permissions.canAccessPrereleasePages,
  exact: true,
};

const routes = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute,
  dashboardRoute,
  contactUsRoute,
  publicationCreateRoute,
  methodologiesIndexRoute,
  methodologyCreateRoute,
  methodologyRoute,
  releaseRoute,
  releaseCreateRoute,
  legacyReleasesRoute,
  preReleaseRoute,
};

export default routes;

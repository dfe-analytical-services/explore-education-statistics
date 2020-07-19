import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import LegacyReleasesPageContainer from '@admin/pages/legacy-releases/LegacyReleasesPageContainer';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import MethodologiesPage from '@admin/pages/methodology/MethodologiesPage';
import MethodologyCreatePage from '@admin/pages/methodology/MethodologyCreatePage';
import PreReleasePage from '@admin/pages/release/PreReleasePage';
import ReleaseCreatePage from '@admin/pages/release/ReleaseCreatePage';
import ReleasePageContainer from '@admin/pages/release/ReleasePageContainer';
import ThemeTopicWrapper from '@admin/pages/theme/ThemeTopicWrapper';
import CreatePublicationPage from '@admin/pages/theme/topic/CreatePublicationPage';
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

export const dashboardThemeTopicRoute: ProtectedRouteProps = {
  path: '/dashboard/theme/:themeId/topic/:topicId',
  component: AdminDashboardPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
};

export const themeTopicRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/topic/:topicId',
  component: ThemeTopicWrapper,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/topic/:topicId/create-publication',
  component: CreatePublicationPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
};

export const contactUsRoute: ProtectedRouteProps = {
  path: '/contact-us',
  component: ContactUsPage,
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
  dashboardThemeTopicRoute,
  themeTopicRoute,
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

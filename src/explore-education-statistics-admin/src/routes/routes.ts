import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import AdoptMethodologyPage from '@admin/pages/methodology/adopt-methodology/AdoptMethodologyPage';
import ExternalMethodologyPage from '@admin/pages/methodology/external-methodology/ExternalMethodologyPage';
import PublicationCreatePage from '@admin/pages/publication/PublicationCreatePage';
import PublicationPageContainer from '@admin/pages/publication/PublicationPageContainer';
import PreReleaseAccessListPage from '@admin/pages/release/pre-release/PreReleaseAccessListPage';
import PreReleasePageContainer from '@admin/pages/release/pre-release/PreReleasePageContainer';
import ReleaseCreatePage from '@admin/pages/release/ReleaseCreatePage';
import ReleaseDataGuidancePage from '@admin/pages/release/ReleaseDataGuidancePage';
import ReleasePageContainer from '@admin/pages/release/ReleasePageContainer';
import ThemeCreatePage from '@admin/pages/themes/ThemeCreatePage';
import ThemeEditPage from '@admin/pages/themes/ThemeEditPage';
import ThemesPage from '@admin/pages/themes/ThemesPage';
import TopicCreatePage from '@admin/pages/themes/topics/TopicCreatePage';
import TopicEditPage from '@admin/pages/themes/topics/TopicEditPage';
import administrationRoutes from '@admin/routes/administrationRoutes';
import documentationRoutes from '@admin/routes/documentationRoutes';
import {
  preReleaseContentRoute,
  preReleaseTableToolRoute,
} from '@admin/routes/preReleaseRoutes';
import PublicationReleaseContributorsPage from '@admin/pages/publication/PublicationReleaseContributorsPage';

export type PublicationRouteParams = {
  publicationId: string;
};

export type ThemeParams = {
  themeId: string;
};

export type TopicParams = {
  topicId: string;
};

export type ThemeTopicParams = ThemeParams & TopicParams;

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

export const themesRoute: ProtectedRouteProps = {
  path: '/themes',
  component: ThemesPage,
  protectionAction: user => user.permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeCreateRoute: ProtectedRouteProps = {
  path: '/themes/create',
  component: ThemeCreatePage,
  protectionAction: user => user.permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeEditRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/edit',
  component: ThemeEditPage,
  protectionAction: user => user.permissions.canManageAllTaxonomy,
  exact: true,
};

export const topicCreateRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/topics/create',
  component: TopicCreatePage,
  protectionAction: user => user.permissions.canManageAllTaxonomy,
  exact: true,
};

export const topicEditRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/topics/:topicId/edit',
  component: TopicEditPage,
  protectionAction: user => user.permissions.canManageAllTaxonomy,
  exact: true,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/topics/:topicId/publications/create',
  component: PublicationCreatePage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const publicationRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId',
  protectionAction: user => user.permissions.canAccessAnalystPages,
  component: PublicationPageContainer,
};

export const publicationReleaseContributorsRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/add-contributors',
  component: PublicationReleaseContributorsPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const methodologyRoute: ProtectedRouteProps = {
  path: '/methodology/:methodologyId',
  component: MethodologyPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
};

export const methodologyAdoptRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/adopt-methodology',
  component: AdoptMethodologyPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
};

export const externalMethodologyEditRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/external-methodology',
  component: ExternalMethodologyPage,
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

export const preReleaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease',
  component: PreReleasePageContainer,
  protectionAction: user => user.permissions.canAccessPrereleasePages,
};

export const preReleaseAccessListRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/prerelease-access-list',
  component: PreReleaseAccessListPage,
  protectionAction: user => user.permissions.canAccessPrereleasePages,
  exact: true,
};

export const releaseDataGuidanceRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseId/data-guidance',
  component: ReleaseDataGuidancePage,
  protectionAction: user => user.permissions.canAccessPrereleasePages,
  exact: true,
};

const routes = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute,
  dashboardRoute,
  contactUsRoute,
  themesRoute,
  themeCreateRoute,
  themeEditRoute,
  topicCreateRoute,
  topicEditRoute,
  publicationCreateRoute,
  publicationReleaseContributorsRoute,
  methodologyRoute,
  methodologyAdoptRoute,
  externalMethodologyEditRoute,
  preReleaseRoute,
  preReleaseContentRoute,
  preReleaseTableToolRoute,
  preReleaseAccessListRoute,
  releaseDataGuidanceRoute,
  releaseRoute,
  releaseCreateRoute,
  publicationRoute,
};

export default routes;

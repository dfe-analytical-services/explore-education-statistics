import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
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
import administrationRoutes from '@admin/routes/administrationRoutes';
import documentationRoutes from '@admin/routes/documentationRoutes';
import {
  preReleaseContentRoute,
  preReleaseTableToolRoute,
} from '@admin/routes/preReleaseRoutes';
import SignInPage from '@admin/pages/sign-in/SignInPage';
import SignedOutPage from '@admin/pages/sign-in/SignedOutPage';
import { RouteProps } from 'react-router';
import ExpiredInvitePage from '@admin/pages/sign-in/ExpiredInvitePage';
import NoInvitationPage from '@admin/pages/sign-in/NoInvitationPage';
import PublishersGuide from '@admin/pages/publishers-guide/PublishersGuide';

interface PublicRouteProps extends RouteProps {
  path: string;
}

export type PublicationRouteParams = {
  publicationId: string;
};

export type ThemeParams = {
  themeId: string;
};

export const signInRoute: PublicRouteProps = {
  path: '/sign-in',
  component: SignInPage,
  exact: true,
};

export const signedOutRoute: PublicRouteProps = {
  path: '/signed-out',
  component: SignedOutPage,
  exact: true,
};

export const expiredInviteRoute: PublicRouteProps = {
  path: '/expired-invite',
  component: ExpiredInvitePage,
  exact: true,
};

export const noInvitationRoute: PublicRouteProps = {
  path: '/no-invitation',
  component: NoInvitationPage,
  exact: true,
};

export const homeRoute: ProtectedRouteProps = {
  path: '/',
  component: AdminDashboardPage,
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publishersGuideRoute: ProtectedRouteProps = {
  path: '/publishers-guide',
  component: PublishersGuide,
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const dashboardRoute: ProtectedRouteProps = {
  path: '/dashboard',
  component: AdminDashboardPage,
  protectionAction: permissions => permissions.canAccessAnalystPages,
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
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeCreateRoute: ProtectedRouteProps = {
  path: '/themes/create',
  component: ThemeCreatePage,
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeEditRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/edit',
  component: ThemeEditPage,
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/publications/create',
  component: PublicationCreatePage,
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publicationRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  component: PublicationPageContainer,
};

export const methodologyRoute: ProtectedRouteProps = {
  path: '/methodology/:methodologyId',
  component: MethodologyPage,
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  component: ReleasePageContainer,
};

export const releaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/create-release',
  component: ReleaseCreatePage,
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const preReleaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease',
  component: PreReleasePageContainer,
  protectionAction: permissions => permissions.canAccessPrereleasePages,
};

export const preReleaseAccessListRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease-access-list',
  component: PreReleaseAccessListPage,
  protectionAction: permissions => permissions.canAccessPrereleasePages,
  exact: true,
};

export const releaseDataGuidanceRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/data-guidance',
  component: ReleaseDataGuidancePage,
  protectionAction: permissions => permissions.canAccessPrereleasePages,
  exact: true,
};

export const publicRoutes = {
  signInRoute,
  signedOutRoute,
  expiredInviteRoute,
  noInvitationRoute,
};

const routes = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute,
  publishersGuideRoute,
  dashboardRoute,
  contactUsRoute,
  themesRoute,
  themeCreateRoute,
  themeEditRoute,
  publicationCreateRoute,
  methodologyRoute,
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

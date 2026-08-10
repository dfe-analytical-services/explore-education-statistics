import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import AdminDashboardPage from '@admin/pages/admin-dashboard/AdminDashboardPage';
import ContactUsPage from '@admin/pages/ContactUsPage';
import EducationInNumbersCreatePage from '@admin/pages/education-in-numbers/EducationInNumbersCreatePage';
import EducationInNumbersListPage from '@admin/pages/education-in-numbers/EducationInNumbersListPage';
import EducationInNumbersPage from '@admin/pages/education-in-numbers/EducationInNumbersPage';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import PublicationCreatePage from '@admin/pages/publication/PublicationCreatePage';
import PublicationPageContainer from '@admin/pages/publication/PublicationPageContainer';
import PublishersGuide from '@admin/pages/publishers-guide/PublishersGuide';
import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import PreReleasePageContainer from '@admin/pages/release/pre-release/PreReleasePageContainer';
import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import ReleaseCreatePage from '@admin/pages/release/ReleaseCreatePage';
import ReleasePageContainer from '@admin/pages/release/ReleasePageContainer';
import ExpiredInvitePage from '@admin/pages/sign-in/ExpiredInvitePage';
import NoInvitationPage from '@admin/pages/sign-in/NoInvitationPage';
import SignedOutPage from '@admin/pages/sign-in/SignedOutPage';
import SignInPage from '@admin/pages/sign-in/SignInPage';
import ThemeCreatePage from '@admin/pages/themes/ThemeCreatePage';
import ThemeEditPage from '@admin/pages/themes/ThemeEditPage';
import ThemesPage from '@admin/pages/themes/ThemesPage';
import administrationRoutes from '@admin/routes/administrationRoutes';
import documentationRoutes from '@admin/routes/documentationRoutes';
import {
  preReleaseContentRoute,
  preReleaseTableToolRoute,
} from '@admin/routes/preReleaseRoutes';
import { RouteProps } from 'react-router';

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
  exact: true,
};

export const signedOutRoute: PublicRouteProps = {
  path: '/signed-out',
  exact: true,
};

export const expiredInviteRoute: PublicRouteProps = {
  path: '/expired-invite',
  exact: true,
};

export const noInvitationRoute: PublicRouteProps = {
  path: '/no-invitation',
  exact: true,
};

export const homeRoute: ProtectedRouteProps = {
  path: '/',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publishersGuideRoute: ProtectedRouteProps = {
  path: '/publishers-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const dashboardRoute: ProtectedRouteProps = {
  path: '/dashboard',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const contactUsRoute: ProtectedRouteProps = {
  path: '/contact-us',
  exact: true,
};

export const themesRoute: ProtectedRouteProps = {
  path: '/themes',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeCreateRoute: ProtectedRouteProps = {
  path: '/themes/create',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeEditRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/edit',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/publications/create',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publicationRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const methodologyRoute: ProtectedRouteProps = {
  path: '/methodology/:methodologyId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/create-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const preReleaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease',
};

export const educationInNumbersListRoute: ProtectedRouteProps = {
  path: '/education-in-numbers',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const educationInNumbersCreateRoute: ProtectedRouteProps = {
  path: '/education-in-numbers/create',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const educationInNumbersRoute: ProtectedRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId',
  protectionAction: permissions => permissions.isBauUser,
};

export const publicRoutes = {
  signInRoute: {
    ...signInRoute,
    component: SignInPage,
  },
  signedOutRoute: {
    ...signedOutRoute,
    component: SignedOutPage,
  },
  expiredInviteRoute: {
    ...expiredInviteRoute,
    component: ExpiredInvitePage,
  },
  noInvitationRoute: {
    ...noInvitationRoute,
    component: NoInvitationPage,
  },
};

const routes = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute: {
    ...homeRoute,
    component: AdminDashboardPage,
  },
  publishersGuideRoute: {
    ...publishersGuideRoute,
    component: PublishersGuide,
  },
  dashboardRoute: {
    ...dashboardRoute,
    component: AdminDashboardPage,
  },
  contactUsRoute: {
    ...contactUsRoute,
    component: ContactUsPage,
  },
  themesRoute: {
    ...themesRoute,
    component: ThemesPage,
  },
  themeCreateRoute: {
    ...themeCreateRoute,
    component: ThemeCreatePage,
  },
  themeEditRoute: {
    ...themeEditRoute,
    component: ThemeEditPage,
  },
  publicationCreateRoute: {
    ...publicationCreateRoute,
    component: PublicationCreatePage,
  },
  methodologyRoute: {
    ...methodologyRoute,
    component: MethodologyPage,
  },
  preReleaseRoute: {
    ...preReleaseRoute,
    component: PreReleasePageContainer,
  },
  preReleaseContentRoute: {
    ...preReleaseContentRoute,
    component: PreReleaseContentPage,
  },
  preReleaseTableToolRoute: {
    ...preReleaseTableToolRoute,
    component: PreReleaseTableToolPage,
  },
  releaseRoute: {
    ...releaseRoute,
    component: ReleasePageContainer,
  },
  releaseCreateRoute: {
    ...releaseCreateRoute,
    component: ReleaseCreatePage,
  },
  publicationRoute: {
    ...publicationRoute,
    component: PublicationPageContainer,
  },
  educationInNumbersListRoute: {
    ...educationInNumbersListRoute,
    component: EducationInNumbersListPage,
  },
  educationInNumbersCreateRoute: {
    ...educationInNumbersCreateRoute,
    component: EducationInNumbersCreatePage,
  },
  educationInNumbersRoute: {
    ...educationInNumbersRoute,
    component: EducationInNumbersPage,
  },
};

export default routes;

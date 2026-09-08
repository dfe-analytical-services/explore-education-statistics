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
import {
  contactUsRoute,
  dashboardRoute,
  educationInNumbersCreateRoute,
  educationInNumbersListRoute,
  educationInNumbersRoute,
  expiredInviteRoute,
  homeRoute,
  methodologyRoute,
  noInvitationRoute,
  preReleaseRoute,
  publicationCreateRoute,
  publicationRoute,
  publishersGuideRoute,
  releaseCreateRoute,
  releaseRoute,
  signInRoute,
  signedOutRoute,
  themeCreateRoute,
  themeEditRoute,
  themesRoute,
} from '@admin/routes/routes';

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

const appRoutes: Record<string, ProtectedRouteProps> = {
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

export default appRoutes;

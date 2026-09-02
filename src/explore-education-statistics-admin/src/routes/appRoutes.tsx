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
import { RouteProps } from 'react-router';

export const publicRoutes: Record<string, RouteProps> = {
  signInRoute: {
    ...signInRoute,
    children: <SignInPage />,
  },
  signedOutRoute: {
    ...signedOutRoute,
    children: <SignedOutPage />,
  },
  expiredInviteRoute: {
    ...expiredInviteRoute,
    children: <ExpiredInvitePage />,
  },
  noInvitationRoute: {
    ...noInvitationRoute,
    children: <NoInvitationPage />,
  },
};

const appRoutes: Record<string, ProtectedRouteProps> = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute: {
    ...homeRoute,
    children: <AdminDashboardPage />,
  },
  publishersGuideRoute: {
    ...publishersGuideRoute,
    children: <PublishersGuide />,
  },
  dashboardRoute: {
    ...dashboardRoute,
    children: <AdminDashboardPage />,
  },
  contactUsRoute: {
    ...contactUsRoute,
    children: <ContactUsPage />,
  },
  themesRoute: {
    ...themesRoute,
    children: <ThemesPage />,
  },
  themeCreateRoute: {
    ...themeCreateRoute,
    children: <ThemeCreatePage />,
  },
  themeEditRoute: {
    ...themeEditRoute,
    children: <ThemeEditPage />,
  },
  publicationCreateRoute: {
    ...publicationCreateRoute,
    children: <PublicationCreatePage />,
  },
  methodologyRoute: {
    ...methodologyRoute,
    children: <MethodologyPage />,
  },
  preReleaseRoute: {
    ...preReleaseRoute,
    children: <PreReleasePageContainer />,
  },
  preReleaseContentRoute: {
    ...preReleaseContentRoute,
    children: <PreReleaseContentPage />,
  },
  preReleaseTableToolRoute: {
    ...preReleaseTableToolRoute,
    children: <PreReleaseTableToolPage />,
  },
  releaseRoute: {
    ...releaseRoute,
    children: <ReleasePageContainer />,
  },
  releaseCreateRoute: {
    ...releaseCreateRoute,
    children: <ReleaseCreatePage />,
  },
  publicationRoute: {
    ...publicationRoute,
    children: <PublicationPageContainer />,
  },
  educationInNumbersListRoute: {
    ...educationInNumbersListRoute,
    children: <EducationInNumbersListPage />,
  },
  educationInNumbersCreateRoute: {
    ...educationInNumbersCreateRoute,
    children: <EducationInNumbersCreatePage />,
  },
  educationInNumbersRoute: {
    ...educationInNumbersRoute,
    children: <EducationInNumbersPage />,
  },
};

export default appRoutes;

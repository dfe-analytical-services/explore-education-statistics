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
  signedOutRoute,
  signInRoute,
  themeCreateRoute,
  themeEditRoute,
  themesRoute,
} from '@admin/routes/routes';
import { ProtectedRouteProps, PublicRouteProps } from '@admin/routes/types';

export const publicRoutes: Record<string, PublicRouteProps> = {
  signInRoute: {
    ...signInRoute,
    element: <SignInPage />,
  },
  signedOutRoute: {
    ...signedOutRoute,
    element: <SignedOutPage />,
  },
  expiredInviteRoute: {
    ...expiredInviteRoute,
    element: <ExpiredInvitePage />,
  },
  noInvitationRoute: {
    ...noInvitationRoute,
    element: <NoInvitationPage />,
  },
};

const appRoutes: Record<string, ProtectedRouteProps> = {
  ...administrationRoutes,
  ...documentationRoutes,
  homeRoute: {
    ...homeRoute,
    element: <AdminDashboardPage />,
  },
  publishersGuideRoute: {
    ...publishersGuideRoute,
    element: <PublishersGuide />,
  },
  dashboardRoute: {
    ...dashboardRoute,
    element: <AdminDashboardPage />,
  },
  contactUsRoute: {
    ...contactUsRoute,
    element: <ContactUsPage />,
  },
  themesRoute: {
    ...themesRoute,
    element: <ThemesPage />,
  },
  themeCreateRoute: {
    ...themeCreateRoute,
    element: <ThemeCreatePage />,
  },
  themeEditRoute: {
    ...themeEditRoute,
    element: <ThemeEditPage />,
  },
  publicationCreateRoute: {
    ...publicationCreateRoute,
    element: <PublicationCreatePage />,
  },
  methodologyRoute: {
    ...methodologyRoute,

    element: <MethodologyPage />,
  },
  preReleaseRoute: {
    ...preReleaseRoute,
    element: <PreReleasePageContainer />,
  },
  preReleaseContentRoute: {
    ...preReleaseContentRoute,
    element: <PreReleaseContentPage />,
  },
  preReleaseTableToolRoute: {
    ...preReleaseTableToolRoute,
    element: <PreReleaseTableToolPage />,
  },
  releaseRoute: {
    ...releaseRoute,
    element: <ReleasePageContainer />,
  },
  releaseCreateRoute: {
    ...releaseCreateRoute,
    element: <ReleaseCreatePage />,
  },
  publicationRoute: {
    ...publicationRoute,
    element: <PublicationPageContainer />,
  },
  educationInNumbersListRoute: {
    ...educationInNumbersListRoute,
    element: <EducationInNumbersListPage />,
  },
  educationInNumbersCreateRoute: {
    ...educationInNumbersCreateRoute,
    element: <EducationInNumbersCreatePage />,
  },
  educationInNumbersRoute: {
    ...educationInNumbersRoute,
    element: <EducationInNumbersPage />,
  },
};

export default appRoutes;

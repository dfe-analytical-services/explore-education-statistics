import { ApplicationPaths } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ProtectedRoutes from '@admin/components/ProtectedRoutes';
import ListMethodologyPages from '@admin/pages/methodology/ListMethodologyPages';
import EditMethodologyPage from '@admin/pages/methodology/EditMethodologyPage';
import CreateMethodologyPage from '@admin/pages/methodology/CreateMethodologyPage';
import CreatePublicationPage from '@admin/pages/create-publication/CreatePublicationPage';
import CreateReleasePage from '@admin/pages/release/create-release/CreateReleasePage';
import ManageReleasePageContainer from '@admin/pages/release/ManageReleasePageContainer';
import SignedOutPage from '@admin/pages/sign-in/SignedOutPage';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import publicationRoutes from '@admin/routes/edit-publication/routes';
import releaseRoutes from '@admin/routes/edit-release/routes';
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import AriaLiveAnnouncer from '@common/components/AriaLiveAnnouncer';
import React from 'react';
import { Redirect, Route, Switch } from 'react-router';
import { BrowserRouter } from 'react-router-dom';

import './App.scss';

import ThemeAndTopic from '@admin/components/ThemeAndTopic';
import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';

import { LoginContext } from './components/Login';
import AdminDashboardPage from './pages/admin-dashboard/AdminDashboardPage';
import AdminDocumentationCreateNewRelease from './pages/documentation/DocumentationCreateNewRelease';

import AdminDocumentationContentDesignStandards from './pages/documentation/DocumentationDesignStandards';
import AdminDocumentationEditRelease from './pages/documentation/DocumentationEditRelease';
import AdminDocumentationGlossary from './pages/documentation/DocumentationGlossary';
import AdminDocumentationHome from './pages/documentation/DocumentationHome';
import AdminDocumentationCreateNewPublication from './pages/documentation/DocumentationCreateNewPublication';
import AdminDocumentationManageContent from './pages/documentation/DocumentationManageContent';
import AdminDocumentationManageData from './pages/documentation/DocumentationManageData';
import AdminDocumentationManageDataBlocks from './pages/documentation/DocumentationManageDataBlocks';
import AdminDocumentationStyle from './pages/documentation/DocumentationStyle';
import AdminDocumentationUsingDashboard from './pages/documentation/DocumentationUsingDashboard';
import IndexPage from './pages/IndexPage';

import BauDashboardPage from './pages/bau/BauDashboardPage';
import BauMethodologyPage from './pages/bau/BauMethodologyPage';

import PrototypeAdminDashboard from './pages/prototypes/PrototypeAdminDashboard';
import PrototypeChartTest from './pages/prototypes/PrototypeChartTest';
import PublicationAssignMethodology from './pages/prototypes/PrototypePublicationPageAssignMethodology';
import PublicationConfirmNew from './pages/prototypes/PrototypePublicationPageConfirmNew';

import PublicationCreateNew from './pages/prototypes/PrototypePublicationPageCreateNew';

import PublicationEditPage from './pages/prototypes/PrototypePublicationPageEditAbsence';
import MethodologyEditPage from './pages/prototypes/PrototypeMethodologyEdit';
import PublicationEditUnresolvedComments from './pages/prototypes/PrototypePublicationPageEditAbsenceUnresolvedComments';
import PublicationEditNew from './pages/prototypes/PrototypePublicationPageEditNew';
import PublicationCreateNewAbsenceConfig from './pages/prototypes/PrototypePublicationPageNewAbsenceConfig';
import PublicationCreateNewAbsenceConfigEdit from './pages/prototypes/PrototypePublicationPageNewAbsenceConfigEdit';
import PublicationCreateNewAbsenceData from './pages/prototypes/PrototypePublicationPageNewAbsenceData';
import PublicationCreateNewAbsenceSchedule from './pages/prototypes/PrototypePublicationPageNewAbsenceSchedule';
import PublicationCreateNewAbsenceScheduleEdit from './pages/prototypes/PrototypePublicationPageNewAbsenceScheduleEdit';
import PublicationCreateNewAbsenceStatus from './pages/prototypes/PrototypePublicationPageNewAbsenceStatus';
import PublicationCreateNewAbsenceTable from './pages/prototypes/PrototypePublicationPageNewAbsenceTable';
import PublicationCreateNewAbsenceViewTables from './pages/prototypes/PrototypePublicationPageNewAbsenceViewTables';
import PublicationReviewPage from './pages/prototypes/PrototypePublicationPageReviewAbsence';
import ReleaseCreateNew from './pages/prototypes/PrototypeReleasePageCreateNew';
import MethodologyCreateNew from './pages/prototypes/PrototypeMethodologyPageCreateNew';
import MethodologyCreateNewConfig from './pages/prototypes/PrototypeMethodologyConfig';
import MethodologyCreateNewStatus from './pages/prototypes/PrototypeMethodologyStatus';
import PrototypesIndexPage from './pages/prototypes/PrototypesIndexPage';
import PrototypeTableTool from './pages/prototypes/PrototypeTableTool';

function App() {
  return (
    <BrowserRouter>
      {/* Non-Prototype Routes*/}
      <AriaLiveAnnouncer>
        <ThemeAndTopic>
          <ApiAuthorizationRoutes />

          <ProtectedRoutes>
            <Switch>
              <ProtectedRoute
                exact
                path={dashboardRoutes.adminDashboard}
                component={AdminDashboardPage}
              />
              <ProtectedRoute
                path={dashboardRoutes.adminDashboardThemeTopic}
                component={AdminDashboardPage}
              />

              <Redirect exact strict from="/" to="/dashboard" />
            </Switch>

            <ProtectedRoute
              exact
              path="/administration"
              component={BauDashboardPage}
            />

            <ProtectedRoute
              exact
              path="/administration/methodology"
              component={BauMethodologyPage}
            />

            <ProtectedRoute
              exact
              path={ApplicationPaths.LoggedOut}
              component={SignedOutPage}
              redirectIfNotLoggedIn={false}
            />
            <ProtectedRoute
              exact
              path="/methodology"
              component={ListMethodologyPages}
            />
            <ProtectedRoute
              exact
              path="/methodology/create"
              component={CreateMethodologyPage}
            />
            <ProtectedRoute
              exact
              path="/methodology/:methodologyId"
              component={EditMethodologyPage}
            />
            <ProtectedRoute
              exact
              path={publicationRoutes.createPublication.route}
              component={CreatePublicationPage}
            />
            <ProtectedRoute
              exact
              path={releaseRoutes.createReleaseRoute.route}
              component={CreateReleasePage}
            />
            <ProtectedRoute
              path="/publication/:publicationId/release/:releaseId"
              component={ManageReleasePageContainer}
            />
            <ProtectedRoute
              exact
              path="/documentation/"
              component={AdminDocumentationHome}
            />
            <ProtectedRoute
              exact
              path="/documentation/content-design-standards-guide"
              component={AdminDocumentationContentDesignStandards}
            />
            <ProtectedRoute
              exact
              path="/documentation/glossary"
              component={AdminDocumentationGlossary}
            />
            <ProtectedRoute
              exact
              path="/documentation/style-guide"
              component={AdminDocumentationStyle}
            />
            <ProtectedRoute
              exact
              path="/documentation/using-dashboard"
              component={AdminDocumentationUsingDashboard}
            />
            <ProtectedRoute
              exact
              path="/documentation/create-new-release"
              component={AdminDocumentationCreateNewRelease}
            />
            <ProtectedRoute
              exact
              path="/documentation/create-new-publication"
              component={AdminDocumentationCreateNewPublication}
            />
            <ProtectedRoute
              exact
              path="/documentation/edit-release"
              component={AdminDocumentationEditRelease}
            />
            <ProtectedRoute
              exact
              path="/documentation/manage-content"
              component={AdminDocumentationManageContent}
            />
            <ProtectedRoute
              exact
              path="/documentation/manage-data"
              component={AdminDocumentationManageData}
            />
            <ProtectedRoute
              exact
              path="/documentation/manage-data-block"
              component={AdminDocumentationManageDataBlocks}
            />
          </ProtectedRoutes>
        </ThemeAndTopic>
        {/* Prototype Routes */}
        <Route exact path="/index" component={IndexPage} />

        <LoginContext.Provider value={PrototypeLoginService.login()}>
          <Route exact path="/prototypes/" component={PrototypesIndexPage} />

          <Route
            exact
            path="/prototypes/admin-dashboard"
            component={PrototypeAdminDashboard}
          />

          <Route
            exact
            path="/prototypes/charts"
            component={PrototypeChartTest}
          />
          <Route
            exact
            path="/prototypes/table-tool"
            component={PrototypeTableTool}
          />

          <Route
            exact
            path="/prototypes/publication-edit"
            component={PublicationEditPage}
          />
          <Route
            exact
            path="/prototypes/methodology-edit"
            component={MethodologyEditPage}
          />
          <Route
            exact
            path="/prototypes/publication-unresolved-comments"
            component={PublicationEditUnresolvedComments}
          />
          <Route
            exact
            path="/prototypes/publication-review"
            render={() => <PublicationEditUnresolvedComments reviewing />}
          />
          <Route
            exact
            path="/prototypes/publication-higher-review"
            component={PublicationReviewPage}
          />
          <Route
            exact
            path="/prototypes/publication-preview"
            component={PublicationReviewPage}
            render={() => <PublicationReviewPage />}
          />
          <Route
            exact
            path="/prototypes/publication-create-new"
            component={PublicationCreateNew}
          />
          <Route
            exact
            path="/prototypes/publication-assign-methodology"
            component={PublicationAssignMethodology}
          />
          <Route
            exact
            path="/prototypes/publication-confirm-new"
            component={PublicationConfirmNew}
          />
          <Route
            exact
            path="/prototypes/publication-edit-new"
            component={PublicationEditNew}
          />
          <Route
            exact
            path="/prototypes/release-create-new"
            component={ReleaseCreateNew}
          />
          <Route
            exact
            path="/prototypes/methodology-create-new"
            component={MethodologyCreateNew}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence"
            render={() => <PublicationEditUnresolvedComments newBlankRelease />}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-config"
            component={PublicationCreateNewAbsenceConfig}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-methodology-config"
            component={MethodologyCreateNewConfig}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-config-edit"
            component={PublicationCreateNewAbsenceConfigEdit}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-data"
            component={PublicationCreateNewAbsenceData}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-table"
            component={PublicationCreateNewAbsenceTable}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-view-table"
            component={PublicationCreateNewAbsenceViewTables}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-schedule"
            component={PublicationCreateNewAbsenceSchedule}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-schedule-edit"
            component={PublicationCreateNewAbsenceScheduleEdit}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-absence-status"
            component={PublicationCreateNewAbsenceStatus}
          />
          <Route
            exact
            path="/prototypes/publication-create-new-methodology-status"
            component={MethodologyCreateNewStatus}
          />
        </LoginContext.Provider>
      </AriaLiveAnnouncer>
    </BrowserRouter>
  );
}

export default App;

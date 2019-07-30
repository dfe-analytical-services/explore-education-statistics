import ProtectedRoute from "@admin/components/ProtectedRoute";
import SignedOutPage from "@admin/pages/sign-in/SignedOutPage";
import SignInPage from '@admin/pages/sign-in/SignInPage';
import releaseRoutes from '@admin/routes/releaseRoutes';
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import React from 'react';
import {Route} from 'react-router';
import {BrowserRouter} from 'react-router-dom';

import './App.scss';

import {LoginContext} from './components/Login';
import AdminDashboardPage from './pages/admin-dashboard/AdminDashboardPage';
import IndexPage from './pages/IndexPage';
import PrototypeAdminDashboard from './pages/prototypes/PrototypeAdminDashboard';

import PrototypeChartTest from './pages/prototypes/PrototypeChartTest';
import AdminDocumentationGlossary from './pages/prototypes/PrototypeDocumentationGlossary';
import AdminDocumentationHome from './pages/prototypes/PrototypeDocumentationHome';
import PublicationAssignMethodology from './pages/prototypes/PrototypePublicationPageAssignMethodology';
import PublicationConfirmNew from './pages/prototypes/PrototypePublicationPageConfirmNew';

import PublicationCreateNew from './pages/prototypes/PrototypePublicationPageCreateNew';

import PublicationEditPage from './pages/prototypes/PrototypePublicationPageEditAbsence';
import PublicationEditNew from './pages/prototypes/PrototypePublicationPageEditNew';
import PublicationCreateNewAbsence from './pages/prototypes/PrototypePublicationPageNewAbsence';
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
import PrototypesIndexPage from './pages/prototypes/PrototypesIndexPage';

function App() {

  return (
    <BrowserRouter>
      <Route exact path="/" component={IndexPage} />

      <LoginContext.Provider value={PrototypeLoginService.login()}>
        {/* Prototype Routes*/}
        <Route exact path="/prototypes/" component={PrototypesIndexPage} />

        <Route
          exact
          path="/prototypes/admin-dashboard"
          component={PrototypeAdminDashboard}
        />

        <Route exact path="/prototypes/charts" component={PrototypeChartTest} />

        <Route
          exact
          path="/prototypes/publication-edit"
          component={PublicationEditPage}
        />
        <Route
          exact
          path="/prototypes/publication-review"
          component={PublicationReviewPage}
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
          path="/prototypes/publication-create-new-absence"
          component={PublicationCreateNewAbsence}
        />
        <Route
          exact
          path="/prototypes/publication-create-new-absence-config"
          component={PublicationCreateNewAbsenceConfig}
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
          path="/prototypes/documentation/"
          component={AdminDocumentationHome}
        />
        <Route
          exact
          path="/prototypes/documentation/glossary"
          component={AdminDocumentationGlossary}
        />
        <Route
          exact
          path="/prototypes/documentation/style-guide"
          component={AdminDocumentationGlossary}
        />
      </LoginContext.Provider>

      {/* Non-Prototype Routes*/}
      <Route exact path="/sign-in" component={SignInPage} />

      <Route exact path="/signed-out" component={SignedOutPage} />

      <ProtectedRoute exact path="/admin-dashboard" component={AdminDashboardPage} />

      {releaseRoutes.map(route => (
        <ProtectedRoute
          exact
          key={route.path}
          path={route.path}
          component={route.component}
        />
      ))}

    </BrowserRouter>
  );
}

export default App;

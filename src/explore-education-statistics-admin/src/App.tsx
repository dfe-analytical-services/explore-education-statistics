import React from 'react';
import { Route } from 'react-router';

import './App.scss';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';
import { BrowserRouter } from 'react-router-dom';
import AdminDashboardPage from './pages/AdminDashboard';
import AdminDocumentationGlossary from './pages/prototypes/PrototypeDocumentationGlossary';
import AdminDocumentationHome from './pages/prototypes/PrototypeDocumentationHome';

import PublicationCreateNew from './pages/prototypes/PrototypePublicationPageCreateNew';
import PublicationAssignMethodology from './pages/prototypes/PrototypePublicationPageAssignMethodology';
import PublicationConfirmNew from './pages/prototypes/PrototypePublicationPageConfirmNew';
import PublicationEditNew from './pages/prototypes/PrototypePublicationPageEditNew';
import ReleaseCreateNew from './pages/prototypes/PrototypeReleasePageCreateNew';

import PublicationEditPage from './pages/prototypes/PrototypePublicationPageEditAbsence';
import PublicationReviewPage from './pages/prototypes/PrototypePublicationPageReviewAbsence';
import PublicationCreateNewAbsence from './pages/prototypes/PrototypePublicationPageNewAbsence';
import PublicationCreateNewAbsenceConfig from './pages/prototypes/PrototypePublicationPageNewAbsenceConfig';
import PublicationCreateNewAbsenceConfigEdit from './pages/prototypes/PrototypePublicationPageNewAbsenceConfigEdit';
import PublicationCreateNewAbsenceData from './pages/prototypes/PrototypePublicationPageNewAbsenceData';
import PublicationCreateNewAbsenceTable from './pages/prototypes/PrototypePublicationPageNewAbsenceTable';
import PublicationCreateNewAbsenceViewTables from './pages/prototypes/PrototypePublicationPageNewAbsenceViewTables';
import PublicationCreateNewAbsenceSchedule from './pages/prototypes/PrototypePublicationPageNewAbsenceSchedule';
import PublicationCreateNewAbsenceScheduleEdit from './pages/prototypes/PrototypePublicationPageNewAbsenceScheduleEdit';
import PublicationCreateNewAbsenceStatus from './pages/prototypes/PrototypePublicationPageNewAbsenceStatus';
import PrototypesIndexPage from './pages/prototypes/PrototypesIndexPage';
import IndexPage from './pages/IndexPage';

import { LoginContext } from './components/Login';

function App() {
  return (
    <BrowserRouter>
      <Route exact path="/" component={IndexPage} />

      <LoginContext.Provider
        value={PrototypeLoginService.getAuthentication('user1')}
      >
        {/* Non-Prototype Routes*/}
        <Route exact path="/admin-dashboard" component={AdminDashboardPage} />

        {/* Prototype Routes*/}
        <Route exact path="/prototypes/" component={PrototypesIndexPage} />

        <Route
          exact
          path="/prototypes/admin-dashboard"
          component={AdminDashboardPage}
        />

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
    </BrowserRouter>
  );
}

export default App;

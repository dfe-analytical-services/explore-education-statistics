import { createBrowserHistory } from 'history';
import React, { Component } from 'react';
import { Route, Router } from 'react-router';

import './App.scss';
import { default as AdminDashboardPage } from './pages/prototypes/PrototypeAdminDashboard';
import { default as AdminDocumentationGlossary } from './pages/prototypes/PrototypeDocumentationGlossary';
import { default as AdminDocumentationHome } from './pages/prototypes/PrototypeDocumentationHome';

import { default as PublicationCreateNew } from './pages/prototypes/PrototypePublicationPageCreateNew';

import { default as PublicationEditPage } from './pages/prototypes/PrototypePublicationPageEditAbsence';
import { default as PublicationCreateNewAbsence } from './pages/prototypes/PrototypePublicationPageNewAbsence';
import { default as PublicationCreateNewAbsenceConfig } from './pages/prototypes/PrototypePublicationPageNewAbsenceConfig';
import { default as PublicationCreateNewAbsenceConfigEdit } from './pages/prototypes/PrototypePublicationPageNewAbsenceConfigEdit';
import { default as PublicationCreateNewAbsenceData } from './pages/prototypes/PrototypePublicationPageNewAbsenceData';
import { default as PublicationCreateNewAbsenceSchedule } from './pages/prototypes/PrototypePublicationPageNewAbsenceSchedule';
import { default as PublicationCreateNewAbsenceScheduleEdit } from './pages/prototypes/PrototypePublicationPageNewAbsenceScheduleEdit';
import { default as PublicationCreateNewAbsenceStatus } from './pages/prototypes/PrototypePublicationPageNewAbsenceStatus';
import { default as PrototypesIndexPage } from './pages/prototypes/PrototypesIndexPage';

const history = createBrowserHistory();

import { LoginContext } from './components/Login';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';

class App extends Component {
  public render() {
    return (
      <Router history={history}>
        <Route exact path="/" component={PrototypesIndexPage} />

        <LoginContext.Provider value={PrototypeLoginService.getUser('user1')}>
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
            path="/prototypes/publication-create-new"
            component={PublicationCreateNew}
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
      </Router>
    );
  }
}

export default App;

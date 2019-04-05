import React, { Component } from "react";
import { createBrowserHistory } from "history";

import logo from "./logo.svg";

import "./App.scss";
import { Route, Router } from "react-router";

import { default as PublicationPage } from "./pages/prototypes/publication";
import { default as PublicationEditPage } from "./pages/prototypes/publication-edit";
import { default as PrototypesIndexPage } from "./pages/prototypes/index";
import { default as BrowseReleasesPage } from "./pages/prototypes/browse-releases";
import { default as StartPage } from "./pages/prototypes/start";
import { default as AdminDashboardPage } from "./pages/prototypes/admin-dashboard";
import { default as PublicationCreateNew } from "./pages/prototypes/publication-create-new";
import { default as PublicationCreateNewAbsence } from "./pages/prototypes/publication-create-new-absence";
import { default as PublicationCreateNewAbsenceConfig } from "./pages/prototypes/publication-create-new-absence-config";
import { default as PublicationCreateNewAbsenceData } from "./pages/prototypes/publication-create-new-absence-data";

const history = createBrowserHistory();

class App extends Component {
  render() {
    return (
      <Router history={history}>
        <Route exact path="/prototypes/" component={PrototypesIndexPage} />
        <Route exact path="/prototypes/start" component={StartPage} />
        <Route
          exact
          path="/prototypes/browse-releases"
          component={BrowseReleasesPage}
        />
        <Route
          exact
          path="/prototypes/publication"
          component={PublicationPage}
        />
        <Route
          exact
          path="/prototypes/publication-edit"
          component={PublicationEditPage}
        />
        <Route exact path="/prototypes/tester" component={BrowseReleasesPage} />
        <Route
          exact
          path="/prototypes/admin-dashboard"
          component={AdminDashboardPage}
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
          path="/prototypes/publication-create-new-absence-data"
          component={PublicationCreateNewAbsenceData}
        />
      </Router>
    );
  }
}

export default App;

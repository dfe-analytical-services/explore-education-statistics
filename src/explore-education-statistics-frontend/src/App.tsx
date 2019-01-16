import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import './App.scss';
import Breadcrumbs from './components/Breadcrumbs';
import PageBanner from './components/PageBanner';
import PageFooter from './components/PageFooter';
import PageHeader from './components/PageHeader';
import AlphaFeedbackPage from './pages/AlphaFeedbackPage';
import CookiesPage from './pages/CookiesPage';
import FeedbackPage from './pages/FeedbackPage';
import HomePage from './pages/HomePage';
import LocalAuthorityPage from './pages/local-authority/LocalAuthorityPage';
import NotFoundPage from './pages/NotFoundPage';
import PrivacyPage from './pages/PrivacyPage';
import PublicationPage from './pages/PublicationPage';
import PublicationsPage from './pages/PublicationsPage';
import ThemePage from './pages/ThemePage';
import ThemesPage from './pages/ThemesPage';
import TopicPage from './pages/TopicPage';
import TopicsPage from './pages/TopicsPage';
import { PrototypeRoutes } from './prototypes/PrototypeHomePage';

const AppRoutes = () => (
  <Switch>
    <Route exact path="/" component={HomePage} />
    <Route exact path="/cookies" component={CookiesPage} />
    <Route exact path="/privacy-policy" component={PrivacyPage} />
    <Route exact path="/alpha-feedback" component={AlphaFeedbackPage} />
    <Route exact path="/feedback" component={FeedbackPage} />

    <Route
      exact
      path="/local-authorities/:localAuthority"
      component={LocalAuthorityPage}
    />

    <Route exact path="/themes" component={ThemesPage} />
    <Route exact path="/themes/:theme" component={ThemePage} />
    <Route exact path="/themes/:theme/:topic" component={TopicPage} />
    <Route
      exact
      path="/themes/:theme/:topic/:publication"
      component={PublicationPage}
    />
    <Route
      exact
      path="/themes/:theme/:topic/:publication/:release"
      component={PublicationPage}
    />

    <Route exact path="/topics" component={TopicsPage} />
    <Route exact path="/topics/:topic" component={TopicPage} />
    <Route
      exact
      path="/topics/:topic/:publication"
      component={PublicationPage}
    />
    <Route
      exact
      path="/topics/:topic/:publication/:release"
      component={PublicationPage}
    />

    <Route exact path="/publications/" component={PublicationsPage} />
    <Route
      exact
      path="/publications/:publication"
      component={PublicationPage}
    />
    <Route
      exact
      path="/publications/:publication/:release"
      component={PublicationPage}
    />

    <Route component={NotFoundPage} />
  </Switch>
);

class App extends Component {
  public render() {
    return (
      <Router>
        <>
          <Switch>
            <Route path="/prototypes">
              <PrototypeRoutes />
            </Route>

            <Route path="/">
              <>
                <PageHeader />

                <div className="govuk-width-container">
                  <PageBanner />
                  <Breadcrumbs />

                  <main
                    className="govuk-main-wrapper app-main-class"
                    id="main-content"
                    role="main"
                  >
                    <AppRoutes />
                  </main>
                </div>

                <PageFooter />
              </>
            </Route>
          </Switch>
        </>
      </Router>
    );
  }
}

export default App;

import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import './App.scss';
import Breadcrumbs from './components/Breadcrumbs';
import PageBanner from './components/PageBanner';
import PageFooter from './components/PageFooter';
import PageHeader from './components/PageHeader';
import ScrollToTop from './components/ScrollToTop';
import AlphaFeedbackPage from './pages/AlphaFeedbackPage';
import CookiesPage from './pages/CookiesPage';
import FeedbackPage from './pages/FeedbackPage';
import FindStatisticsPage from './pages/find-statistics/FindStatisticsPage';
import HomePage from './pages/HomePage';
import LocalAuthorityPage from './pages/local-authority/LocalAuthorityPage';
import NotFoundPage from './pages/NotFoundPage';
import PrivacyPage from './pages/PrivacyPage';
import PublicationPage from './pages/publication/PublicationPage';
import { PrototypeRoutes } from './prototypes/PrototypeIndexPage';

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

    <Route
      exact
      path="/find-statistics-and-data"
      component={FindStatisticsPage}
    />
    <Route
      exact
      path="/find-statistics-and-data/:publication"
      component={PublicationPage}
    />
    <Route
      exact
      path="/find-statistics-and-data/:publication/:release"
      component={PublicationPage}
    />
    <Route component={NotFoundPage} />
  </Switch>
);

class App extends Component {
  public render() {
    return (
      <Router>
        <ScrollToTop>
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
        </ScrollToTop>
      </Router>
    );
  }
}

export default App;

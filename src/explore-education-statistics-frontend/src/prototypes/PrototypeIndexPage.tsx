import React from 'react';
import { Route } from 'react-router-dom';
import Breadcrumbs from '../components/Breadcrumbs';
import Link from '../components/Link';
import PageHeader from '../components/PageHeader';
import PrototypePageBanner from './components/PrototypePageBanner';
import PrototypeLocalAuthorityDataTable from './local-authority/PrototypeLocalAuthorityDataTable';
import PublicationPage from './PrototypePublicationPage';
import StartPage from './PrototypeStartPage';
import ThemePage from './PrototypeThemePage';
import TopicPage from './PrototypeTopicPage';

export const PrototypeRoutes = () => (
  <>
    <Route exact path="/prototypes" component={PrototypeIndexPage} />
    <Route exact path="/prototypes/start" component={StartPage} />
    <Route exact path="/prototypes/publication" component={PublicationPage} />
    <Route exact path="/prototypes/theme" component={ThemePage} />
    <Route exact path="/prototypes/topic" component={TopicPage} />
    <Route
      exact
      path="/prototypes/local-authority/data-table"
      component={PrototypeLocalAuthorityDataTable}
    />
  </>
);

export const PrototypeIndexPage = () => {
  return (
    <>
      <PageHeader />

      <div className="govuk-width-container">
        <PrototypePageBanner />

        <Breadcrumbs />

        <main
          className="app-main-class govuk-main-wrapper"
          id="main-content"
          role="main"
        >
          <h1>Prototypes index page</h1>

          <ul>
            <li>
              <Link to="/prototypes/start">Start page</Link>
            </li>
            <li>
              <Link to="/prototypes/topic">Topic page</Link>
            </li>
            <li>
              <Link to="/prototypes/theme">Theme page</Link>
            </li>
            <li>
              <Link to="/prototypes/publication">Publication page</Link>
            </li>
            <li>
              <Link to="/prototypes/local-authority/data-table">
                Local authority data table
              </Link>
            </li>
          </ul>
        </main>
      </div>
    </>
  );
};

import React from 'react';
import { Route } from 'react-router-dom';
import Breadcrumbs from '../components/Breadcrumbs';
import Link from '../components/Link';
import PageHeader from '../components/PageHeader';
import PrototypePageBanner from './components/PrototypePageBanner';
import PrototypeDataTableLocalAuthority from './data-table/PrototypeDataTableLocalAuthority';
import PrototypeDataTableNational from './data-table/PrototypeDataTableNational';
import HomePage from './PrototypeHomePage';
import HomePageV2 from './PrototypeHomePageV2';
import PublicationPage from './PrototypePublicationPage';
import StartPage from './PrototypeStartPage';
import ThemePage from './PrototypeThemePage';
import TopicPage from './PrototypeTopicPage';

export const PrototypeRoutes = () => (
  <>
    <Route exact path="/prototypes" component={PrototypeIndexPage} />
    <Route exact path="/prototypes/start" component={StartPage} />
    <Route exact path="/prototypes/home" component={HomePage} />
    <Route exact path="/prototypes/home-v2" component={HomePageV2} />
    <Route exact path="/prototypes/publication" component={PublicationPage} />
    <Route exact path="/prototypes/theme" component={ThemePage} />
    <Route exact path="/prototypes/topic" component={TopicPage} />
    <Route
      exact
      path="/prototypes/data-table/national"
      component={PrototypeDataTableNational}
    />
    <Route
      exact
      path="/prototypes/data-table/local-authority"
      component={PrototypeDataTableLocalAuthority}
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
              <Link to="/prototypes/home">Home page</Link>
            </li>
            <li>
              <Link to="/prototypes/home-v2">
                Home page - alternative headings
              </Link>
            </li>
            <li>
              <Link to="/prototypes/theme">Theme page</Link>
            </li>
            <li>
              <Link to="/prototypes/topic">Topic page</Link>
            </li>
            <li>
              <Link to="/prototypes/publication">Publication page</Link>
            </li>
          </ul>

          <h2>Data table</h2>

          <ul>
            <li>
              <Link to="/prototypes/data-table/local-authority">
                Data table - local authority
              </Link>
            </li>
            <li>
              <Link to="/prototypes/data-table/national">
                Data table - national level
              </Link>
            </li>
          </ul>
        </main>
      </div>
    </>
  );
};

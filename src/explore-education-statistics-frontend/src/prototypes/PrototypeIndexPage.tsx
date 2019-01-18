import React from 'react';
import { Route } from 'react-router-dom';
import Breadcrumbs from '../components/Breadcrumbs';
import Link from '../components/Link';
import PageHeader from '../components/PageHeader';
import PrototypePageBanner from './components/PrototypePageBanner';
import PrototypeDataTableV1LocalAuthority from './data-table/PrototypeDataTableV1LocalAuthority';
import PrototypeDataTableV1National from './data-table/PrototypeDataTableV1National';
import PrototypeDataTableV1VerticalLayout from './data-table/PrototypeDataTableV1VerticalLayout';
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
      component={PrototypeDataTableV1National}
    />
    <Route
      exact
      path="/prototypes/data-table/local-authority"
      component={PrototypeDataTableV1LocalAuthority}
    />
    <Route
      exact
      path="/prototypes/data-table/vertical-layout"
      component={PrototypeDataTableV1VerticalLayout}
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
                Data table v1 - local authority
              </Link>
            </li>
            <li>
              <Link to="/prototypes/data-table/national">
                Data table v1 - national level
              </Link>
            </li>
            <li>
              <Link to="/prototypes/data-table/vertical-layout">
                Data table v1 - vertical layout
              </Link>
            </li>
          </ul>
        </main>
      </div>
    </>
  );
};

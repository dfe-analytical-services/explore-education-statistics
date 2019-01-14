import React from 'react';
import { Route } from 'react-router-dom';
import Breadcrumbs from '../components/Breadcrumbs';
import Link from '../components/Link';
import PublicationPage from './PrototypesPublicationPage';

export const PrototypeRoutes = () => (
  <>
    <Route exact path="/prototypes" component={PrototypesHomePage} />
    <Route exact path="/prototypes/publication" component={PublicationPage} />
  </>
);

export const PrototypesHomePage = () => {
  return (
    <>
      <Breadcrumbs />

      <main
        className="app-main-class govuk-main-wrapper"
        id="main-content"
        role="main"
      >
        <h1>Prototypes</h1>

        <ul>
          <li>
            <Link to="/prototypes/publication">Publication Page</Link>
          </li>
        </ul>
      </main>
    </>
  );
};

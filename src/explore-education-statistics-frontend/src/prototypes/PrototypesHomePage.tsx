import React from 'react';
import { Route } from 'react-router-dom';
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
    <div>
      <h1>Prototypes</h1>

      <ul>
        <li>
          <Link to="/prototypes/publication">Publication Page</Link>
        </li>
      </ul>
    </div>
  );
};

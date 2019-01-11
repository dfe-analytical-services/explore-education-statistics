import React from 'react';
import { Route } from 'react-router-dom';
import Link from '../components/Link';
import TestPage from './TestPage';

export const PrototypeRoutes = () => (
  <>
    <Route exact path="/prototypes" component={PrototypesHomePage} />
    <Route exact path="/prototypes/test-page" component={TestPage} />
  </>
);

export const PrototypesHomePage = () => {
  return (
    <div>
      <h1>Prototypes</h1>

      <ul>
        <li>
          <Link to="/prototypes/test-page">Test Page</Link>
        </li>
      </ul>
    </div>
  );
};

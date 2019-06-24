import React from 'react';
import Link from '../../components/Link';
import PrototypePage from './components/PrototypePage';

function PrototypesIndexPage() {
  return (
    <PrototypePage>
      <h1>Index page for administrative application</h1>

      <h3>Prototypes</h3>
      <ul>
        <li>
          <Link to="/prototypes/admin-dashboard">
            Administrators dashboard page
          </Link>
        </li>
      </ul>

      <h3>Tools</h3>
      <ul>
        <li>
          <a href="/tools">Administrative tools</a>
        </li>
      </ul>
    </PrototypePage>
  );
}

export default PrototypesIndexPage;

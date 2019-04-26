import React from 'react';
import Link from '../../components/Link';
import PrototypePage from './components/PrototypePage';

function PrototypesIndexPage() {
  return (
    <PrototypePage>
      <h1>Prototype index page for administrators</h1>

      <ul>
        <li>
          <Link to="/prototypes/admin-dashboard">
            Administrators dashboard page
          </Link>
        </li>
      </ul>
    </PrototypePage>
  );
}

export default PrototypesIndexPage;

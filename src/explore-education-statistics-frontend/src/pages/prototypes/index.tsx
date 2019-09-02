import Link from '@frontend/components/Link';
import React from 'react';
import PrototypePage from '../../prototypes/components/PrototypePage';

function PrototypesIndexPage() {
  return (
    <PrototypePage>
      <h1>Prototypes index page</h1>

      <ul>
        <li>
          <Link to="/prototypes/start">Start page</Link>
        </li>
      </ul>
    </PrototypePage>
  );
}

export default PrototypesIndexPage;

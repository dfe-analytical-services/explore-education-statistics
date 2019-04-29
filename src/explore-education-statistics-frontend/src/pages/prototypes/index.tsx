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
        <li>
          <Link to="/prototypes/home-v2">Home page</Link>
        </li>
        <li>
          <Link to="/prototypes/browse-releases">Browse releases</Link>
        </li>
        <li>
          <Link to="/prototypes/publication">
            Publication page (pupil absence)
          </Link>
        </li>
        <li>
          <Link to="/prototypes/publication-exclusions">
            Publication page (exclusions)
          </Link>
        </li>
        <li>
          <Link to="/prototypes/publication-gcse">Publication page (KS4)</Link>
        </li>
        <li>
          <Link to="/prototypes/methodology-home">Methodology home</Link>
        </li>
        <li>
          <Link to="/prototypes/methodology-specific">
            Specific methodology
          </Link>
        </li>
        <li>
          <Link to="/prototypes/methodology-absence">
            Methodology page for absence
          </Link>
        </li>
      </ul>

      <ul>
        <li>
          <Link to="/prototypes/table-tool">Table tool page</Link>
        </li>
      </ul>

      <ul>
        <li>
          <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/">
            Administrator Prototype Index
          </a>
        </li>
      </ul>

      <ul>
        <li>
          <Link to="/prototypes/graphs">Example graphs page</Link>
        </li>
      </ul>
    </PrototypePage>
  );
}

export default PrototypesIndexPage;

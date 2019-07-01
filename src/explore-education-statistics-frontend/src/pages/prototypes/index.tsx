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
          <Link to="/prototypes/home">Home page</Link>
        </li>
        <li>
          <Link to="/prototypes/browse-releases">Browse releases</Link>
        </li>
        <li>
          <Link to="/prototypes/publication-gcse">
            Publication page example
          </Link>
          {` `}
          (Outdated){` `}
          <Link to="/statistics/pupil-absence-in-schools-in-england">
            See working app for lastest version
          </Link>
        </li>
        <li>
          <Link to="/prototypes/methodology-home">Methodology home</Link>
          {` `}
          (Outdated){` `}
          <Link to="/methodologies">See working app for lastest version</Link>
        </li>

        <li>
          <Link to="/prototypes/methodology-absence">
            Methodology page for absence
          </Link>
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

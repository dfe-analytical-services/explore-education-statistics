import React, { Component } from 'react';
import Link from 'src/components/Link';
import PrototypePage from '../../prototypes/components/PrototypePage';

class PrototypesIndexPage extends Component {
  public render() {
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
            <Link to="/prototypes/methodology-home">Methodology home</Link>
          </li>
          <li>
            <Link to="/prototypes/methodology-absence">
              Methodology page for absence
            </Link>
          </li>
        </ul>
      </PrototypePage>
    );
  }
}

export default PrototypesIndexPage;

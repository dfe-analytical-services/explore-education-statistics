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
            <Link to="/prototypes/start">Start page - test</Link>
          </li>
          <li>
            <Link to="/prototypes/home-v1">Home page (3 nav options)</Link>
          </li>
          <li>
            <Link to="/prototypes/home-v2">Home page (2 nav options)</Link>
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
        </ul>
      </PrototypePage>
    );
  }
}

export default PrototypesIndexPage;

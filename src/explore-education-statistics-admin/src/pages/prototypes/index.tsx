import React, { Component } from 'react';
import Link from '../../components/Link';
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
            <Link to="/prototypes/browse-releases">Browse releases</Link>
          </li>
          <li>
            <Link to="/prototypes/publication">
              Publication page (pupil absence)
            </Link>
          </li>
        </ul>
     </PrototypePage>
    );
  }
}

export default PrototypesIndexPage;

import React, { Component } from 'react';
import Link from '../components/Link';

class HomePage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1>Explore education statistics</h1>
          <p>Use this service to:</p>
          <ul>
            <li>do something</li>
            <li>do something else</li>
          </ul>
          <Link to="themes" role="button"
            className="govuk-button govuk-button--start govuk-!-margin-top-2 govuk-!-margin-bottom-8">
            Start now
          </Link>
          <h2>Before you start</h2>
          <p>Do something.</p>
        </div>

        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items" role="complementary">
            <h2>Quick Links</h2>

            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list govuk-body-s">
                <li>
                  <Link to="/themes" data-testid="home-page--themes-link">
                    Homepage (Tech Demo)
                  </Link>
                </li>
                <li>
                  <Link to="/local-authorities/sheffield">
                    Local Authority - Sheffield
                  </Link>
                </li>
                <li>
                  <Link to="/prototypes">Prototypes</Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>
    );
  }
}

export default HomePage;

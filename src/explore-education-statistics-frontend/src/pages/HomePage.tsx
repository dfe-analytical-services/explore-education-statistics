import React, { Component } from 'react';
import Link from '../components/Link';

class HomePage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Explore education statistics</h1>
          <p className="govuk-body">Use this service to:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>do something</li>
            <li>do something else</li>
          </ul>
          <a
            href="#"
            role="button"
            className="govuk-button govuk-button--start govuk-!-margin-top-2 govuk-!-margin-bottom-8"
          >
            Start now
          </a>
          <h2 className="govuk-heading-m">Before you start</h2>
          <p className="govuk-body">Do something.</p>
        </div>

        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items" role="complementary">
            <h2 className="govuk-heading-m">Quick Links</h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list govuk-!-font-size-16">
                <li>
                  <Link to="/themes">
                    Themes
                  </Link>
                </li>
                <li>
                  <Link to="topics">
                    Topics
                  </Link>
                </li>
                <li>
                  <Link to="/publications">
                    Publications
                  </Link>
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

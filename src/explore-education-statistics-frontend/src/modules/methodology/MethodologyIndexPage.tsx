import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React, { Component } from 'react';

class MethodologyIndexPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Methodology' }]}>
        <PageTitle title="Education statistics: methodology" />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Browse to find out about the methodology behind specific education
              statistics and data and how and why they're collected and
              published.
            </p>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h2 className="govuk-heading-m" id="releated-content">
                Related content
              </h2>
              <nav role="navigation" aria-labelledby="subsection-title">
                <ul className="govuk-list">
                  <li>
                    <Link to="/statistics">Find statistics and data</Link>
                  </li>
                  <li>
                    <Link to="/glossary">Glossary</Link>
                  </li>
                </ul>
              </nav>
            </aside>
          </div>
        </div>
      </Page>
    );
  }
}

export default MethodologyIndexPage;

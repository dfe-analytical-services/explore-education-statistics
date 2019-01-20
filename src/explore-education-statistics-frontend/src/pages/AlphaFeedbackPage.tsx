import React, { Component } from 'react';
import { Helmet } from 'react-helmet';

class AlphaFeedbackPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <Helmet>
          <title>Alpha Feedback - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          <div className="app-content__header">
            <span className="govuk-caption-xl">
              Explore education statistics service
            </span>
            <h1>Alpha feedback</h1>
          </div>
        </div>
      </div>
    );
  }
}

export default AlphaFeedbackPage;

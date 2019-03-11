import React, { Component } from 'react';
import { Helmet } from 'react-helmet';

class PrivacyPolicyPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <Helmet>
          <title>Privacy Policy - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          <h1>Privacy policy</h1>
        </div>
      </div>
    );
  }
}

export default PrivacyPolicyPage;

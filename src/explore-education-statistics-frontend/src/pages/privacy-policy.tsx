import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import Page from '../components/Page';

class PrivacyPolicyPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Privacy Policy' }]}>
        <Helmet>
          <title>Privacy Policy - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          <h1>Privacy policy</h1>
        </div>
      </Page>
    );
  }
}

export default PrivacyPolicyPage;

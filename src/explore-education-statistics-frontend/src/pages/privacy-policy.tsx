import React, { Component } from 'react';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';

class PrivacyPolicyPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Privacy Policy' }]}>
        <PageTitle title="Feedback" />
      </Page>
    );
  }
}

export default PrivacyPolicyPage;

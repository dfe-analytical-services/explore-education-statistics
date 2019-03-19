import React, { Component } from 'react';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';

class PrivacyPolicyPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Privacy Policy' }]}>
        <PageTitle title="Explore education statistics privacy policy" />

        <div>
          <h3>Who we are</h3>
          <p>
            The Explore education statistics service is operated by the
            Department for Education ('we' or 'us'). For the purpose of data
            protection legislation, DfE is the data controller for the personal
            data processed as part of the Explore education statistics service.
          </p>
          <p>
            This privacy policy sets out how we collect and process your
            personal data through the service. This policy applies to all users
            of the service ('you').
          </p>
        </div>
      </Page>
    );
  }
}

export default PrivacyPolicyPage;

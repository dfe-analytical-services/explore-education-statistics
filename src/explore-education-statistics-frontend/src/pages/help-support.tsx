import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function HelpSupportPage() {
  return (
    <Page breadcrumbs={[{ name: 'Help and support' }]}>
      <PageTitle title="Help and support" />
      <p>
        If you need help and support or have a question about education
        statistics and data contact:
        <a href="">how to manage cookies.</a>
      </p>

      <p>
        <strong>Explore education statistics team</strong>
      </p>

      <p>Email</p>
      <p>
        <a href="/help-about-service">About the service</a>
      </p>
    </Page>
  );
}

export default HelpSupportPage;

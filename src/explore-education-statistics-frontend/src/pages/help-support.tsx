import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function HelpSupportPage() {
  return (
    <Page breadcrumbs={[{ name: 'Help and support' }]}>
      <PageTitle title="Help and support" />
    </Page>
  );
}

export default HelpSupportPage;

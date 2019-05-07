import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function ContactPage() {
  return (
    <Page breadcrumbs={[{ name: 'Contact' }]}>
      <PageTitle title="Contact explore education statistics" />
    </Page>
  );
}

export default ContactPage;

import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function ContactPage() {
  return (
    <Page breadcrumbs={[{ name: 'Contact' }]}>
      <PageTitle title="Contact explore education statistics" />
      <p>
        If you need help and support or have a question about education
        statistics:
      </p>

      <p>
        <strong>Explore education statistics team</strong>
      </p>

      <p>Email</p>
      <p>
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>
    </Page>
  );
}

export default ContactPage;

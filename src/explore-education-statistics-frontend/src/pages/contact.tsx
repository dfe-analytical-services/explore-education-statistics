import Page from '@frontend/components/Page';
import React from 'react';

function ContactPage() {
  return (
    <Page
      title="Contact explore education statistics"
      breadcrumbLabel="Contact"
      pageMeta={{ title: 'Contact' }}
    >
      <p>
        If you need help and support or have a question about education
        statistics and data contact:
      </p>

      <p>
        <strong>Explore education statistics team</strong>
      </p>

      <p>
        Email
        <br />
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>
    </Page>
  );
}

export default ContactPage;

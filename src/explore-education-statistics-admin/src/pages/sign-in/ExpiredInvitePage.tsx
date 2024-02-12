import Page from '@admin/components/Page';
import React from 'react';

export default function ExpiredInvitePage() {
  return (
    <Page title="Invitation expired" caption="Explore education statistics">
      <p>Your invitation to the service has expired.</p>
      <p>
        You can request access to the Explore education statistics service by
        contacting{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
        .
      </p>
    </Page>
  );
}

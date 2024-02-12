import Page from '@admin/components/Page';
import React from 'react';

export default function NoInvitationPage() {
  return (
    <Page title="No invitation" caption="Explore education statistics">
      <p>You do not have an invitation to the service.</p>
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

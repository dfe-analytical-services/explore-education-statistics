import Page from '@admin/components/Page';
import React from 'react';

const ExpiredInvitePage = () => {
  return (
    <Page title="Invitation expired" caption="Explore education statistics">
      <p className="govuk-body">Your invitation to the service has expired.</p>
      <p className="govuk-body">
        You can request access to the Explore education statistics service by
        contacting{' '}
        <a
          className="govuk-link"
          href="mailto:explore.statistics@education.gov.uk"
        >
          explore.statistics@education.gov.uk
        </a>
        .
      </p>
    </Page>
  );
};

export default ExpiredInvitePage;

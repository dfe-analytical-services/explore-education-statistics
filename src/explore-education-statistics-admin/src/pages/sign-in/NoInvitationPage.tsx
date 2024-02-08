import Page from '@admin/components/Page';
import React from 'react';

const NoInvitationPage = () => {
  return (
    <Page title="No invitation" caption="Explore education statistics">
      <p className="govuk-body">
        You do not have an invitation to the service.
      </p>
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

export default NoInvitationPage;

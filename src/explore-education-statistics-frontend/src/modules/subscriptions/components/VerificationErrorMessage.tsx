import React from 'react';

const VerificationErrorMessage = () => {
  return (
    <div className="govuk-panel__body">
      <p>
        Your subscription verification token has expired. You can try again by
        re-subscribing from the publication's main screen.
      </p>
      <p>
        If this issue persists, please contact{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>{' '}
        for support with details of the publication you are trying to subscribe
        to.
      </p>
    </div>
  );
};

export default VerificationErrorMessage;

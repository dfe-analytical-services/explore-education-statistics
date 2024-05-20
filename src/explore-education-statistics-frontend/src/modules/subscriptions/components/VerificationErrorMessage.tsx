import React from 'react';
import Link from '@frontend/components/Link';

interface VerificationErrorMessageProps {
  slug: string;
}

const VerificationErrorMessage = ({ slug }: VerificationErrorMessageProps) => {
  return (
    <div className="govuk-panel__body">
      <p>
        Your subscription verification token has expired. You can try again by
        re-subscribing from the{' '}
        <Link to={`/find-statistics/${slug}`}>publication's main screen.</Link>
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

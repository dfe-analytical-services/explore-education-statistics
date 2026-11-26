import { useTenant } from '@common/contexts/TenantContext';
import React from 'react';
import Link from '@frontend/components/Link';

interface VerificationErrorMessageProps {
  slug: string;
}

const VerificationErrorMessage = ({ slug }: VerificationErrorMessageProps) => {
  const { contactEmail } = useTenant();

  return (
    <div className="govuk-panel__body">
      <p>
        Your subscription verification token has expired. You can try again by
        re-subscribing from the{' '}
        <Link to={`/find-statistics/${slug}`}>publication's main screen.</Link>
      </p>
      <p>
        If this issue persists, please contact{' '}
        <a href={`mailto:${contactEmail}`}>{contactEmail}</a> for support with
        details of the publication you are trying to subscribe to.
      </p>
    </div>
  );
};

export default VerificationErrorMessage;

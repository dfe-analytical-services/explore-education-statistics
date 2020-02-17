import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React from 'react';

const MethodologySummaryPage = ({ handleApiErrors }: ErrorControlProps) => {
  return (
    <>
      <h2 className="govuk-heading-l">Methodology summary</h2>
      <p>
        These details will be shown to users to help identify this methodology.
      </p>
    </>
  );
};

export default withErrorControl(MethodologySummaryPage);

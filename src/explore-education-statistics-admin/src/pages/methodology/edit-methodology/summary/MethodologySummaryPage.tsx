import { ErrorControlState } from '@admin/contexts/ErrorControlContext';
import withErrorControl from '@admin/hocs/withErrorControl';
import React from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

const MethodologySummaryPage = ({
  handleApiErrors,
}: ErrorControlState & MethodologyTabProps) => {
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

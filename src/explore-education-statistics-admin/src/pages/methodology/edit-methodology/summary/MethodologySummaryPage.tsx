import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

const MethodologySummaryPage = ({
  handleApiErrors,
}: ErrorControlProps & MethodologyTabProps) => {
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

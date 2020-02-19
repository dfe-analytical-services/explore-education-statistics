import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

const MethodologyReleaseStatusPage = ({
  handleApiErrors,
}: ErrorControlProps & MethodologyTabProps) => {
  return (
    <>
      <h2 className="govuk-heading-l">Methodology release status</h2>
      <p>Lorem ipsum.</p>
    </>
  );
};

export default withErrorControl(MethodologyReleaseStatusPage);

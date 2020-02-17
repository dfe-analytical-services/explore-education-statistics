import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React from 'react';

const MethodologyContentPage = ({ handleApiErrors }: ErrorControlProps) => {
  return (
    <>
      <h2 className="govuk-heading-l">Methodology content</h2>
      <p>Lorem ipsum.</p>
    </>
  );
};

export default withErrorControl(MethodologyContentPage);

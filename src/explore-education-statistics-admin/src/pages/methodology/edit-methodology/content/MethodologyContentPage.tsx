import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

const MethodologyContentPage = ({
  methodology,
}: ErrorControlProps & MethodologyTabProps) => {
  return (
    <>
      <main
        className="govuk-main-wrapper app-main-class"
        id="main-content"
        role="main"
      >
        <h1
          className="govuk-heading-xl"
          data-testid={`page-title ${methodology.title}`}
        >
          {methodology.title}
        </h1>
        <p>Lorem ipsum.</p>
        <p>{JSON.stringify(methodology)}</p>
      </main>
    </>
  );
};

export default withErrorControl(MethodologyContentPage);

import isBrowser from '@common/utils/isBrowser';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  id?: string;
}

const BrowserWarning = ({ children, id = 'browserWarning' }: Props) => {
  if (!isBrowser('IE')) {
    return null;
  }

  return (
    <div className="govuk-error-summary" role="alert" aria-labelledby={id}>
      <h2 className="govuk-error-summary__title" id={id}>
        Incompatible web browser detected
      </h2>

      <div className="govuk-error-summary__body">
        <p>
          The following features on this page are not supported by your web
          browser:
        </p>

        {children}

        <p>
          To enable all features, we recommend changing your web browser to
          Google Chrome or Microsoft Edge.
        </p>
      </div>
    </div>
  );
};

export default BrowserWarning;

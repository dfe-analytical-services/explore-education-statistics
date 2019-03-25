import React, { FunctionComponent, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  id?: string;
}

const ErrorMessage: FunctionComponent<Props> = ({ children, id }) => {
  return (
    <span className="govuk-error-message" id={id}>
      {children}
    </span>
  );
};

export default ErrorMessage;

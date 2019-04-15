import React, { ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  id?: string;
}

const ErrorMessage = ({ children, id }: Props) => {
  return (
    <span className="govuk-error-message" id={id}>
      {children}
    </span>
  );
};

export default ErrorMessage;

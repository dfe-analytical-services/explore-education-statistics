import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { ReactNode } from 'react';

interface Props {
  // Announce errors on specific fields only when the form error summary is not used.
  announceError?: boolean;
  children?: ReactNode;
  id?: string;
}

const ErrorMessage = ({ announceError = false, children, id }: Props) => {
  return (
    <span
      className="govuk-error-message"
      id={id}
      data-testid={id}
      role={announceError ? 'status' : undefined}
    >
      <VisuallyHidden>Error: </VisuallyHidden>
      {children}
    </span>
  );
};

export default ErrorMessage;

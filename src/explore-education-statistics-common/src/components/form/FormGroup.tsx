import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  hasError?: boolean;
}

const FormGroup = ({ children, className, hasError = false }: Props) => {
  return (
    <div
      className={classNames('govuk-form-group', className, {
        'govuk-form-group--error': hasError,
      })}
    >
      {children}
    </div>
  );
};

export default FormGroup;

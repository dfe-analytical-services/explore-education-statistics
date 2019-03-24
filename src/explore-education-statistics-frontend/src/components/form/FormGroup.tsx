import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  hasError?: boolean;
}

const FormGroup = ({ children, hasError = false }: Props) => {
  return (
    <div
      className={classNames('govuk-form-group', {
        'govuk-form-group--error': hasError,
      })}
    >
      {children}
    </div>
  );
};

export default FormGroup;

import classNames from 'classnames';
import React, { FunctionComponent, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  hasError?: boolean;
}

const FormGroup: FunctionComponent<Props> = ({
  children,
  hasError = false,
}) => {
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

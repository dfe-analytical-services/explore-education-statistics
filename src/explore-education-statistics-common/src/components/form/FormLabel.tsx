import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface FormLabelProps {
  id: string;
  hideLabel?: boolean;
  label: string | ReactNode;
}

const FormLabel = ({ id, hideLabel, label }: FormLabelProps) => {
  return (
    <label
      className={classNames('govuk-label', {
        'govuk-visually-hidden': hideLabel,
      })}
      htmlFor={id}
    >
      {label}
    </label>
  );
};

export default FormLabel;

import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface FormLabelProps {
  className?: string;
  id: string;
  hideLabel?: boolean;
  label: string | ReactNode;
}

const FormLabel = ({ className, id, hideLabel, label }: FormLabelProps) => {
  return (
    <label
      className={classNames('govuk-label', className, {
        'govuk-visually-hidden': hideLabel,
      })}
      htmlFor={id}
    >
      {label}
    </label>
  );
};

export default FormLabel;

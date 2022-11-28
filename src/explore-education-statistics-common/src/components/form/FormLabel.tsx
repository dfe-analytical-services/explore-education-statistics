import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface FormLabelProps {
  className?: string;
  id: string;
  hideLabel?: boolean;
  label: string | ReactNode;
  labelSize?: 'xl' | 'l' | 'm' | 's';
}

const FormLabel = ({
  className,
  id,
  hideLabel,
  label,
  labelSize,
}: FormLabelProps) => {
  return (
    <label
      className={classNames('govuk-label', className, {
        [`govuk-label--${labelSize}`]: labelSize,
        'govuk-visually-hidden': hideLabel,
      })}
      htmlFor={id}
    >
      {label}
    </label>
  );
};

export default FormLabel;

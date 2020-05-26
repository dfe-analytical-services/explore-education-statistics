import classNames from 'classnames';
import React, { FocusEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import FormGroup from './FormGroup';

export interface FormFieldsetProps {
  children?: ReactNode;
  className?: string;
  error?: string;
  hint?: string;
  id: string;
  legend: ReactNode | string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  legendHidden?: boolean;
  onBlur?: FocusEventHandler<HTMLFieldSetElement>;
  onFocus?: FocusEventHandler<HTMLFieldSetElement>;
}

const FormFieldset = ({
  children,
  className,
  error,
  hint,
  id,
  legend,
  legendSize = 'l',
  legendHidden = false,
  onBlur,
  onFocus,
}: FormFieldsetProps) => {
  return (
    <FormGroup hasError={!!error}>
      <fieldset
        aria-describedby={
          classNames({
            [`${id}-error`]: !!error,
            [`${id}-hint`]: !!hint,
          }) || undefined
        }
        className={classNames('govuk-fieldset', className)}
        id={id}
        onBlur={onBlur}
        onFocus={onFocus}
      >
        <legend
          className={classNames(
            'govuk-fieldset__legend',
            `govuk-fieldset__legend--${legendSize}`,
            {
              'govuk-visually-hidden': legendHidden,
            },
          )}
        >
          {legend}
        </legend>

        {hint && (
          <span className="govuk-hint" id={`${id}-hint`}>
            {hint}
          </span>
        )}

        {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

        {children}
      </fieldset>
    </FormGroup>
  );
};

export default FormFieldset;

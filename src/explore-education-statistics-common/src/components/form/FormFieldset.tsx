import classNames from 'classnames';
import React, { ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import FormGroup from './FormGroup';
import createDescribedBy from './util/createDescribedBy';

export interface FormFieldsetProps {
  children?: ReactNode;
  error?: string;
  hint?: string;
  id: string;
  legend: ReactNode | string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  legendHidden?: boolean;
  className?: string;
}

const FormFieldset = ({
  children,
  error,
  hint,
  id,
  legend,
  legendSize = 'm',
  legendHidden = false,
  className,
}: FormFieldsetProps) => {
  return (
    <FormGroup hasError={!!error}>
      <fieldset
        className={classNames('govuk-fieldset', {
          [className || '']: className,
        })}
        id={id}
        aria-describedby={createDescribedBy({
          id,
          error: !!error,
          hint: !!hint,
        })}
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

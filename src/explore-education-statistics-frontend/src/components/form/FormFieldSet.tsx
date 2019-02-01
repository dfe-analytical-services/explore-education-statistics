import classNames from 'classnames';
import React, { FunctionComponent, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import FormGroup from './FormGroup';
import createDescribedBy from './util/createDescribedBy';

export interface FieldSetProps {
  error?: string;
  hint?: string;
  id: string;
  legend?: ReactNode | string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
}

const FormFieldSet: FunctionComponent<FieldSetProps> = ({
  children,
  error,
  hint,
  id,
  legend,
  legendSize = 'm',
}) => {
  return (
    <FormGroup hasError={!!error}>
      <fieldset
        className="govuk-fieldset"
        id={id}
        aria-describedby={createDescribedBy({
          id,
          error: !!error,
          hint: !!hint,
        })}
      >
        {legend && (
          <legend
            className={classNames(
              'govuk-fieldset__legend',
              `govuk-fieldset__legend--${legendSize}`,
            )}
          >
            {legend}
          </legend>
        )}
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

export default FormFieldSet;

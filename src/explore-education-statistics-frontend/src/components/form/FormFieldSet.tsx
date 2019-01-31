import classNames from 'classnames';
import React, { FunctionComponent, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

export interface FieldSetProps {
  error?: string;
  hint?: string;
  id?: string;
  legend?: string | ReactNode;
  legendSize?: 'xl' | 'l' | 'm' | 's';
}

let idCounter = 0;

const FormFieldSet: FunctionComponent<FieldSetProps> = ({
  children,
  error,
  hint,
  id = `formFieldSet-${(idCounter += 1)}`,
  legend,
  legendSize = 'm',
}) => {
  return (
    <fieldset
      className="govuk-fieldset"
      aria-describedby={createDescribedBy({
        id,
        error: error !== undefined,
        hint: hint !== undefined,
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
  );
};

export default FormFieldSet;

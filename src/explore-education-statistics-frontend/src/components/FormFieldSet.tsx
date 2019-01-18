import classNames from 'classnames';
import React, { FunctionComponent, ReactNode } from 'react';

export interface FieldSetProps {
  legend: string | ReactNode;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  hint?: string;
  hintId?: string;
}

let idCounter = 0;

const FormFieldSet: FunctionComponent<FieldSetProps> = ({
  children,
  hint,
  hintId = `formFieldSetHint-${(idCounter += 1)}`,
  legend,
  legendSize = 'm',
}) => {
  return (
    <fieldset
      className="govuk-fieldset"
      aria-describedby={hint ? hintId : undefined}
    >
      <legend
        className={classNames(
          'govuk-fieldset__legend',
          `govuk-fieldset__legend--${legendSize}`,
        )}
      >
        {legend}
      </legend>
      {hint && (
        <span className="govuk-hint" id={hintId}>
          {hint}
        </span>
      )}
      {children}
    </fieldset>
  );
};

export default FormFieldSet;

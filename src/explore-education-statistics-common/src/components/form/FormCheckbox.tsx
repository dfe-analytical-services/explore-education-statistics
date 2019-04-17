import classNames from 'classnames';
import React, { ChangeEventHandler, ReactNode } from 'react';

export type CheckboxChangeEventHandler = ChangeEventHandler<HTMLInputElement>;

export interface FormCheckboxProps {
  checked?: boolean;
  conditional?: ReactNode;
  id: string;
  hint?: string;
  label: string;
  name: string;
  onChange?: CheckboxChangeEventHandler;
  value: string;
}

const FormCheckbox = ({
  checked,
  conditional,
  id,
  hint,
  label,
  name,
  onChange,
  value,
}: FormCheckboxProps) => {
  return (
    <>
      <div className="govuk-checkboxes__item">
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          className="govuk-checkboxes__input"
          checked={checked}
          id={id}
          name={name}
          onChange={onChange}
          type="checkbox"
          value={value}
        />
        <label className="govuk-label govuk-checkboxes__label" htmlFor={id}>
          {label}
        </label>
        {hint && (
          <span
            id={`${id}-item-hint`}
            className="govuk-hint govuk-checkboxes__hint"
          >
            {hint}
          </span>
        )}
      </div>
      {conditional && (
        <div
          className={classNames('govuk-checkboxes__conditional', {
            'govuk-checkboxes__conditional--hidden': !checked,
          })}
        >
          {conditional}
        </div>
      )}
    </>
  );
};

export default FormCheckbox;

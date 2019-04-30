import classNames from 'classnames';
import React, { ChangeEventHandler, ReactNode } from 'react';

export type RadioChangeEventHandler = ChangeEventHandler<HTMLInputElement>;

export interface FormRadioProps {
  checked?: boolean;
  defaultChecked?: boolean;
  conditional?: ReactNode;
  hint?: string;
  id: string;
  label: string;
  name: string;
  onChange?: RadioChangeEventHandler;
  value: string;
  children?: ReactNode;
}

const FormRadio = ({
  checked,
  defaultChecked,
  conditional,
  hint,
  id,
  label,
  name,
  onChange,
  value,
  children,
}: FormRadioProps) => {
  return (
    <>
      <div className="govuk-radios__item">
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          data-aria-controls={children ? `${id}-conditional` : undefined}
          className="govuk-radios__input"
          checked={checked}
          defaultChecked={defaultChecked}
          id={id}
          name={name}
          onChange={onChange}
          type="radio"
          value={value}
          data-testid={label}
        />
        <label className="govuk-label govuk-radios__label" htmlFor={id}>
          {label}
        </label>
        {hint && (
          <span
            id={`${id}-item-hint`}
            className="govuk-hint govuk-radios__hint"
          >
            {hint}
          </span>
        )}
      </div>

      {conditional && (
        <div
          className={classNames('govuk-radios__conditional', {
            'govuk-radios__conditional--hidden': !checked,
          })}
          hidden={!checked}
        >
          {conditional}
        </div>
      )}

      {children && (
        <div
          className="govuk-radios__conditional govuk-radios__conditional--hidden"
          id={`${id}-conditional`}
        >
          {children}
        </div>
      )}
    </>
  );
};

export default FormRadio;

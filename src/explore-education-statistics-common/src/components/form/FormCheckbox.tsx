import classNames from 'classnames';
import React, { ChangeEvent, memo, ReactNode } from 'react';

export type OtherCheckboxChangeProps = Pick<FormCheckboxProps, 'label'>;

export type CheckboxChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  otherCheckboxProps: OtherCheckboxChangeProps,
) => void;

export interface FormCheckboxProps {
  checked?: boolean;
  defaultChecked?: boolean;
  className?: string;
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
  defaultChecked,
  className,
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
      <div className={classNames('govuk-checkboxes__item', className)}>
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          className="govuk-checkboxes__input"
          checked={checked}
          defaultChecked={defaultChecked}
          id={id}
          name={name}
          onChange={event => {
            if (onChange) {
              onChange(event, { label });
            }
          }}
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

export default memo(FormCheckbox);

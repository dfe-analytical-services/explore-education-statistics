import classNames from 'classnames';
import React, { ChangeEvent, FocusEventHandler, memo, ReactNode } from 'react';

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
  boldLabel?: boolean;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: CheckboxChangeEventHandler;
  value: string;
  disabled?: boolean;
}

const FormCheckbox = ({
  checked,
  defaultChecked,
  className,
  conditional,
  id,
  hint,
  label,
  boldLabel = false,
  name,
  onBlur,
  onChange,
  value,
  disabled = false,
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
          onBlur={onBlur}
          onChange={event => {
            if (onChange) {
              onChange(event, { label });
            }
          }}
          type="checkbox"
          value={value}
          disabled={disabled}
        />
        <label
          className={classNames('govuk-label govuk-checkboxes__label', {
            'govuk-!-font-weight-bold': boldLabel,
          })}
          htmlFor={id}
        >
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

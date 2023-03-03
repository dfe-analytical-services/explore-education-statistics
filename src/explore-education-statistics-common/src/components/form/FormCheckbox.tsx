import classNames from 'classnames';
import React, {
  ChangeEvent,
  FocusEventHandler,
  memo,
  ReactNode,
  Ref,
} from 'react';

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
  hint?: string | ReactNode;
  hintSmall?: boolean;
  label: string;
  boldLabel?: boolean;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: CheckboxChangeEventHandler;
  value: string;
  disabled?: boolean;
  inputRef?: Ref<HTMLInputElement>;
}

const FormCheckbox = ({
  checked,
  defaultChecked,
  className,
  conditional,
  id,
  hint,
  hintSmall = false,
  label,
  boldLabel = false,
  name,
  onBlur,
  onChange,
  value,
  disabled = false,
  inputRef,
}: FormCheckboxProps) => {
  return (
    <>
      <div
        className={classNames('govuk-checkboxes__item', className)}
        data-testid={`Checkbox item for ${label}`}
      >
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
          ref={inputRef}
        />
        <label
          className={classNames('govuk-label govuk-checkboxes__label', {
            'govuk-!-font-weight-bold': boldLabel,
            'govuk-!-padding-bottom-1': hintSmall,
          })}
          htmlFor={id}
        >
          {label}
        </label>
        {hint && (
          <span
            id={`${id}-item-hint`}
            className={classNames('govuk-hint govuk-checkboxes__hint', {
              'govuk-!-font-size-14 govuk-!-margin-bottom-1': hintSmall,
            })}
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

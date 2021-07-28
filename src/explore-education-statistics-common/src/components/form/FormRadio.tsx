import useMounted from '@common/hooks/useMounted';
import classNames from 'classnames';
import React, { ChangeEvent, FocusEventHandler, memo, ReactNode } from 'react';

export type OtherRadioChangeProps = Pick<FormRadioProps, 'label'>;

export type RadioChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  radioProps: OtherRadioChangeProps,
) => void;

export interface FormRadioProps {
  checked?: boolean;
  conditional?: ReactNode;
  defaultChecked?: boolean;
  displayLabel?: ReactNode;
  hiddenConditional?: boolean;
  hint?: string | ReactNode;
  id: string;
  label: string;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: RadioChangeEventHandler;
  value: string;
  disabled?: boolean;
}

const FormRadio = ({
  checked,
  conditional,
  defaultChecked,
  displayLabel,
  hiddenConditional,
  hint,
  id,
  label,
  name,
  onBlur,
  onChange,
  value,
  disabled = false,
}: FormRadioProps) => {
  const { onMounted } = useMounted(undefined, false);

  /* eslint-disable jsx-a11y/role-supports-aria-props */
  return (
    <>
      <div
        className="govuk-radios__item"
        data-testid={`Radio item for ${label}`}
      >
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          aria-controls={onMounted(
            conditional ? `${id}-conditional` : undefined,
          )}
          aria-expanded={onMounted(conditional ? checked : undefined)}
          className="govuk-radios__input"
          checked={checked}
          defaultChecked={defaultChecked}
          disabled={disabled}
          id={id}
          name={name}
          onBlur={onBlur}
          onChange={event => {
            if (onChange) {
              onChange(event, { label });
            }
          }}
          type="radio"
          value={value}
        />
        <label className="govuk-label govuk-radios__label" htmlFor={id}>
          {displayLabel && displayLabel}
          {!displayLabel && label}
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
            'govuk-radios__conditional--hidden': !checked || hiddenConditional,
          })}
          id={`${id}-conditional`}
        >
          {conditional}
        </div>
      )}
    </>
  );
};

export default memo(FormRadio);

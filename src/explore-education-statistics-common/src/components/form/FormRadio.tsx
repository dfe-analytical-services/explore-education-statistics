import useMounted from '@common/hooks/useMounted';
import classNames from 'classnames';
import React, { ChangeEvent, memo, ReactNode } from 'react';

export type OtherRadioChangeProps = Pick<FormRadioProps, 'label'>;

export type RadioChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  radioProps: OtherRadioChangeProps,
) => void;

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
  disabled?: boolean;
  jsRequired?: boolean;
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
  disabled = false,
  jsRequired = false,
}: FormRadioProps) => {
  const { onMounted } = useMounted(undefined, jsRequired);

  /* eslint-disable jsx-a11y/role-supports-aria-props */
  return (
    <>
      <div className="govuk-radios__item">
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          data-aria-controls={onMounted(
            !conditional ? `${id}-conditional` : undefined,
          )}
          aria-controls={onMounted(
            conditional ? `${id}-conditional` : undefined,
          )}
          aria-expanded={onMounted(conditional ? checked : undefined)}
          className="govuk-radios__input"
          checked={checked}
          defaultChecked={defaultChecked}
          id={id}
          name={name}
          onChange={event => {
            if (onChange) {
              onChange(event, { label });
            }
          }}
          type="radio"
          value={value}
          data-testid={label}
          disabled={disabled}
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
          id={`${id}-conditional`}
        >
          {conditional}
        </div>
      )}
    </>
  );
};

export default memo(FormRadio);

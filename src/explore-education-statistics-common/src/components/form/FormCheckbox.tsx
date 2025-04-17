import useMounted from '@common/hooks/useMounted';
import classNames from 'classnames';
import React, {
  ChangeEvent,
  FocusEventHandler,
  memo,
  ReactNode,
  Ref,
} from 'react';
import styles from './FormCheckbox.module.scss';

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
  disabled?: boolean;
  inputRef?: Ref<HTMLInputElement>;
  value?: string;
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
  disabled = false,
  inputRef,
  value,
}: FormCheckboxProps) => {
  const { onMounted } = useMounted(undefined, false);

  return (
    <>
      <div
        className={classNames('govuk-checkboxes__item', className)}
        data-testid={`Checkbox item for ${label}`}
      >
        <input
          aria-describedby={hint ? `${id}-item-hint` : undefined}
          aria-controls={onMounted(
            conditional ? `${id}-conditional` : undefined,
          )}
          aria-expanded={onMounted(conditional ? checked : undefined)}
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
          disabled={disabled}
          ref={inputRef}
          value={value}
        />
        <label
          className={classNames(
            'govuk-label govuk-checkboxes__label',
            styles.label,
            {
              'govuk-!-font-weight-bold': boldLabel,
              'govuk-!-padding-bottom-1': hintSmall,
            },
          )}
          htmlFor={id}
        >
          {label}
        </label>
        {hint && (
          <div
            id={`${id}-item-hint`}
            className={classNames('govuk-hint govuk-checkboxes__hint', {
              'govuk-!-font-size-14 govuk-!-margin-bottom-1': hintSmall,
            })}
          >
            {hint}
          </div>
        )}
      </div>
      {conditional && (
        <div
          className={classNames('govuk-checkboxes__conditional', {
            'govuk-checkboxes__conditional--hidden': !checked,
          })}
          id={`${id}-conditional`}
        >
          {conditional}
        </div>
      )}
    </>
  );
};

export default memo(FormCheckbox);

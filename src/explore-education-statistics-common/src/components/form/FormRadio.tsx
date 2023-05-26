import useMounted from '@common/hooks/useMounted';
import styles from '@common/components/form/FormRadio.module.scss';
import classNames from 'classnames';
import React, {
  ChangeEvent,
  FocusEventHandler,
  memo,
  ReactNode,
  Ref,
} from 'react';

export type OtherRadioChangeProps = Pick<FormRadioProps, 'label'>;

export type RadioChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  radioProps: OtherRadioChangeProps,
) => void;

export interface FormRadioProps {
  checked?: boolean;
  className?: string;
  conditional?: ReactNode;
  defaultChecked?: boolean;
  disabled?: boolean;
  divider?: string;
  hiddenConditional?: boolean;
  hint?: string | ReactNode;
  hintSmall?: boolean;
  inlineHint?: boolean;
  id: string;
  inputRef?: Ref<HTMLInputElement>;
  label: string;
  labelClassName?: string;
  name: string;
  value: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: RadioChangeEventHandler;
}

const FormRadio = ({
  checked,
  className,
  conditional,
  defaultChecked,
  disabled = false,
  divider,
  hiddenConditional,
  hint,
  hintSmall = false,
  id,
  inlineHint,
  inputRef,
  label,
  labelClassName,
  name,
  value,
  onBlur,
  onChange,
}: FormRadioProps) => {
  const { onMounted } = useMounted(undefined, false);

  /* eslint-disable jsx-a11y/role-supports-aria-props */
  return (
    <>
      <div
        className={classNames('govuk-radios__item', className, {
          [styles.inlineHint]: inlineHint,
        })}
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
          ref={inputRef}
          type="radio"
          value={value}
        />
        <label
          className={classNames(
            'govuk-label govuk-radios__label',
            labelClassName,
          )}
          htmlFor={id}
        >
          {label}
        </label>
        {hint && (
          <span
            id={`${id}-item-hint`}
            className={classNames('govuk-hint govuk-radios__hint', {
              'govuk-!-font-size-14': hintSmall,
            })}
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

      {divider && <div className={styles.divider}>{divider}</div>}
    </>
  );
};

export default memo(FormRadio);

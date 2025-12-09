import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, {
  ChangeEventHandler,
  CSSProperties,
  FocusEventHandler,
  ReactNode,
  Ref,
} from 'react';
import ErrorMessage from '../ErrorMessage';

export type SelectChangeEventHandler = ChangeEventHandler<HTMLSelectElement>;

export interface SelectOption<Value = string | number> {
  label: string;
  value: Value;
  selected?: boolean;
  style?: CSSProperties;
}

export interface FormSelectProps
  extends Pick<FormLabelProps, 'hideLabel' | 'label'> {
  autoFocus?: boolean;
  className?: string;
  disabled?: boolean;
  error?: string;
  id: string;
  /**
   * Renders the label and select inline.
   * If a hint is provided it appears below the label
   * and select instead of between them.
   */
  inline?: boolean;
  /**
   * Renders the hint inline with the label.
   * Has no effect if `inline` is true.
   */
  inlineHint?: boolean;
  inputRef?: Ref<HTMLSelectElement>;
  hint?: string | ReactNode;
  labelClassName?: string;
  name: string;
  onBlur?: FocusEventHandler;
  onChange?: SelectChangeEventHandler;
  options?: SelectOption[];
  optGroups?: Dictionary<SelectOption[]>;
  order?:
    | (keyof SelectOption)[]
    | ((option: SelectOption) => SelectOption[keyof SelectOption])[];
  orderDirection?: ('asc' | 'desc')[];
  placeholder?: string;
  value?: string;
}

const FormSelect = ({
  autoFocus,
  className,
  disabled,
  error,
  id,
  inline = false,
  inlineHint = false,
  inputRef,
  hint,
  hideLabel,
  label,
  labelClassName,
  name,
  onBlur,
  onChange,
  options,
  optGroups,
  order = ['label'],
  orderDirection = ['asc'],
  placeholder,
  value,
}: FormSelectProps) => {
  const hintAndError = (
    <>
      {hint && (
        <div id={`${id}-hint`} className="govuk-hint">
          {hint}
        </div>
      )}
      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
    </>
  );

  return (
    <>
      <FormSelectWrapper inline={inline}>
        <FormSelectLabelWrapper inlineHint={inlineHint && !inline}>
          <FormLabel
            className={classNames(labelClassName, {
              'govuk-!-margin-right-2': inline,
            })}
            id={id}
            label={label}
            hideLabel={hideLabel}
          />

          {/* Hint and error moved below the select when inline */}
          {!inline && hintAndError}
        </FormSelectLabelWrapper>

        <select
          aria-describedby={
            classNames({
              [`${id}-error`]: !!error,
              [`${id}-hint`]: !!hint,
            }) || undefined
          }
          className={classNames('govuk-select', className, {
            'govuk-select--error': !!error,
          })}
          // eslint-disable-next-line jsx-a11y/no-autofocus
          autoFocus={autoFocus}
          id={id}
          name={name}
          disabled={disabled}
          ref={inputRef}
          onBlur={onBlur}
          onChange={onChange}
          value={value}
        >
          {placeholder && <option value="">{placeholder}</option>}
          {options &&
            (order === undefined || order.length === 0
              ? options
              : orderBy(options, order, orderDirection)
            ).map(option => (
              <option
                value={option.value}
                key={`${option.value}-${option.label}`}
                selected={option.selected}
                style={option.style}
              >
                {option.label}
              </option>
            ))}
          {optGroups &&
            Object.keys(optGroups)
              .sort()
              .map(group => (
                <optgroup key={group} label={group}>
                  {optGroups[group].map(option => (
                    <option
                      key={`value-${option.value}`}
                      value={option.value}
                      style={option.style}
                    >
                      {option.label}
                    </option>
                  ))}
                </optgroup>
              ))}
        </select>
      </FormSelectWrapper>
      {inline && hintAndError}
    </>
  );
};

FormSelect.unordered = [] as [];

export default FormSelect;

function FormSelectWrapper({
  inline,
  children,
}: {
  inline: boolean;
  children: ReactNode;
}) {
  return inline ? (
    <div className="dfe-flex dfe-align-items--center">{children}</div>
  ) : (
    <>{children}</>
  );
}

function FormSelectLabelWrapper({
  inlineHint,
  children,
}: {
  inlineHint: boolean;
  children: ReactNode;
}) {
  return inlineHint ? (
    <div className="dfe-flex dfe-justify-content--space-between">
      {children}
    </div>
  ) : (
    <>{children}</>
  );
}

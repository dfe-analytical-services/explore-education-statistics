import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, {
  ChangeEventHandler,
  CSSProperties,
  FocusEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';

export type SelectChangeEventHandler = ChangeEventHandler<HTMLSelectElement>;

export interface SelectOption<Value = string | number> {
  label: string;
  value: Value;
  selected?: boolean;
  style?: CSSProperties;
}

export interface FormSelectProps extends FormLabelProps {
  className?: string;
  disabled?: boolean;
  error?: string;
  id: string;
  inline?: boolean;
  hint?: string;
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
  className,
  disabled,
  error,
  id,
  inline = false,
  hint,
  hideLabel,
  label,
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
  return (
    <>
      <FormSelectWrapper inline={inline}>
        <FormLabel
          className={classNames({ 'govuk-!-margin-right-2': inline })}
          id={id}
          label={label}
          hideLabel={hideLabel}
        />

        {/* Hint and error moved below the select when inline */}
        {!inline && (
          <>
            {hint && (
              <span id={`${id}-hint`} className="govuk-hint">
                {hint}
              </span>
            )}
            {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
          </>
        )}

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
          id={id}
          name={name}
          disabled={disabled}
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
      {inline && (
        <>
          {hint && (
            <span id={`${id}-hint`} className="govuk-hint">
              {hint}
            </span>
          )}
          {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
        </>
      )}
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

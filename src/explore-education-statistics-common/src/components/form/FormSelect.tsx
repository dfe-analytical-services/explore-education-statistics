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

export interface SelectOption {
  label: string;
  value: string | number;
  selected?: boolean;
  style?: CSSProperties;
}

export interface FormSelectProps {
  disabled?: boolean;
  error?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onBlur?: FocusEventHandler;
  onChange?: SelectChangeEventHandler;
  options?: SelectOption[];
  optGroups?: Dictionary<SelectOption[]>;
  order?:
    | (keyof SelectOption)[]
    | ((option: SelectOption) => SelectOption[keyof SelectOption])[];
  orderDirection?: ('asc' | 'desc')[];
  value?: string | number;
  className?: string;
}

const FormSelect = ({
  disabled,
  error,
  id,
  label,
  name,
  onBlur,
  onChange,
  options,
  optGroups,
  order = ['label'],
  orderDirection = ['asc'],
  value,
  className,
}: FormSelectProps) => {
  return (
    <>
      <label className="govuk-label" htmlFor={id}>
        {label}
      </label>
      {error && <ErrorMessage>{error}</ErrorMessage>}
      <select
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
          Object.keys(optGroups).map(group => (
            <optgroup key={`group-${group}`} label={group}>
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
    </>
  );
};

export default FormSelect;

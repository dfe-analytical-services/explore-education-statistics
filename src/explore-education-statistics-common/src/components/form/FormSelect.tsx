import classNames from 'classnames';
import React, { ChangeEventHandler, FocusEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import styles from './FormSelect.module.scss';

export type SelectChangeEventHandler = ChangeEventHandler<HTMLSelectElement>;

export interface SelectOption {
  label: string;
  value: string | number;
}

export interface FormSelectProps {
  error?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onBlur?: FocusEventHandler;
  onChange?: SelectChangeEventHandler;
  options: SelectOption[];
  value?: string | number;
}

const FormSelect = ({
  error,
  id,
  label,
  name,
  onBlur,
  onChange,
  options,
  value,
}: FormSelectProps) => {
  return (
    <>
      <label className="govuk-label" htmlFor={id}>
        {label}
      </label>
      {error && <ErrorMessage>{error}</ErrorMessage>}
      <select
        className={classNames('govuk-select', styles.select, {
          'govuk-select--error': !!error,
        })}
        id={id}
        name={name}
        onBlur={onBlur}
        onChange={onChange}
        value={value}
      >
        {options.map(option => (
          <option value={option.value} key={`${option.value}-${option.label}`}>
            {option.label}
          </option>
        ))}
      </select>
    </>
  );
};

export default FormSelect;

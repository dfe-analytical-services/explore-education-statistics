import classNames from 'classnames';
import React, { ChangeEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import styles from './FormSelect.module.scss';

export interface SelectOption {
  label: string;
  value: string | number;
}

export interface FormSelectProps {
  error?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<any>;
  options: SelectOption[];
  value?: string | number;
}

const FormSelect = ({
  error,
  id,
  label,
  name,
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

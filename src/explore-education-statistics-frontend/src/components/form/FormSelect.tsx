import classNames from 'classnames';
import React, { ChangeEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import styles from './FormSelect.module.scss';

export interface SelectOption {
  text: string;
  value: string | number;
}

interface Props {
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
}: Props) => {
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
          <option value={option.value} key={`${option.value}-${option.text}`}>
            {option.text}
          </option>
        ))}
      </select>
    </>
  );
};

export default FormSelect;

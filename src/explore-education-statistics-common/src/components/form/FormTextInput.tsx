import classNames from 'classnames';
import React, { ChangeEventHandler, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

export interface FormTextInputProps {
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
}

const FormTextInput = ({
  error,
  hint,
  id,
  label,
  name,
  onChange,
  width,
}: FormTextInputProps) => {
  return (
    <>
      <label className="govuk-label" htmlFor={id}>
        {label}
      </label>
      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}
      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
      <input
        aria-describedby={createDescribedBy({
          id,
          error: !!error,
          hint: !!hint,
        })}
        type="text"
        className={classNames('govuk-input', {
          [`govuk-input--width-${width}`]: width !== undefined,
        })}
        id={id}
        name={name}
        onChange={event => {
          if (onChange) {
            onChange(event);
          }
        }}
      />
    </>
  );
};

export default FormTextInput;

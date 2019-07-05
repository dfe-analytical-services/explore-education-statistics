import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

export interface FormTextInputProps {
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
  value?: string;
  defaultValue?: string;
}

const FormTextInput = ({
  error,
  hint,
  id,
  label,
  width,
  ...props
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
        {...props}
        aria-describedby={createDescribedBy({
          id,
          error: !!error,
          hint: !!hint,
        })}
        className={classNames('govuk-input', {
          [`govuk-input--width-${width}`]: width !== undefined,
        })}
        id={id}
        type="text"
      />
    </>
  );
};

export default FormTextInput;

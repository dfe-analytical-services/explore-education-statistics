import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

export interface FormFileInputProps {
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

const FormFileInput = ({
  disabled,
  error,
  hint,
  id,
  label,
  name,
  onChange,
  onClick,
  onKeyPress,
}: FormFileInputProps) => {
  return (
    <>
      <label className="govuk-label govuk-label--s" htmlFor={id}>
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
        className="govuk-file-upload"
        id={id}
        type="file"
        disabled={disabled}
        name={name}
        onChange={onChange}
        onClick={onClick}
        onKeyPress={onKeyPress}
      />
    </>
  );
};

export default FormFileInput;

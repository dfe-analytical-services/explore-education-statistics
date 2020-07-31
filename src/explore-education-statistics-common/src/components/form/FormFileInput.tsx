import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';

export interface FormFileInputProps {
  accept?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

const FormFileInput = ({
  accept,
  disabled,
  error,
  hint,
  id,
  label,
  name,
  onBlur,
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
        aria-describedby={
          classNames({
            [`${id}-error`]: !!error,
            [`${id}-hint`]: !!hint,
          }) || undefined
        }
        className="govuk-file-upload"
        id={id}
        type="file"
        disabled={disabled}
        name={name}
        accept={accept}
        onBlur={onBlur}
        onChange={onChange}
        onClick={onClick}
        onKeyPress={onKeyPress}
      />
    </>
  );
};

export default FormFileInput;

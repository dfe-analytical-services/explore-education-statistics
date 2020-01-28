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
  type?: string;
  pattern?: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
  percentageWidth?: string;
  value?: string;
  defaultValue?: string;
  min?: string;
  max?: string;
  list?: string;
  disabled?: boolean;
}

const FormTextInput = ({
  error,
  hint,
  id,
  label,
  width,
  percentageWidth,
  type = 'text',
  ...props
}: FormTextInputProps) => {
  return (
    <>
      <label
        className={classNames('govuk-label', {
          'govuk-label--s': type === 'file',
        })}
        htmlFor={id}
      >
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
        className={classNames({
          'govuk-input': type !== 'file',
          'govuk-file-upload': type === 'file',
          [`govuk-input--width-${width}`]: width !== undefined,
          [`govuk-!-width-${percentageWidth}`]: percentageWidth !== undefined,
        })}
        id={id}
        type={type}
      />
    </>
  );
};

export default FormTextInput;

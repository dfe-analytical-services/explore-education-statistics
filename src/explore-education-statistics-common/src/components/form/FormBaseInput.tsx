import ErrorMessage from '@common/components/ErrorMessage';
import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  memo,
  MouseEventHandler,
  ReactNode,
} from 'react';

export interface FormBaseInputProps extends FormLabelProps {
  className?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  name: string;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

interface HiddenProps {
  defaultValue?: string | number;
  type?: 'text' | 'number' | 'color';
  value?: string | number;
}

const FormBaseInput = ({
  className,
  error,
  hint,
  id,
  hideLabel,
  label,
  width,
  type = 'text',
  ...props
}: FormBaseInputProps & HiddenProps) => {
  return (
    <>
      <FormLabel id={id} label={label} hideLabel={hideLabel} />

      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}
      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
      <input
        {...props}
        aria-describedby={
          classNames({
            [`${id}-error`]: !!error,
            [`${id}-hint`]: !!hint,
          }) || undefined
        }
        className={classNames('govuk-input', className, {
          [`govuk-input--width-${width}`]: width !== undefined,
        })}
        id={id}
        type={type}
      />
    </>
  );
};

export default memo(FormBaseInput);

import ErrorMessage from '@common/components/ErrorMessage';
import createDescribedBy from '@common/components/form/util/createDescribedBy';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
  memo,
} from 'react';

export interface FormBaseInputProps {
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  percentageWidth?: string;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
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
  error,
  hint,
  id,
  label,
  width,
  percentageWidth,
  type = 'text',
  ...props
}: FormBaseInputProps & HiddenProps) => {
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
          [`govuk-!-width-${percentageWidth}`]: percentageWidth !== undefined,
        })}
        id={id}
        type={type}
      />
    </>
  );
};

export default memo(FormBaseInput);

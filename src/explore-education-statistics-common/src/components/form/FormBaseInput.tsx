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
  addOn?: ReactNode;
  addOnContainerClassName?: string;
  className?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  list?: string;
  name: string;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

interface HiddenProps {
  defaultValue?: string | number;
  type?: 'text' | 'number' | 'color' | 'search';
  value?: string | number;
}

const FormBaseInput = ({
  addOn,
  addOnContainerClassName,
  className,
  error,
  hint,
  id,
  hideLabel,
  label,
  labelSize,
  width,
  type = 'text',
  ...props
}: FormBaseInputProps & HiddenProps) => {
  return (
    <>
      <FormLabel
        id={id}
        label={label}
        labelSize={labelSize}
        hideLabel={hideLabel}
      />

      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}

      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
      <div
        className={classNames(
          {
            'dfe-flex': !!addOn,
          },
          addOnContainerClassName,
        )}
      >
        <input
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...props}
          aria-describedby={
            classNames({
              [`${id}-error`]: !!error,
              [`${id}-hint`]: !!hint,
            }) || undefined
          }
          className={classNames('govuk-input', className, {
            [`govuk-input--width-${width}`]: width !== undefined,
            'govuk-input--error': !!error,
          })}
          id={id}
          type={type}
        />
        {addOn}
      </div>
    </>
  );
};

export default memo(FormBaseInput);

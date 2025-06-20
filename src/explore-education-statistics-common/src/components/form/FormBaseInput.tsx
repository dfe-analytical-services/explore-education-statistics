import ErrorMessage from '@common/components/ErrorMessage';
import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  HTMLAttributes,
  KeyboardEventHandler,
  memo,
  MouseEventHandler,
  ReactNode,
  Ref,
  useCallback,
} from 'react';

export interface FormBaseInputProps
  extends Pick<FormLabelProps, 'hideLabel' | 'label' | 'labelSize'> {
  addOn?: ReactNode;
  addOnContainerClassName?: string;
  announceError?: boolean;
  autoFocus?: boolean;
  className?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  inputMode?: HTMLAttributes<HTMLInputElement>['inputMode'];
  inputRef?: Ref<HTMLInputElement>;
  list?: string;
  maxLength?: number;
  name: string;
  trimValue?: boolean;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

interface HiddenProps {
  defaultValue?: string | number;
  type?: 'text' | 'color' | 'search';
  value?: string | number;
}

function FormBaseInput({
  addOn,
  addOnContainerClassName,
  announceError,
  className,
  error,
  hint,
  hideLabel,
  id,
  inputMode,
  inputRef,
  label,
  labelSize,
  maxLength,
  trimValue = true,
  width,
  type = 'text',
  onBlur,
  onChange,
  onKeyPress,
  ...props
}: FormBaseInputProps & HiddenProps) {
  const handleBlur: FocusEventHandler<HTMLInputElement> = useCallback(
    event => {
      if (trimValue) {
        // eslint-disable-next-line no-param-reassign
        event.target.value = event.target.value.trim();
        onChange?.(event);
      }

      onBlur?.(event);
    },
    [onBlur, onChange, trimValue],
  );

  const handleKeyPress: KeyboardEventHandler<HTMLInputElement> = useCallback(
    event => {
      if (trimValue && event.key === 'Enter') {
        (event.currentTarget as HTMLInputElement)?.blur();
      }

      onKeyPress?.(event);
    },
    [onKeyPress, trimValue],
  );

  const input = (
    <input
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      aria-describedby={
        classNames({
          [`${id}-error`]: !!error,
          [`${id}-hint`]: !!hint,
          [`${id}-info`]: !!maxLength,
        }) || undefined
      }
      className={classNames('govuk-input', className, {
        [`govuk-input--width-${width}`]: width !== undefined,
        'govuk-input--error': !!error,
      })}
      id={id}
      inputMode={inputMode}
      ref={inputRef}
      type={type}
      onBlur={handleBlur}
      onChange={onChange}
      onKeyPress={handleKeyPress}
    />
  );

  return (
    <>
      <FormLabel
        id={id}
        label={label}
        labelSize={labelSize}
        hideLabel={hideLabel}
      />

      {hint && (
        <div id={`${id}-hint`} className="govuk-hint">
          {hint}
        </div>
      )}
      {(announceError || error) && (
        <ErrorMessage announceError={announceError} id={`${id}-error`}>
          {error}
        </ErrorMessage>
      )}
      {addOn ? (
        <div className={classNames('dfe-flex', addOnContainerClassName)}>
          {input}
          {addOn}
        </div>
      ) : (
        input
      )}
    </>
  );
}

export default memo(FormBaseInput);

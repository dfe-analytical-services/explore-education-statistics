import ErrorMessage from '@common/components/ErrorMessage';
import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
  Ref,
  memo,
} from 'react';

export interface FormTextAreaProps
  extends Pick<FormLabelProps, 'hideLabel' | 'label'> {
  className?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  inputRef?: Ref<HTMLTextAreaElement>;
  maxLength?: number;
  name: string;
  rows?: number;
  value?: string;
  onBlur?: FocusEventHandler<HTMLTextAreaElement>;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onClick?: MouseEventHandler<HTMLTextAreaElement>;
  onKeyPress?: KeyboardEventHandler<HTMLTextAreaElement>;
}

function FormBaseTextArea({
  className,
  disabled,
  error,
  hint,
  id,
  inputRef,
  hideLabel,
  label,
  maxLength,
  name,
  rows = 5,
  value,
  onBlur,
  onChange,
  onClick,
  onKeyPress,
}: FormTextAreaProps) {
  return (
    <>
      <FormLabel id={id} label={label} hideLabel={hideLabel} />

      {hint && (
        <div id={`${id}-hint`} className="govuk-hint">
          {hint}
        </div>
      )}

      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

      <textarea
        aria-describedby={
          classNames({
            [`${id}-error`]: !!error,
            [`${id}-hint`]: !!hint,
            [`${id}-info`]: !!maxLength,
          }) || undefined
        }
        className={classNames('govuk-textarea', className, {
          'govuk-textarea--error': !!error,
        })}
        disabled={disabled}
        id={id}
        name={name}
        ref={inputRef}
        onBlur={onBlur}
        onChange={onChange}
        onClick={onClick}
        onKeyPress={onKeyPress}
        rows={rows}
        value={value}
      />
    </>
  );
}

export default memo(FormBaseTextArea);

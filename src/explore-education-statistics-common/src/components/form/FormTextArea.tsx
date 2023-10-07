import FormGroup from '@common/components/form/FormGroup';
import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
  Ref,
} from 'react';
import ErrorMessage from '../ErrorMessage';

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

export default function FormTextArea({
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
  const textArea = (
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
          'govuk-js-character-count govuk-textarea--error':
            maxLength && (value?.length ?? 0) > maxLength,
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

  if (!!maxLength && maxLength > 0) {
    const remaining = maxLength - (value?.length ?? 0);

    return (
      <div className="govuk-character-count">
        <FormGroup>{textArea}</FormGroup>

        <div
          aria-live="polite"
          className={classNames('govuk-character-count__message', {
            'govuk-hint': remaining >= 0,
            'govuk-error-message': remaining < 0,
          })}
          id={`${id}-info`}
        >
          {remaining >= 0
            ? `You have ${remaining} character${
                remaining !== 1 ? 's' : ''
              } remaining`
            : `You have ${Math.abs(remaining)} character${
                remaining !== -1 ? 's' : ''
              } too many`}
        </div>
      </div>
    );
  }

  return textArea;
}

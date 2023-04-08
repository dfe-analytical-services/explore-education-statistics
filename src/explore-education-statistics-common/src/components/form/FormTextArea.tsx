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

export interface FormTextAreaProps extends FormLabelProps {
  className?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  // eslint-disable-next-line react/no-unused-prop-types
  testId?: string;
  maxLength?: number;
  name: string;
  rows?: number;
  textAreaRef?: Ref<HTMLTextAreaElement>;
  value?: string;
  onBlur?: FocusEventHandler<HTMLTextAreaElement>;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onClick?: MouseEventHandler<HTMLTextAreaElement>;
  onKeyPress?: KeyboardEventHandler<HTMLTextAreaElement>;
}

const FormTextArea = ({
  className,
  disabled,
  error,
  hint,
  id,
  hideLabel,
  label,
  maxLength,
  name,
  rows = 5,
  textAreaRef,
  value,
  onBlur,
  onChange,
  onClick,
  onKeyPress,
}: FormTextAreaProps) => {
  const textArea = (
    <>
      <FormLabel id={id} label={label} hideLabel={hideLabel} />

      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}

      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

      <textarea
        data-testid="comment-textarea"
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
        ref={textAreaRef}
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

        <span
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
        </span>
      </div>
    );
  }

  return textArea;
};

export default FormTextArea;

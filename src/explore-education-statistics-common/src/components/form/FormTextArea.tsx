import FormLabel, { FormLabelProps } from '@common/components/form/FormLabel';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';

export interface FormTextAreaProps extends FormLabelProps {
  className?: string;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  name: string;
  rows?: number;
  value?: string;
  onBlur?: FocusEventHandler<HTMLTextAreaElement>;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onClick?: MouseEventHandler<HTMLTextAreaElement>;
  onKeyPress?: KeyboardEventHandler<HTMLTextAreaElement>;
}

const FormTextArea = ({
  className,
  error,
  hint,
  id,
  hideLabel,
  label,
  rows = 5,
  ...props
}: FormTextAreaProps) => {
  return (
    <>
      <FormLabel id={id} label={label} hideLabel={hideLabel} />

      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}

      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

      <textarea
        {...props}
        aria-describedby={
          classNames({
            [`${id}-error`]: !!error,
            [`${id}-hint`]: !!hint,
          }) || undefined
        }
        className={classNames('govuk-textarea', className)}
        id={id}
        rows={rows}
      />
    </>
  );
};

export default FormTextArea;

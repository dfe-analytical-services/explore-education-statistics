import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

export interface FormTextAreaProps {
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onKeyPress?: KeyboardEventHandler<HTMLTextAreaElement>;
  onClick?: MouseEventHandler<HTMLTextAreaElement>;
  rows?: number;
  value?: string;
  defaultValue?: string;
  list?: string;
  additionalClass?: string;
}

const FormTextArea = ({
  error,
  hint,
  id,
  label,
  additionalClass,
  ...props
}: FormTextAreaProps) => {
  return (
    <>
      <label
        className='govuk-label'
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
      <textarea
        {...props}
        className={classNames('govuk-textarea', {
          [additionalClass || '']: additionalClass,
        })}

        id={id}
      />
    </>
  );
};

export default FormTextArea;

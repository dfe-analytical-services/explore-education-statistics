import classNames from 'classnames';
import { FileUpload } from 'govuk-frontend';
import React, {
  ChangeEventHandler,
  DragEventHandler,
  FocusEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
  useEffect,
  useRef,
} from 'react';
import ErrorMessage from '../ErrorMessage';

export interface FormFileInputProps {
  accept?: string;
  disabled?: boolean;
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  onClick?: MouseEventHandler<HTMLInputElement>;
  onDrop?: DragEventHandler<HTMLDivElement>;
  onKeyPress?: KeyboardEventHandler<HTMLInputElement>;
}

const FormFileInput = ({
  accept,
  disabled,
  error,
  hint,
  id,
  label,
  name,
  onBlur,
  onChange,
  onClick,
  onDrop,
  onKeyPress,
}: FormFileInputProps) => {
  const dropZoneRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    // eslint-disable-next-line no-new
    new FileUpload(dropZoneRef.current);
  }, []);

  return (
    <>
      <label className="govuk-label govuk-label--s" htmlFor={id}>
        {label}
      </label>
      {hint && (
        <div id={`${id}-hint`} className="govuk-hint">
          {hint}
        </div>
      )}
      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
      <div
        className="govuk-drop-zone"
        data-module="govuk-file-upload"
        ref={dropZoneRef}
        onDrop={onDrop}
      >
        <input
          aria-describedby={
            classNames({
              [`${id}-error`]: !!error,
              [`${id}-hint`]: !!hint,
            }) || undefined
          }
          className="govuk-file-upload"
          id={id}
          type="file"
          disabled={disabled}
          name={name}
          accept={accept}
          onBlur={onBlur}
          onChange={onChange}
          onClick={onClick}
          onKeyPress={onKeyPress}
          data-testid={`file-input-${id}`}
        />
      </div>
    </>
  );
};

export default FormFileInput;

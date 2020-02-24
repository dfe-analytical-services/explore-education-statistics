import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import ErrorMessage from '../ErrorMessage';
import EditableMarkdownRenderer from '../../../../explore-education-statistics-admin/src/modules/find-statistics/components/EditableMarkdownRenderer';

export interface FormWysiwygAreaProps {
  error?: ReactNode | string;
  hint?: string;
  id: string;
  toolbarStyle?: 'full' | 'reduced';
  label: ReactNode | string;
  source: string;
  name: string;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onKeyPress?: KeyboardEventHandler<HTMLTextAreaElement>;
  onClick?: MouseEventHandler<HTMLTextAreaElement>;
  onContentChange: (content: string) => void;
  rows?: number;
  value?: string;
  defaultValue?: string;
  list?: string;
  additionalClass?: string;
}

const FormWysiwygArea = ({
  error,
  hint,
  source,
  onContentChange,
  id,
  label,
  ...props
}: FormWysiwygAreaProps) => {
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
      <EditableMarkdownRenderer
        {...props}
        id={id}
        onContentChange={(content: string) => onContentChange(content)}
        source={source}
      />
    </>
  );
};

export default FormWysiwygArea;

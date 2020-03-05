import React, {
  ChangeEventHandler,
  KeyboardEventHandler,
  MouseEventHandler,
  ReactNode,
} from 'react';
import EditableMarkdownRenderer, {
  MarkdownRendererProps,
} from '@admin/modules/find-statistics/components/EditableMarkdownRenderer';
import ErrorMessage from '@common/components/ErrorMessage';

export interface FormWysiwygAreaProps extends MarkdownRendererProps {
  error?: ReactNode | string;
  hint?: string;
  id: string;
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
        onContentChange={(content: string) => onContentChange(content)}
        source={source}
      />
    </>
  );
};

export default FormWysiwygArea;

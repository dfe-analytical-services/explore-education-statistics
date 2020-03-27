import EditableProps from '@admin/components/editable/types/EditableProps';
import FormEditor from '@admin/components/form/FormEditor';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface EditableMarkdownRendererProps extends EditableProps {
  allowHeadings?: boolean;
  id: string;
  label: string;
  source: string;
  toolbarConfig?: string[];
  onChange: (content: string) => void;
}

const EditableMarkdownRenderer = ({
  allowHeadings,
  canDelete,
  editable = true,
  id,
  label,
  source = '',
  toolbarConfig,
  onChange,
  onDelete,
}: EditableMarkdownRendererProps) => {
  return (
    <FormEditor
      useMarkdown
      hideLabel
      value={source}
      id={id}
      label={label}
      allowHeadings={allowHeadings}
      canDelete={canDelete}
      editable={editable}
      toolbarConfig={toolbarConfig}
      onChange={onChange}
      onDelete={onDelete}
    />
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);

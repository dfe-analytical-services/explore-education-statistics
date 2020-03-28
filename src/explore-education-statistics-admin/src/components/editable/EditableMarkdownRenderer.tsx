import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableProps from '@admin/components/editable/types/EditableProps';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface EditableMarkdownRendererProps extends EditableProps {
  allowHeadings?: boolean;
  id: string;
  label: string;
  source: string;
  onSave: (content: string) => void;
}

const EditableMarkdownRenderer = ({
  allowHeadings,
  canDelete,
  editable = true,
  source = '',
  id,
  label,
  onSave,
  onDelete,
}: EditableMarkdownRendererProps) => {
  return (
    <EditableContentBlock
      allowHeadings={allowHeadings}
      editable={editable}
      id={id}
      label={label}
      value={source}
      canDelete={canDelete}
      onSave={onSave}
      onDelete={onDelete}
      useMarkdown
    />
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);

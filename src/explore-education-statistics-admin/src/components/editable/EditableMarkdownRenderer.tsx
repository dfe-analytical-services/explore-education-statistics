import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableProps from '@admin/components/editable/types/EditableProps';
import { useEditingContext } from '@admin/contexts/EditingContext';
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
  const { isEditing } = useEditingContext();

  if (!isEditing) {
    return <ReactMarkdown source={source} />;
  }

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

export default EditableMarkdownRenderer;

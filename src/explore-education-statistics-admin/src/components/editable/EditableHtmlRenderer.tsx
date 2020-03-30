import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableProps from '@admin/components/editable/types/EditableProps';
import { useEditingContext } from '@admin/contexts/EditingContext';
import React from 'react';

export interface EditableHtmlRendererProps extends EditableProps {
  allowHeadings?: boolean;
  id: string;
  label: string;
  source: string;
  onSave: (content: string) => void;
}

const EditableHtmlRenderer = ({
  allowHeadings,
  canDelete,
  editable = true,
  id,
  label,
  source = '',
  onDelete,
  onSave,
}: EditableHtmlRendererProps) => {
  const { isEditing } = useEditingContext();

  if (!isEditing) {
    return (
      <div
        // eslint-disable-next-line react/no-danger
        dangerouslySetInnerHTML={{ __html: source }}
      />
    );
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
    />
  );
};

export default EditableHtmlRenderer;

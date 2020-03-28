import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableProps from '@admin/components/editable/types/EditableProps';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
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

const HtmlRenderer = ({ source }: { source: string }) => {
  return (
    <div
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: source }}
    />
  );
};

export default wrapEditableComponent(EditableHtmlRenderer, HtmlRenderer);

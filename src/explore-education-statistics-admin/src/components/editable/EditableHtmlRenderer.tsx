import EditableProps from '@admin/components/editable/types/EditableProps';
import FormEditor from '@admin/components/form/FormEditor';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';

export interface EditableHtmlRendererProps extends EditableProps {
  allowHeadings?: boolean;
  id: string;
  label: string;
  source: string;
  onChange: (content: string) => void;
}

const EditableHtmlRenderer = ({
  allowHeadings,
  canDelete,
  editable = true,
  id,
  label,
  source = '',
  onDelete,
  onChange,
}: EditableHtmlRendererProps) => {
  return (
    <FormEditor
      value={source}
      editable={editable}
      id={id}
      label={label}
      allowHeadings={allowHeadings}
      canDelete={canDelete}
      onChange={onChange}
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

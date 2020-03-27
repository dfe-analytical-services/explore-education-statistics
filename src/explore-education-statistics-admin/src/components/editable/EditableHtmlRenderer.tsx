import EditableProps from '@admin/components/editable/types/EditableProps';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';

export interface EditableHtmlRendererProps extends EditableProps {
  allowHeadings?: boolean;
  source: string;
  onContentChange: (content: string) => void;
}

const EditableHtmlRenderer = ({
  allowHeadings,
  source = '',
  canDelete,
  editable = true,
  onDelete,
  onContentChange,
}: EditableHtmlRendererProps) => {
  return (
    <WysiwygEditor
      content={source}
      editable={editable}
      allowHeadings={allowHeadings}
      canDelete={canDelete}
      onContentChange={onContentChange}
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

import EditableProps from '@admin/components/editable/types/EditableProps';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface EditableMarkdownRendererProps extends EditableProps {
  allowHeadings?: boolean;
  insideAccordion?: boolean;
  source: string;
  toolbarConfig?: string[];
  onContentChange: (content: string) => void;
}

const EditableMarkdownRenderer = ({
  allowHeadings,
  canDelete,
  editable = true,
  source = '',
  toolbarConfig,
  onContentChange,
  onDelete,
}: EditableMarkdownRendererProps) => {
  return (
    <WysiwygEditor
      content={source}
      allowHeadings={allowHeadings}
      canDelete={canDelete}
      editable={editable}
      useMarkdown
      toolbarConfig={toolbarConfig}
      onContentChange={onContentChange}
      onDelete={onDelete}
    />
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);

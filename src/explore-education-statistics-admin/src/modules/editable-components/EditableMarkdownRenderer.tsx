import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';

export type MarkdownRendererProps = RendererProps &
  ReactMarkdownProps & {
    canDelete?: boolean;
    onDelete: () => void;
    toolbarConfig?: string[];
    editable?: boolean;
    onContentChange: (content: string) => void;
  };

const EditingMarkdownRenderer = ({
  source = '',
  canDelete,
  toolbarConfig,
  onDelete,
  editable = true,
  onContentChange,
}: MarkdownRendererProps) => {
  return (
    <>
      <WysiwygEditor
        content={source}
        canDelete={canDelete}
        editable={editable}
        useMarkdown
        toolbarConfig={toolbarConfig}
        onContentChange={onContentChange}
        onDelete={onDelete}
      />
    </>
  );
};
const EditableMarkdownRenderer = wrapEditableComponent(
  EditingMarkdownRenderer,
  ReactMarkdown,
);
export default EditableMarkdownRenderer;

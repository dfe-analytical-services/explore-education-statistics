import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import wrapEditableComponent, {
  EditingContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext } from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';

export type MarkdownRendererProps = RendererProps &
  ReactMarkdownProps & {
    canDelete: boolean;
    onDelete: () => void;
    editable?: boolean;
    onContentChange: (content: string) => Promise<unknown>;
  };

const EditingMarkdownRenderer = ({
  source = '',
  canDelete,
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

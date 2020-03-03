import React, { useContext } from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';

export type MarkdownRendererProps = RendererProps &
  ReactMarkdownProps & {
    canDelete?: boolean;
    onDelete?: () => void;
    toolbarConfig?: string[];
    editable?: boolean;
    onContentChange?: (content: string) => void;
  };

const EditableMarkdownRenderer = ({
  contentId,
  source,
  canDelete,
  toolbarConfig,
  onDelete,
  editable = true,
  onContentChange,
}: MarkdownRendererProps) => {
  const [markdown, setMarkdown] = React.useState(source);

  const editingContext = React.useContext(EditingContentBlockContext);

  const { handleApiErrors } = useContext(ErrorControlContext);

  return (
    <>
      <WysiwygEditor
        content={markdown || ''}
        canDelete={canDelete}
        editable={editable}
        useMarkdown
        toolbarConfig={toolbarConfig}
        onContentChange={async (ss: string) => {
          if (
            editingContext.releaseId &&
            editingContext.sectionId &&
            contentId
          ) {
            await releaseContentService
              .updateContentSectionBlock(
                editingContext.releaseId,
                editingContext.sectionId,
                contentId,
                {
                  body: ss,
                },
              )
              .catch(handleApiErrors);
          }
          if (onContentChange) onContentChange(ss);
          setMarkdown(ss);
        }}
        onDelete={onDelete}
      />
    </>
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);

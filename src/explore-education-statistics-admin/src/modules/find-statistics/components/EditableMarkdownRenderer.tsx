import WysiwygEditor from '@admin/components/WysiwygEditor';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';

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
            await releaseContentService.updateContentSectionBlock(
              editingContext.releaseId,
              editingContext.sectionId,
              contentId,
              {
                body: ss,
              },
            );
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

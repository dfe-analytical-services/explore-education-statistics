import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import React from 'react';
import ReactMarkdown, { ReactMarkdownProps } from 'react-markdown';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlock';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';

export type MarkdownRendererProps = RendererProps &
  ReactMarkdownProps & {
    canDelete: boolean;
    onDelete: () => void;
    editable?: boolean;
  };

const EditableMarkdownRenderer = ({
  contentId,
  source,
  canDelete,
  onDelete,
  editable = true,
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
        onContentChange={async ss => {
          if (
            editingContext.releaseId &&
            editingContext.sectionId &&
            contentId
          ) {
            const {
              body,
            } = await releaseContentService.updateContentSectionBlock(
              editingContext.releaseId,
              editingContext.sectionId,
              contentId,
              {
                body: ss,
              },
            );

            setMarkdown(body);
          }
        }}
        onDelete={onDelete}
      />
    </>
  );
};

export default wrapEditableComponent(EditableMarkdownRenderer, ReactMarkdown);

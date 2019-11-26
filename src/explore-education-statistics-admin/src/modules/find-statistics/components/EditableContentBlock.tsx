import { EditableRelease } from '@admin/services/publicationService';
import ContentBlock, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlock';
import { ContentBlock as ContentBlockData } from '@common/services/publicationService';
import React from 'react';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';
import AddComment from '@admin/pages/prototypes/components/PrototypeEditableContentAddComment';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import ResolveComment from '@admin/pages/prototypes/components/PrototypeEditableContentResolveComment';
import { ContentBlockViewModel } from '@admin/services/release/edit-release/content/types';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

export interface Props extends ContentBlockProps {
  content: EditableRelease['content'][0]['content'];

  sectionId: string;

  editable?: boolean;
  canAddBlocks: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  onContentChange?: (block: ContentBlockData, content: string) => void;
  onAddContent?: (content: ContentBlockViewModel) => void;
}

interface EditingContentBlockContext extends ReleaseContentContext {
  sectionId?: string;
}

export const EditingContentBlockContext = React.createContext<
  EditingContentBlockContext
>({
  releaseId: undefined,
  isEditing: false,
  sectionId: undefined,
});

interface AddContentButtonProps {
  order?: number;
  onClick: (type: string, order: number | undefined) => void;
}

const AddContentButton = ({ order, onClick }: AddContentButtonProps) => {
  return (
    <>
      <Button
        className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
        onClick={() => onClick('HtmlBlock', order)}
      >
        Add HTML
      </Button>
    </>
  );
};

const EditableContentBlock = ({
  content,
  id = '',
  sectionId,
  editable,
  onContentChange,
  reviewing,
  resolveComments,
  canAddBlocks = false,
  onAddContent,
}: Props) => {
  const editingContext = React.useContext(EditingContext);

  const [contentBlocks, setContentBlocks] = React.useState(content);

  if (content.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  const onAddContentCallback = (type: string, order: number | undefined) => {
    if (editingContext.releaseId && sectionId) {
      const { releaseId } = editingContext;

      releaseContentService
        .addContentSectionBlock(releaseId, sectionId, {
          body: 'Click to edit',
          type,
          order,
        })
        .then(result => {
          if (onAddContent) onAddContent(result);
          return result;
        })
        .then(() =>
          releaseContentService.getContentSection(releaseId, sectionId),
        )
        .then(section => {
          setContentBlocks(section.content);
        });
    }
  };

  const onDeleteContent = async (contentId: string) => {
    const { releaseId } = editingContext;
    if (releaseId && sectionId && contentId) {
      await releaseContentService.deleteContentSectionBlock(
        releaseId,
        sectionId,
        contentId,
      );

      const { content: newContent } = await releaseContentService.getContentSection(
        releaseId,
        sectionId,
      );

      setContentBlocks(newContent);
    }
  };

  return (
    <EditingContentBlockContext.Provider
      value={{
        ...editingContext,
        sectionId,
      }}
    >
      {contentBlocks.map((block, index) => {
        const key = `${index}-${block.heading}-${block.type}`;
        return (
          <React.Fragment key={key}>
            {reviewing && <AddComment initialComments={block.comments} />}
            {resolveComments && (
              <ResolveComment initialComments={block.comments} />
            )}
            {canAddBlocks && (
              <AddContentButton order={index} onClick={onAddContentCallback} />
            )}
            <EditableContentSubBlockRenderer
              editable={editable}
              canDelete={!!canAddBlocks}
              block={block}
              id={id}
              index={index}
              onContentChange={newContent => {
                if (onContentChange) {
                  onContentChange(block, newContent);
                }
              }}
              onDelete={() => onDeleteContent(block.id)}
            />
            {canAddBlocks && index === contentBlocks.length - 1 && (
              <AddContentButton onClick={onAddContentCallback} />
            )}
          </React.Fragment>
        );
      })}
    </EditingContentBlockContext.Provider>
  );
};

export default wrapEditableComponent(EditableContentBlock, ContentBlock);

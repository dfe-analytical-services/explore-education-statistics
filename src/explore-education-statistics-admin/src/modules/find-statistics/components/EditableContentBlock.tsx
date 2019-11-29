/* eslint-disable react/no-array-index-key */
import { EditableRelease } from '@admin/services/publicationService';
import ContentBlock, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlock';
import { ContentBlock as ContentBlockData } from '@common/services/publicationService';
import React, { ReactNode } from 'react';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';
import AddComment from '@admin/pages/prototypes/components/PrototypeEditableContentAddComment';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import ResolveComment from '@admin/pages/prototypes/components/PrototypeEditableContentResolveComment';
import { ContentBlockViewModel } from '@admin/services/release/edit-release/content/types';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
} from 'react-beautiful-dnd';
import classnames from 'classnames';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';
import styles from './EditableContentBlock.module.scss';

export interface Props extends ContentBlockProps {
  content: EditableRelease['content'][0]['content'];

  sectionId: string;

  editable?: boolean;
  canAddBlocks: boolean;
  isReordering?: boolean;
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

const WrappedInDroppable = ({
  draggable,
  droppableId,
  children,
}: {
  draggable: boolean;
  droppableId: string;
  children: ReactNode | ReactNode[];
}) => {
  return (
    <>
      {draggable ? (
        <Droppable droppableId={droppableId} type="content">
          {droppableProvided => (
            <div
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
            >
              {children}
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      ) : (
        children
      )}
    </>
  );
};

const WrappedInDraggable = ({
  draggable,
  draggableId,
  index,
  children,
}: {
  draggable: boolean;
  draggableId: string;
  index: number;
  children: ReactNode | ReactNode[];
}) => (
  <>
    {draggable ? (
      <Draggable draggableId={draggableId} index={index} type="contentBlock">
        {draggableProvided => (
          <div
            {...draggableProvided.draggableProps}
            ref={draggableProvided.innerRef}
            className={styles.isDragging}
          >
            <span
              {...draggableProvided.dragHandleProps}
              className={styles.dragHandle}
            />
            {children}
          </div>
        )}
      </Draggable>
    ) : (
      children
    )}
  </>
);

const EditableContentBlock = ({
  content,
  id = '',
  sectionId,
  editable = true,
  onContentChange,
  reviewing,
  resolveComments,
  canAddBlocks = false,
  onAddContent,
  isReordering = false,
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

      const {
        content: newContent,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      setContentBlocks(newContent);
    }
  };

  const onDragEnd = (result: DropResult) => {
    const { source, destination, type } = result;

    if (type === 'content' && destination) {
      const newContentBlocks = [...contentBlocks];
      const [removed] = newContentBlocks.splice(source.index,1);
      newContentBlocks.splice(destination.index, 0, removed);
      setContentBlocks(newContentBlocks);
    }
  };

  const saveOrder = () => {

  };

  return (
    <EditingContentBlockContext.Provider
      value={{
        ...editingContext,
        sectionId,
      }}
    >
      <DragDropContext onDragEnd={onDragEnd}>
        <WrappedInDroppable
          draggable={isReordering}
          droppableId={`${sectionId}`}
        >
          {contentBlocks.map((block, index) => (
            <WrappedInDraggable
              draggable={isReordering}
              draggableId={`${block.id}`}
              key={`${block.id}`}
              index={index}
            >
              {!isReordering && (
                <>
                  {reviewing && <AddComment initialComments={block.comments} />}
                  {resolveComments && (
                    <ResolveComment initialComments={block.comments} />
                  )}
                  {canAddBlocks && (
                    <AddContentButton
                      order={index}
                      onClick={onAddContentCallback}
                    />
                  )}
                </>
              )}

              <EditableContentSubBlockRenderer
                editable={editable && !isReordering}
                canDelete={!!canAddBlocks && !isReordering}
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

              {!isReordering &&
                canAddBlocks &&
                index === contentBlocks.length - 1 && (
                  <AddContentButton onClick={onAddContentCallback} />
                )}
            </WrappedInDraggable>
          ))}
        </WrappedInDroppable>
      </DragDropContext>
    </EditingContentBlockContext.Provider>
  );
};

export default wrapEditableComponent(EditableContentBlock, ContentBlock);

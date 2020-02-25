import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import { ReorderHook } from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import sortBy from 'lodash/sortBy';
import React, { MouseEventHandler, ReactNode, useContext } from 'react';
import { Dictionary } from 'src/types';
import AddContentButton from './AddContentButton';
import Comments from './Comments';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

export interface ReleaseContentAccordionSectionProps {
  id?: string;
  contentItem: ContentType;
  index: number;

  headingButtons?: ReactNode[];
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange?: (content?: EditableContentBlock[]) => void;
  canToggle?: boolean;
  onRemoveSection?: EditableAccordionSectionProps['onRemoveSection'];
  canAddBlocks?: boolean;
}

const ReleaseContentAccordionSection = ({
  id: sectionId,
  index,
  contentItem,
  headingButtons,
  canToggle = true,
  onHeadingChange,
  onContentChange,
  onRemoveSection,
  canAddBlocks = true,
}: ReleaseContentAccordionSectionProps) => {
  const { caption, heading } = contentItem;
  const [isReordering, setIsReordering] = React.useState(false);
  const [content, setContent] = React.useState(contentItem.content);
  const saveOrder = React.useRef<ReorderHook>();
  const {
    isEditing,
    isCommenting,
    isReviewing,
    availableDataBlocks,
    updateAvailableDataBlocks,
    releaseId,
  } = useContext(EditingContext);
  const { handleApiErrors } = useContext(ErrorControlContext);

  const onReorderClick: MouseEventHandler = e => {
    e.stopPropagation();
    e.preventDefault();

    if (isReordering) {
      if (saveOrder.current) {
        saveOrder.current(contentItem.id).then(() => setIsReordering(false));
      } else {
        setIsReordering(false);
      }
    } else {
      setIsReordering(true);
    }
  };

  // const contentChange = React.useCallback(
  //   (newContent?: EditableContentBlock[]) => {
  //     setContent(newContent);
  //     if (onContentChange) {
  //       onContentChange(newContent);
  //     }
  //   },
  //   [onContentChange],
  // );

  const onSaveBlockOrder = async (order: Dictionary<number>) => {
    return null;
  };

  const onBlockContentChange = (blockId: string) => {
    return async (newContent: string) => {
      if (releaseId && sectionId && blockId) {
        const { body } = await releaseContentService
          .updateContentSectionBlock(releaseId, sectionId, blockId, {
            body: newContent,
          })
          .catch(handleApiErrors);

        if (onContentChange) onContentChange(body);
      }
    };
  };
  const onBlockDelete = (blockId: string) => {
    return async () => {
      if (releaseId && sectionId && blockId) {
        await releaseContentService
          .deleteContentSectionBlock(releaseId, sectionId, blockId)
          .catch(handleApiErrors);

        if (updateAvailableDataBlocks) {
          updateAvailableDataBlocks();
        }

        const {
          content: newContentBlocks,
        } = await releaseContentService.getContentSection(releaseId, sectionId);

        setContent(newContentBlocks);

        if (onContentChange) onContentChange(newContentBlocks);
      }
    };
  };

  const onAddContentCallback = (
    type: string,
    data: string,
    order: number | undefined,
  ) => {
    if (releaseId && sectionId) {
      let addPromise: Promise<unknown>;

      if (type === 'DataBlock') {
        addPromise = releaseContentService
          .attachContentSectionBlock(releaseId, sectionId, {
            contentBlockId: data,
            order: order || 0,
          })
          .then(v => {
            if (updateAvailableDataBlocks) {
              updateAvailableDataBlocks();
            }
            return v;
          });
      } else {
        addPromise = releaseContentService.addContentSectionBlock(
          releaseId,
          sectionId,
          {
            body: data,
            type,
            order,
          },
        );
      }

      addPromise
        .then(() =>
          releaseContentService.getContentSection(releaseId, sectionId),
        )
        .then(section => {
          if (onContentChange) onContentChange(section.content);
          setContent(section.content);
        })
        .catch(handleApiErrors);
    }
  };

  return (
    <AccordionSection
      id={sectionId}
      index={index}
      heading={heading || ''}
      caption={caption}
      canToggle={canToggle}
      headingButtons={headingButtons || []}
      onHeadingChange={onHeadingChange}
      onRemoveSection={onRemoveSection}
      onSaveOrder={onSaveBlockOrder}
      sectionId={sectionId}
      footerButtons={
        !isReordering &&
        canAddBlocks && (
          <AddContentButton
            onClick={(type, data) =>
              onAddContentCallback(type, data, content ? content.length : 0)
            }
            availableDataBlocks={availableDataBlocks}
          />
        )
      }
    >
      {/*
      <ContentBlock
        id={`${heading}-content`}
        isReordering={isReordering}
        canAddBlocks
        sectionId={id}
        content={content}
        publication={publication}
        onContentChange={newContent => contentChange(newContent)}
        onReorderHook={s => {
          saveOrder.current = s;
        }}
      />
      */}
      {content &&
        sortBy(content, 'order').map((contentBlock, blockIndex) => (
          <div
            key={`content-section-${contentBlock.id}`}
            id={`content-section-${contentBlock.id}`}
            className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
          >
            {!isReordering && (
              <>
                {(isCommenting || isReviewing) && (
                  <Comments
                    contentBlockId={contentBlock.id}
                    initialComments={contentBlock.comments}
                    canResolve={isReviewing}
                    canComment={isCommenting}
                    onCommentsChange={async comments => {
                      const newBlocks = [...content];
                      newBlocks[blockIndex].comments = comments;
                      setContent(newBlocks);
                      if (onContentChange) {
                        onContentChange(newBlocks);
                      }
                    }}
                  />
                )}
              </>
            )}

            <EditableContentSubBlockRenderer
              editable={!isReordering}
              canDelete={!isReordering}
              block={contentBlock}
              onContentChange={onBlockContentChange(contentBlock.id)}
              onDelete={onBlockDelete(contentBlock.id)}
            />
          </div>
        ))}
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;

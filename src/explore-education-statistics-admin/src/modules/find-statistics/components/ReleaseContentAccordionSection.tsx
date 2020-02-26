import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import ContentBlocks, {
  ReorderHook,
} from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { MouseEventHandler, useContext } from 'react';
import { Dictionary } from 'src/types';
import AddContentButton from './AddContentButton';

export interface ReleaseContentAccordionSectionProps {
  id?: string;
  contentItem: ContentType;
  index: number;
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange?: (content?: EditableContentBlock[]) => void;
  onRemoveSection?: EditableAccordionSectionProps['onRemoveSection'];
  canAddBlocks?: boolean;
}

const ReleaseContentAccordionSection = ({
  id: sectionId,
  index,
  contentItem,
  onHeadingChange,
  onContentChange,
  onRemoveSection,
  canAddBlocks = true,
  ...restOfProps
}: ReleaseContentAccordionSectionProps) => {
  const { caption, heading } = contentItem;
  const [isReordering, setIsReordering] = React.useState(false);
  const [content, setContent] = React.useState(contentItem.content);
  const saveOrder = React.useRef<ReorderHook>();
  const {
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

  const onBlockSaveOrder = async (order: Dictionary<number>) => {
    if (releaseId && sectionId) {
      const newBlocks = await releaseContentService.updateContentSectionBlocksOrder(
        releaseId,
        sectionId,
        order,
      );
      if (onContentChange) onContentChange(newBlocks);
    }
  };

  const onBlockContentChange = (blockId: string) => {
    return async (newContent: string) => {
      if (releaseId && sectionId && blockId) {
        const updatedBlock = await releaseContentService.updateContentSectionBlock(
          releaseId,
          sectionId,
          blockId,
          {
            body: newContent,
          },
        );
        if (onContentChange) {
          const newBlocks = [
            ...((contentItem && contentItem.content) || []).map(block => {
              if (block.id === updatedBlock.id) {
                return updatedBlock;
              }
              return block;
            }),
          ];
          onContentChange(newBlocks);
        }
      }
    };
  };

  const onBlockDelete = (blockId: string) => {
    return async () => {
      if (releaseId && sectionId && blockId) {
        await releaseContentService.deleteContentSectionBlock(
          releaseId,
          sectionId,
          blockId,
        );

        if (updateAvailableDataBlocks) updateAvailableDataBlocks();

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
      onHeadingChange={onHeadingChange}
      onRemoveSection={onRemoveSection}
      sectionId={sectionId}
      headerButtons={
        <a
          role="button"
          tabIndex={0}
          onClick={() => setIsReordering(!isReordering)}
          onKeyPress={e => {
            if (e.key === 'Enter') setIsReordering(!isReordering);
          }}
          className={`govuk-button ${!isReordering &&
            'govuk-button--secondary'} govuk-!-margin-right-2`}
        >
          {isReordering ? 'Save order' : 'Reorder'}
        </a>
      }
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
      {...restOfProps}
    >
      <ContentBlocks
        id={`${heading}-content`}
        isReordering={isReordering}
        canAddBlocks
        sectionId={sectionId}
        onContentChange={onContentChange}
        onBlockSaveOrder={onBlockSaveOrder}
        onBlockContentChange={onBlockContentChange}
        onBlockDelete={onBlockDelete}
        content={contentItem.content}
      />
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;

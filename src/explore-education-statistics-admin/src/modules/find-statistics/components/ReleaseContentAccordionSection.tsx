import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import ContentBlocks from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext } from 'react';
import { Dictionary } from 'src/types';
import { ErrorControlContext } from 'src/components/ErrorBoundary';
import AddContentButton from './AddContentButton';

export interface ReleaseContentAccordionSectionProps {
  sectionId: string;
  contentItem: ContentType;
  index: number;
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange: (content?: EditableContentBlock[]) => void;
  onRemoveSection?: EditableAccordionSectionProps['onRemoveSection'];
  canAddBlocks?: boolean;
}

const ReleaseContentAccordionSection = ({
  sectionId,
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
  const {
    availableDataBlocks,
    updateAvailableDataBlocks,
    releaseId,
  } = useContext(EditingContext);
  const { handleApiErrors } = useContext(ErrorControlContext);

  const onBlockSaveOrder = async (order: Dictionary<number>) => {
    if (releaseId && sectionId) {
      const newBlocks = await releaseContentService.updateContentSectionBlocksOrder(
        releaseId,
        sectionId,
        order,
      );
      onContentChange(newBlocks);
    }
  };

  const onBlockContentChange = async (blockId: string, newContent: string) => {
    if (releaseId && sectionId && blockId) {
      const updatedBlock = await releaseContentService.updateContentSectionBlock(
        releaseId,
        sectionId,
        blockId,
        {
          body: newContent,
        },
      );
      {
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

        onContentChange(newContentBlocks);
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
          onContentChange(section.content);
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
              onAddContentCallback(
                type,
                data,
                contentItem.content ? contentItem.content.length : 0,
              )
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
        allowComments
      />
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;

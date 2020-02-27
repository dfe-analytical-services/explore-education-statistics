import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import ContentBlocks from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@admin/types';
import Button from '@common/components/Button';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext } from 'react';
import AddDataBlockButton from './AddDataBlockButton';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  contentItem: ContentType;
  index: number;
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange: (content?: EditableContentBlock[]) => void;
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
  const { updateAvailableDataBlocks, releaseId } = useContext(EditingContext);

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

  const onBlockDelete = async (blockId: string) => {
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

  const onSectionAddTextBlock = async (type: 'MarkdownBlock' | 'HTMLBlock') => {
    if (releaseId && sectionId) {
      await releaseContentService.addContentSectionBlock(releaseId, sectionId, {
        body: '',
        type,
        order: contentItem.content ? contentItem.content.length : undefined,
      });
      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      onContentChange(newContentBlocks);
    }
  };

  const onSectionAddDataBlock = async (datablockId: string) => {
    if (releaseId && sectionId) {
      await releaseContentService.attachContentSectionBlock(
        releaseId,
        sectionId,
        {
          contentBlockId: datablockId,
          order: contentItem.content ? contentItem.content.length : 0,
        },
      );

      if (updateAvailableDataBlocks) updateAvailableDataBlocks();

      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      onContentChange(newContentBlocks);
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

      {!isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-align--center">
          <Button
            variant="secondary"
            onClick={() => {
              onSectionAddTextBlock('MarkdownBlock');
            }}
          >
            Add text block
          </Button>
          <AddDataBlockButton
            onAddDataBlock={dataBlockId => {
              onSectionAddDataBlock(dataBlockId);
            }}
          />
        </div>
      )}
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;

import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import MethodologyEditableBlock from '@admin/pages/methodology/edit-methodology/content/components/MethodologyEditableBlock';
import { EditableContentBlock } from '@admin/services/types/content';
import Button from '@common/components/Button';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, { memo, useCallback, useEffect, useState } from 'react';
import MethodologyBlock from '@admin/pages/methodology/components/MethodologyBlock';
import { ContentSectionKeys } from '../context/MethodologyContextActionTypes';
import useMethodologyActions from '../context/useMethodologyActions';

interface MethodologyAccordionSectionProps {
  id: string;
  section: ContentSection<EditableContentBlock>;
  sectionKey: ContentSectionKeys;
  methodologyId: string;
}

const MethodologyAccordionSection = ({
  sectionKey,
  section: { id: sectionId, caption, heading, content: sectionContent = [] },
  methodologyId,
  ...props
}: MethodologyAccordionSectionProps) => {
  const { editingMode } = useEditingContext();

  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
    updateContentSectionHeading,
    removeContentSection,
  } = useMethodologyActions();

  const [isReordering, setIsReordering] = useState(false);
  const [blocks, setBlocks] = useState<EditableContentBlock[]>(sectionContent);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const addBlockToAccordionSection = useCallback(async () => {
    await addContentSectionBlock({
      methodologyId,
      sectionId,
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
      sectionKey,
    });
  }, [
    addContentSectionBlock,
    methodologyId,
    sectionId,
    sectionContent.length,
    sectionKey,
  ]);

  const updateBlockInAccordionSection = useCallback(
    async (blockId, bodyContent) => {
      await updateContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        bodyContent,
        sectionKey,
      });
    },
    [methodologyId, sectionId, sectionKey, updateContentSectionBlock],
  );

  const removeBlockFromAccordionSection = useCallback(
    (blockId: string) =>
      deleteContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        sectionKey,
      }),
    [deleteContentSectionBlock, methodologyId, sectionId, sectionKey],
  );

  const reorderBlocksInAccordionSection = useCallback(async () => {
    const order = blocks.reduce<Dictionary<number>>((acc, block, newIndex) => {
      acc[block.id] = newIndex;
      return acc;
    }, {});

    await updateSectionBlockOrder({
      methodologyId,
      sectionId,
      order,
      sectionKey,
    });
  }, [blocks, methodologyId, sectionId, sectionKey, updateSectionBlockOrder]);

  const handleHeadingChange = useCallback(
    (newHeading: string) =>
      updateContentSectionHeading({
        methodologyId,
        sectionId,
        heading: newHeading,
        sectionKey,
      }),
    [methodologyId, sectionId, sectionKey, updateContentSectionHeading],
  );

  const handleRemoveSection = useCallback(
    () =>
      removeContentSection({
        methodologyId,
        sectionId,
        sectionKey,
      }),
    [methodologyId, removeContentSection, sectionId, sectionKey],
  );

  return (
    <EditableAccordionSection
      {...props}
      heading={heading}
      caption={caption}
      headerButtons={
        <Button
          variant={!isReordering ? 'secondary' : undefined}
          onClick={async () => {
            if (isReordering) {
              await reorderBlocksInAccordionSection();
              setIsReordering(false);
            } else {
              setIsReordering(true);
            }
          }}
        >
          {isReordering ? 'Save section order' : 'Reorder this section'}
        </Button>
      }
      onHeadingChange={handleHeadingChange}
      onRemoveSection={handleRemoveSection}
    >
      <EditableSectionBlocks<EditableContentBlock>
        blocks={blocks}
        isReordering={isReordering}
        sectionId={sectionId}
        onBlocksChange={setBlocks}
        renderBlock={block => (
          <MethodologyBlock methodologyId={methodologyId} block={block} />
        )}
        renderEditableBlock={(block, index) => (
          <MethodologyEditableBlock
            allowImages
            block={block}
            editable={!isReordering}
            methodologyId={methodologyId}
            onSave={updateBlockInAccordionSection}
            onDelete={removeBlockFromAccordionSection}
            testId={`${heading}-editableContentBlock${index + 1}`}
          />
        )}
      />
      {editingMode === 'edit' && !isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-align--centre">
          <Button
            variant="secondary"
            onClick={addBlockToAccordionSection}
            testId={`${heading}-addTextBlockButton`}
          >
            Add text block
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(MethodologyAccordionSection);

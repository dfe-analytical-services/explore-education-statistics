import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import EducationInNumbersEditableBlock from '@admin/pages/education-in-numbers/content/components/EducationInNumbersEditableBlock';
import EditableSectionBlocks from '@admin/pages/education-in-numbers/content/components/EducationInNumbersEditableSectionBlocks';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import { EinEditableContentSection } from '@admin/services/educationInNumbersContentService';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import Button from '@common/components/Button';
import EinContentBlockRenderer from '@common/modules/education-in-numbers/EinContentBlockRenderer';
import {
  EinBlockType,
  EinContentBlock,
} from '@common/services/types/einBlocks';
import React, { memo, useCallback, useEffect, useRef, useState } from 'react';

interface EducationInNumbersAccordionSectionProps {
  id: string;
  section: EinEditableContentSection;
  educationInNumbersPageId: string;
  educationInNumbersPageSlug: string;
  onRemoveSection: (sectionId: string) => void;
}

const EducationInNumbersAccordionSection = ({
  section: { id: sectionId, caption, heading, content: sectionContent = [] },
  educationInNumbersPageId,
  educationInNumbersPageSlug,
  onRemoveSection,
  ...props
}: EducationInNumbersAccordionSectionProps) => {
  const { editingMode } = useEditingContext();
  const { publicAppUrl } = useConfig();

  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
    updateContentSectionHeading,
  } = useEducationInNumbersPageContentActions();

  const [isReordering, setIsReordering] = useState(false);
  const [blocks, setBlocks] = useState<EinContentBlock[]>(sectionContent);

  const addTextBlockButton = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const addBlockToAccordionSection = useCallback(
    async (blockType: EinBlockType) => {
      const newBlock = await addContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        block: {
          type: blockType,
          order: sectionContent.length,
        },
      });

      focusAddedSectionBlockButton(newBlock.id);
    },
    [
      addContentSectionBlock,
      educationInNumbersPageId,
      sectionId,
      sectionContent.length,
    ],
  );

  const updateBlockInAccordionSection = useCallback(
    async (blockId: string, content: string, type: EinBlockType) => {
      await updateContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        blockId,
        content,
        type,
      });
    },
    [educationInNumbersPageId, sectionId, updateContentSectionBlock],
  );

  const removeBlockFromAccordionSection = useCallback(
    async (blockId: string) => {
      await deleteContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        blockId,
      });

      setTimeout(() => {
        addTextBlockButton.current?.focus();
      }, 100);
    },
    [
      deleteContentSectionBlock,
      addTextBlockButton,
      educationInNumbersPageId,
      sectionId,
    ],
  );

  const reorderBlocksInAccordionSection = useCallback(async () => {
    const order = blocks.map(block => block.id);

    await updateSectionBlockOrder({
      educationInNumbersPageId,
      sectionId,
      order,
    });
  }, [blocks, educationInNumbersPageId, sectionId, updateSectionBlockOrder]);

  const handleHeadingChange = useCallback(
    (newHeading: string) =>
      updateContentSectionHeading({
        educationInNumbersPageId,
        sectionId,
        heading: newHeading,
      }),
    [educationInNumbersPageId, sectionId, updateContentSectionHeading],
  );

  return (
    <EditableAccordionSection
      {...props}
      anchorLinkUrl={
        editingMode === 'preview'
          ? id =>
              `${publicAppUrl}/education-in-numbers/${educationInNumbersPageSlug}#${id}`
          : undefined
      }
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
      onRemoveSection={() => onRemoveSection(sectionId)}
    >
      <EditableSectionBlocks<EinContentBlock>
        blocks={blocks}
        isReordering={isReordering}
        onBlocksChange={setBlocks}
        renderBlock={block => <EinContentBlockRenderer block={block} />}
        renderEditableBlock={block => (
          <EducationInNumbersEditableBlock
            block={block}
            editable={!isReordering}
            onSave={updateBlockInAccordionSection}
            onDelete={removeBlockFromAccordionSection}
          />
        )}
      />
      {editingMode === 'edit' && !isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-flex dfe-gap-3 dfe-justify-content--center">
          <Button
            variant="secondary"
            onClick={() => addBlockToAccordionSection('HtmlBlock')}
            ref={addTextBlockButton}
          >
            Add text block
          </Button>
          <Button
            variant="secondary"
            onClick={() => addBlockToAccordionSection('TileGroupBlock')}
          >
            Add group block
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(EducationInNumbersAccordionSection);

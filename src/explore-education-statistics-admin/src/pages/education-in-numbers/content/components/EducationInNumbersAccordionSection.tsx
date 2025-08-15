import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useConfig } from '@admin/contexts/ConfigContext';
import EducationInNumbersBlock from '@admin/pages/education-in-numbers/components/EducationInNumbersBlock';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import EducationInNumbersEditableBlock from '@admin/pages/education-in-numbers/content/components/EducationInNumbersEditableBlock';
import { EditableContentBlock } from '@admin/services/types/content';
import Button from '@common/components/Button';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import React, { memo, useCallback, useEffect, useRef, useState } from 'react';

interface EducationInNumbersAccordionSectionProps {
  id: string;
  section: ContentSection<EditableContentBlock>;
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
  const [blocks, setBlocks] = useState<EditableContentBlock[]>(sectionContent);

  const addTextBlockButton = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const addBlockToAccordionSection = useCallback(async () => {
    const newBlock = await addContentSectionBlock({
      educationInNumbersPageId,
      sectionId,
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
    });

    focusAddedSectionBlockButton(newBlock.id);
  }, [
    addContentSectionBlock,
    educationInNumbersPageId,
    sectionId,
    sectionContent.length,
  ]);

  const updateBlockInAccordionSection = useCallback(
    async (blockId: string, bodyContent: string) => {
      await updateContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        blockId,
        bodyContent,
      });
    },
    [educationInNumbersPageId, sectionId, updateContentSectionBlock],
  );

  const removeBlockFromAccordionSection = useCallback(
    (blockId: string) => {
      deleteContentSectionBlock({
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
    const order = blocks.reduce<Dictionary<number>>((acc, block, newIndex) => {
      acc[block.id] = newIndex;
      return acc;
    }, {});

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
      <EditableSectionBlocks<EditableContentBlock>
        blocks={blocks}
        isReordering={isReordering}
        onBlocksChange={setBlocks}
        renderBlock={block => <EducationInNumbersBlock block={block} />}
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
        <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
          <Button
            variant="secondary"
            onClick={addBlockToAccordionSection}
            ref={addTextBlockButton}
          >
            Add text block
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(EducationInNumbersAccordionSection);

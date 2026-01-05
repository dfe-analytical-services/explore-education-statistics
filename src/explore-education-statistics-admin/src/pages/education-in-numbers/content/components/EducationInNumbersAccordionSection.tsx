import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import EducationInNumbersEditableBlock from '@admin/pages/education-in-numbers/content/components/EducationInNumbersEditableBlock';
import EditableSectionBlocks from '@admin/pages/education-in-numbers/content/components/EducationInNumbersEditableSectionBlocks';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import { EinEditableContentSection } from '@admin/services/educationInNumbersContentService';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import Button from '@common/components/Button';
import VisuallyHidden from '@common/components/VisuallyHidden';
import EinContentBlockRenderer from '@common/modules/education-in-numbers/components/EinContentBlockRenderer';
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
  section: { id: sectionId, heading, content: sectionContent = [] },
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

  const htmlBlocks = blocks.filter(block => block.type === 'HtmlBlock');
  const groupBlocks = blocks.filter(block => block.type === 'TileGroupBlock');

  const getBlockButtonLabels = (block: EinContentBlock) => {
    if (block.type === 'HtmlBlock') {
      const htmlBlockIndex = htmlBlocks.findIndex(
        htmlBlock => htmlBlock.id === block.id,
      );
      const sectionContext =
        htmlBlocks.length > 1
          ? `${htmlBlockIndex + 1} in ${heading}`
          : `in ${heading}`;
      return {
        editLabel: (
          <>
            Edit<VisuallyHidden> text</VisuallyHidden> block
            <VisuallyHidden> {sectionContext}</VisuallyHidden>
          </>
        ),
        removeLabel: (
          <>
            Remove<VisuallyHidden> text</VisuallyHidden> block
            <VisuallyHidden> {sectionContext}</VisuallyHidden>
          </>
        ),
      };
    }

    const groupBlockIndex = groupBlocks.findIndex(
      groupBlock => groupBlock.id === block.id,
    );
    const sectionContext =
      groupBlocks.length > 1
        ? `group block ${groupBlockIndex + 1} in ${heading}`
        : `group block in ${heading}`;

    return {
      groupButtonsLabel: block.title ?? sectionContext,
      removeLabel: (
        <>
          Remove<VisuallyHidden> group</VisuallyHidden> block
          <VisuallyHidden> {block.title ?? sectionContext}</VisuallyHidden>
        </>
      ),
    };
  };

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
          {isReordering ? (
            'Save section order'
          ) : (
            <>
              Reorder this section
              <VisuallyHidden>{`: ${heading}`}</VisuallyHidden>
            </>
          )}
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
        renderEditableBlock={(block, contentBlockNumber) => {
          const { editLabel, groupButtonsLabel, removeLabel } =
            getBlockButtonLabels(block);
          return (
            <EducationInNumbersEditableBlock
              label={
                !heading
                  ? 'Content block'
                  : `Content block ${contentBlockNumber} for the "${heading}" section`
              }
              sectionId={sectionId}
              block={block}
              editable={!isReordering}
              editButtonLabel={editLabel}
              groupButtonsLabel={groupButtonsLabel}
              removeButtonLabel={removeLabel}
              onSave={updateBlockInAccordionSection}
              onDelete={removeBlockFromAccordionSection}
            />
          );
        }}
      />
      {editingMode === 'edit' && !isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-flex dfe-gap-3 dfe-justify-content--center">
          <Button
            variant="secondary"
            onClick={() => addBlockToAccordionSection('HtmlBlock')}
            ref={addTextBlockButton}
          >
            Add text block<VisuallyHidden>{` to ${heading}`}</VisuallyHidden>
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

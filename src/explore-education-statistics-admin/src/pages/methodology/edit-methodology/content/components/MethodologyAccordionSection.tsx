import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useConfig } from '@admin/contexts/ConfigContext';
import MethodologyEditableBlock from '@admin/pages/methodology/edit-methodology/content/components/MethodologyEditableBlock';
import {
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import Button from '@common/components/Button';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import MethodologyBlock from '@admin/pages/methodology/components/MethodologyBlock';
import { ContentSectionKeys } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContextActionTypes';
import useMethodologyContentActions from '@admin/pages/methodology/edit-methodology/content/context/useMethodologyContentActions';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import React, { memo, useCallback, useEffect, useRef, useState } from 'react';

interface MethodologyAccordionSectionProps {
  id: string;
  section: ContentSection<EditableContentBlock>;
  sectionKey: ContentSectionKeys;
  methodologyId: string;
  methodologySlug: string;
  onRemoveSection: (sectionId: string) => void;
}

const MethodologyAccordionSection = ({
  sectionKey,
  section: { id: sectionId, heading, content: sectionContent = [] },
  methodologyId,
  methodologySlug,
  onRemoveSection,
  ...props
}: MethodologyAccordionSectionProps) => {
  const { editingMode } = useEditingContext();
  const { publicAppUrl } = useConfig();

  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
    updateContentSectionHeading,
  } = useMethodologyContentActions();

  const [isReordering, setIsReordering] = useState(false);
  const [blocks, setBlocks] = useState<EditableContentBlock[]>(sectionContent);

  const addTextBlockButton = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const addBlockToAccordionSection = useCallback(async () => {
    const newBlock = await addContentSectionBlock({
      methodologyId,
      sectionId,
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
      sectionKey,
    });

    focusAddedSectionBlockButton(newBlock.id);
  }, [
    addContentSectionBlock,
    methodologyId,
    sectionId,
    sectionContent.length,
    sectionKey,
  ]);

  const updateBlockInAccordionSection = useCallback(
    async (blockId: string, bodyContent: string) => {
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
    (blockId: string) => {
      deleteContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        sectionKey,
      });

      setTimeout(() => {
        addTextBlockButton.current?.focus();
      }, 100);
    },
    [
      deleteContentSectionBlock,
      addTextBlockButton,
      methodologyId,
      sectionId,
      sectionKey,
    ],
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

  const getBlockButtonLabels = (block: EditableBlock) => {
    const blockIndex = blocks.findIndex(htmlBlock => htmlBlock.id === block.id);
    const sectionContext =
      blocks.length > 1 ? `${blockIndex + 1} in ${heading}` : `in ${heading}`;
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
  };

  return (
    <EditableAccordionSection
      {...props}
      anchorLinkIdPrefix={sectionKey}
      anchorLinkUrl={
        editingMode === 'preview'
          ? id => `${publicAppUrl}/methodology/${methodologySlug}#${id}`
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
      <EditableSectionBlocks<EditableContentBlock>
        blocks={blocks}
        isReordering={isReordering}
        onBlocksChange={setBlocks}
        renderBlock={block => (
          <MethodologyBlock methodologyId={methodologyId} block={block} />
        )}
        renderEditableBlock={(block, contentBlockNumber) => {
          const { editLabel, removeLabel } = getBlockButtonLabels(block);
          return (
            <MethodologyEditableBlock
              allowImages
              block={block}
              editable={!isReordering}
              editButtonLabel={editLabel}
              methodologyId={methodologyId}
              removeButtonLabel={removeLabel}
              onSave={updateBlockInAccordionSection}
              onDelete={removeBlockFromAccordionSection}
              label={
                !heading
                  ? 'Content block'
                  : `Content block ${contentBlockNumber} for the "${heading}" section`
              }
            />
          );
        }}
      />
      {editingMode === 'edit' && !isReordering && (
        <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
          <Button
            variant="secondary"
            onClick={addBlockToAccordionSection}
            ref={addTextBlockButton}
          >
            Add text block<VisuallyHidden>{` to ${heading}`}</VisuallyHidden>
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(MethodologyAccordionSection);

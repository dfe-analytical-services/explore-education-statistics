import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useConfig } from '@admin/contexts/ConfigContext';
import MethodologyEditableBlock from '@admin/pages/methodology/edit-methodology/content/components/MethodologyEditableBlock';
import { EditableContentBlock } from '@admin/services/types/content';
import Button from '@common/components/Button';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import MethodologyBlock from '@admin/pages/methodology/components/MethodologyBlock';
import { ContentSectionKeys } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContextActionTypes';
import useMethodologyContentActions from '@admin/pages/methodology/edit-methodology/content/context/useMethodologyContentActions';
import React, { memo, useCallback, useEffect, useState } from 'react';

interface MethodologyAccordionSectionProps {
  id: string;
  section: ContentSection<EditableContentBlock>;
  sectionKey: ContentSectionKeys;
  methodologyId: string;
  methodologySlug: string;
}

const MethodologyAccordionSection = ({
  sectionKey,
  section: { id: sectionId, caption, heading, content: sectionContent = [] },
  methodologyId,
  methodologySlug,
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
    removeContentSection,
  } = useMethodologyContentActions();

  const [isReordering, setIsReordering] = useState(false);
  const [blocks, setBlocks] = useState<EditableContentBlock[]>(sectionContent);

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

    setTimeout(() => {
      const newBlockEl = document.querySelector(
        `#editableSectionBlocks-${newBlock.id}`,
      );
      const newBlockButton = newBlockEl?.querySelector(
        'button.govuk-button--secondary',
      ) as HTMLButtonElement;
      if (newBlockButton) {
        newBlockButton.focus();
      }
    }, 100);
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
      anchorLinkIdPrefix={sectionKey}
      anchorLinkUrl={
        editingMode === 'preview'
          ? id => `${publicAppUrl}/methodology/${methodologySlug}#${id}`
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
      onRemoveSection={handleRemoveSection}
    >
      <EditableSectionBlocks<EditableContentBlock>
        blocks={blocks}
        isReordering={isReordering}
        onBlocksChange={setBlocks}
        renderBlock={block => (
          <MethodologyBlock methodologyId={methodologyId} block={block} />
        )}
        renderEditableBlock={block => (
          <MethodologyEditableBlock
            allowImages
            block={block}
            editable={!isReordering}
            methodologyId={methodologyId}
            onSave={updateBlockInAccordionSection}
            onDelete={removeBlockFromAccordionSection}
          />
        )}
      />
      {editingMode === 'edit' && !isReordering && (
        <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
          <Button variant="secondary" onClick={addBlockToAccordionSection}>
            Add text block
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(MethodologyAccordionSection);

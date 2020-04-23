import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import {
  EditableBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, { memo, useCallback, useEffect, useState } from 'react';
import AddDataBlockButton from './AddDataBlockButton';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  section: ContentSection<EditableBlock>;
  release: EditableRelease;
}

const ReleaseContentAccordionSection = ({
  release,
  section: { id: sectionId, caption, heading, content: sectionContent = [] },
  ...props
}: ReleaseContentAccordionSectionProps) => {
  const { isEditing } = useEditingContext();

  const {
    addContentSectionBlock,
    attachContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
    updateContentSectionHeading,
    removeContentSection,
  } = useReleaseActions();

  const [isReordering, setIsReordering] = useState(false);
  const [blocks, setBlocks] = useState<EditableBlock[]>(sectionContent);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const getChartFile = useGetChartFile(release.id);

  const addBlockToAccordionSection = useCallback(async () => {
    await addContentSectionBlock({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
    });
  }, [release.id, sectionId, sectionContent.length, addContentSectionBlock]);

  const attachDataBlockToAccordionSection = useCallback(
    async (contentBlockId: string) => {
      await attachContentSectionBlock({
        releaseId: release.id,
        sectionId,
        sectionKey: 'content',
        block: {
          contentBlockId,
          order: sectionContent.length,
        },
      });
    },
    [release.id, sectionId, sectionContent.length, attachContentSectionBlock],
  );

  const updateBlockInAccordionSection = useCallback(
    async (blockId, bodyContent) => {
      await updateContentSectionBlock({
        releaseId: release.id,
        sectionId,
        blockId,
        sectionKey: 'content',
        bodyContent,
      });
    },
    [release.id, sectionId, updateContentSectionBlock],
  );

  const removeBlockFromAccordionSection = useCallback(
    (blockId: string) =>
      deleteContentSectionBlock({
        releaseId: release.id,
        sectionId,
        blockId,
        sectionKey: 'content',
      }),
    [release.id, sectionId, deleteContentSectionBlock],
  );

  const reorderBlocksInAccordionSection = useCallback(async () => {
    const order = blocks.reduce<Dictionary<number>>((acc, block, newIndex) => {
      acc[block.id] = newIndex;
      return acc;
    }, {});

    await updateSectionBlockOrder({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      order,
    });
  }, [blocks, release.id, sectionId, updateSectionBlockOrder]);

  const handleHeadingChange = useCallback(
    title =>
      updateContentSectionHeading({
        sectionId,
        title,
        releaseId: release.id,
      }),
    [release.id, sectionId, updateContentSectionHeading],
  );

  const handleRemoveSection = useCallback(
    () =>
      removeContentSection({
        sectionId,
        releaseId: release.id,
      }),
    [release.id, removeContentSection, sectionId],
  );

  return (
    <EditableAccordionSection
      {...props}
      heading={heading || ''}
      caption={caption}
      onHeadingChange={handleHeadingChange}
      onRemoveSection={handleRemoveSection}
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
    >
      <EditableSectionBlocks
        allowHeadings
        allowComments
        isReordering={isReordering}
        sectionId={sectionId}
        getInfographic={getChartFile}
        content={blocks}
        onBlockContentSave={updateBlockInAccordionSection}
        onBlockDelete={removeBlockFromAccordionSection}
        onBlocksChange={setBlocks}
      />

      {isEditing && !isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-align--centre">
          <Button variant="secondary" onClick={addBlockToAccordionSection}>
            Add text block
          </Button>
          <AddDataBlockButton
            onAddDataBlock={attachDataBlockToAccordionSection}
          />
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(ReleaseContentAccordionSection);

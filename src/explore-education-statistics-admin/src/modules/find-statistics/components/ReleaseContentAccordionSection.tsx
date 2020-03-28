import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import {
  EditableBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, { useCallback, useContext, useState } from 'react';
import AddDataBlockButton from './AddDataBlockButton';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  contentItem: ContentSection<EditableBlock>;
  index: number;
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onRemoveSection?: EditableAccordionSectionProps['onRemoveSection'];
  canAddBlocks?: boolean;
  release: EditableRelease;
}

const ReleaseContentAccordionSection = ({
  release,
  id: sectionId,
  index,
  contentItem,
  onHeadingChange,
  onRemoveSection,
  canAddBlocks = true,
  ...restOfProps
}: ReleaseContentAccordionSectionProps) => {
  const { isEditing } = useContext(EditingContext);
  const { caption, heading, content: sectionContent = [] } = contentItem;
  const [isReordering, setIsReordering] = useState(false);
  const {
    addContentSectionBlock,
    attachContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
  } = useReleaseActions();

  const [blocks, setBlocks] = useState<EditableBlock[]>(sectionContent);

  const getChartFile = useGetChartFile(release.id);

  const addBlockToAccordionSection = useCallback(async () => {
    await addContentSectionBlock({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'MarkDownBlock',
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

  return (
    <AccordionSection
      {...restOfProps}
      id={sectionId}
      index={index}
      heading={heading || ''}
      caption={caption}
      onHeadingChange={onHeadingChange}
      onRemoveSection={onRemoveSection}
      sectionId={sectionId}
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

      {isEditing && !isReordering && canAddBlocks && (
        <div className="govuk-!-margin-bottom-8 dfe-align--center">
          <Button variant="secondary" onClick={addBlockToAccordionSection}>
            Add text block
          </Button>
          <AddDataBlockButton
            onAddDataBlock={attachDataBlockToAccordionSection}
          />
        </div>
      )}
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;

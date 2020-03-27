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
  const { caption, heading } = contentItem;
  const { content: sectionContent = [] } = contentItem;
  const [isReordering, setIsReordering] = useState(false);
  const {
    addContentSectionBlock,
    attachContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
  } = useReleaseActions();

  const getChartFile = useGetChartFile(release.id);

  const addBlockToAccordionSection = useCallback(() => {
    addContentSectionBlock({
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
    (contentBlockId: string) => {
      attachContentSectionBlock({
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
    (blockId, bodyContent) => {
      updateContentSectionBlock({
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

  const reorderBlocksInAccordionSection = useCallback(
    order => {
      updateSectionBlockOrder({
        releaseId: release.id,
        sectionId,
        sectionKey: 'content',
        order,
      });
    },
    [release.id, sectionId, updateSectionBlockOrder],
  );

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
        <Button
          onClick={() => setIsReordering(!isReordering)}
          className={`govuk-button ${!isReordering &&
            'govuk-button--secondary'}`}
        >
          {isReordering ? 'Save section order' : 'Reorder this section'}
        </Button>
      }
      {...restOfProps}
    >
      <EditableSectionBlocks
        isReordering={isReordering}
        sectionId={sectionId}
        getInfographic={getChartFile}
        onBlockSaveOrder={reorderBlocksInAccordionSection}
        onBlockContentChange={updateBlockInAccordionSection}
        onBlockDelete={removeBlockFromAccordionSection}
        content={sectionContent}
        allowComments
        allowHeadings
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

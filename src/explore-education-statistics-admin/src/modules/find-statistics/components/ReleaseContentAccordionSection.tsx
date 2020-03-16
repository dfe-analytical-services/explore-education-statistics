import ContentBlocks from '@admin/components/editable/EditableContentBlocks';
import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import React, { useCallback, useState } from 'react';
import AddDataBlockButton from './AddDataBlockButton';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  contentItem: ContentType;
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

  const addBlockToAccordionSection = useCallback(() => {
    addContentSectionBlock({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'MarkdownBlock',
        order: sectionContent.length,
        body: '',
      },
    });
  }, [release.id, sectionId, sectionContent.length, addContentSectionBlock]);

  const attachDataBlockToAccordionSection = useCallback(
    (datablockId: string) => {
      attachContentSectionBlock({
        releaseId: release.id,
        sectionId,
        sectionKey: 'content',
        block: {
          contentBlockId: datablockId,
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
          {isReordering ? 'Save order' : 'Reorder'}
        </Button>
      }
      {...restOfProps}
    >
      <ContentBlocks
        id={`${heading}-content`}
        isReordering={isReordering}
        sectionId={sectionId}
        onBlockSaveOrder={reorderBlocksInAccordionSection}
        onBlockContentChange={updateBlockInAccordionSection}
        onBlockDelete={removeBlockFromAccordionSection}
        content={sectionContent}
        allowComments
        insideAccordion
      />

      {!isReordering && canAddBlocks && (
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

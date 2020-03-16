import ContentBlocks from '@admin/components/editable/EditableContentBlocks';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import { ContentSection } from '@common/services/publicationService';
import React, { useCallback, useState } from 'react';
import useMethodologyActions from '../context/useMethodologyActions';

interface MethodologyAccordionSectionProps {
  content: ContentSection<EditableContentBlock>;
  isAnnex?: boolean;
  methodologyId: string;
}

const MethodologyAccordionSection = ({
  isAnnex = false,
  content,
  methodologyId,
}: MethodologyAccordionSectionProps) => {
  const { caption, heading, id: sectionId } = content;
  const { content: sectionContent = [] } = content;
  const [isReordering, setIsReordering] = useState(false);

  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
  } = useMethodologyActions();

  const addBlockToAccordionSection = useCallback(() => {
    addContentSectionBlock({
      methodologyId,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'MarkdownBlock',
        order: sectionContent.length,
        body: '',
      },
      isAnnex,
    });
  }, [methodologyId, sectionId, sectionContent.length, addContentSectionBlock]);

  const updateBlockInAccordionSection = useCallback(
    (blockId, bodyContent) => {
      updateContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        bodyContent,
        isAnnex,
      });
    },
    [methodologyId, sectionId, updateContentSectionBlock],
  );

  const removeBlockFromAccordionSection = useCallback(
    (blockId: string) =>
      deleteContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        isAnnex,
      }),
    [methodologyId, sectionId, deleteContentSectionBlock],
  );

  const reorderBlocksInAccordionSection = useCallback(
    order => {
      updateSectionBlockOrder({
        methodologyId,
        sectionId,
        order,
        isAnnex,
      });
    },
    [methodologyId, sectionId, updateSectionBlockOrder],
  );

  return (
    <EditableAccordionSection
      heading={heading}
      caption={caption}
      isReordering={isReordering}
      headerButtons={
        <Button
          onClick={() => setIsReordering(!isReordering)}
          className={`govuk-button ${!isReordering &&
            'govuk-button--secondary'}`}
        >
          {isReordering ? 'Save order' : 'Reorder'}
        </Button>
      }
      {...content}
    >
      <ContentBlocks
        id={`${heading}-content`}
        isReordering={isReordering}
        sectionId={sectionId}
        onBlockSaveOrder={reorderBlocksInAccordionSection}
        onBlockContentChange={updateBlockInAccordionSection}
        onBlockDelete={removeBlockFromAccordionSection}
        content={sectionContent}
      />
      {!isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-align--center">
          <Button variant="secondary" onClick={addBlockToAccordionSection}>
            Add text block
          </Button>
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default MethodologyAccordionSection;

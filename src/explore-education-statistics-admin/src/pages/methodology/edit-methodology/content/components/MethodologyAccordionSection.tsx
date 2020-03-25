import ContentBlocks from '@admin/components/editable/EditableContentBlocks';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { ContentSection } from '@common/services/publicationService';
import React, { useCallback, useContext, useState } from 'react';
import useMethodologyActions from '../context/useMethodologyActions';
import { ContentSectionKeys } from '../context/MethodologyContextActionTypes';

interface MethodologyAccordionSectionProps {
  content: ContentSection<EditableContentBlock>;
  sectionKey: ContentSectionKeys;
  methodologyId: string;
  index: number;
  id: string;
}

const MethodologyAccordionSection = ({
  sectionKey,
  content,
  methodologyId,
  ...restOfProps
}: MethodologyAccordionSectionProps) => {
  const { isEditing } = useContext(EditingContext);
  const { caption, heading, id: sectionId } = content;
  const { content: sectionContent = [] } = content;
  const [isReordering, setIsReordering] = useState(false);

  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    updateSectionBlockOrder,
    updateContentSectionHeading,
    removeContentSection,
  } = useMethodologyActions();

  const addBlockToAccordionSection = useCallback(() => {
    addContentSectionBlock({
      methodologyId,
      sectionId,
      block: {
        type: 'MarkdownBlock',
        order: sectionContent.length,
        body: '',
      },
      sectionKey,
    });
  }, [methodologyId, sectionId, sectionContent.length, addContentSectionBlock]);

  const updateBlockInAccordionSection = useCallback(
    (blockId, bodyContent) => {
      updateContentSectionBlock({
        methodologyId,
        sectionId,
        blockId,
        bodyContent,
        sectionKey,
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
        sectionKey,
      }),
    [methodologyId, sectionId, deleteContentSectionBlock],
  );

  const reorderBlocksInAccordionSection = useCallback(
    order => {
      updateSectionBlockOrder({
        methodologyId,
        sectionId,
        order,
        sectionKey,
      });
    },
    [methodologyId, sectionId, updateSectionBlockOrder],
  );

  const onSaveHeading = useCallback(
    (newHeading: string) =>
      updateContentSectionHeading({
        methodologyId,
        sectionId,
        heading: newHeading,
        sectionKey,
      }),
    [methodologyId, sectionId, updateContentSectionHeading],
  );

  const removeSection = useCallback(
    () =>
      removeContentSection({
        methodologyId,
        sectionId,
        sectionKey,
      }),
    [],
  );

  return (
    <EditableAccordionSection
      {...content}
      {...restOfProps}
      heading={heading}
      caption={caption}
      isReordering={isReordering}
      headerButtons={
        <Button
          onClick={() => setIsReordering(!isReordering)}
          className={`govuk-button ${!isReordering &&
            'govuk-button--secondary'}`}
        >
          {isReordering ? 'Save section order' : 'Reorder this section'}
        </Button>
      }
      onHeadingChange={onSaveHeading}
      onRemoveSection={removeSection}
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
      {isEditing && !isReordering && (
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

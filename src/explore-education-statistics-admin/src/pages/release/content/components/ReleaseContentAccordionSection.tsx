import { CommentsChangeHandler } from '@admin/components/editable/Comments';
import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import DataBlockSelectForm from '@admin/pages/release/content/components/DataBlockSelectForm';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import useToggle from '@common/hooks/useToggle';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, { memo, useCallback, useEffect, useState } from 'react';

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

  const actions = useReleaseContentActions();

  const [isReordering, setIsReordering] = useState(false);
  const [showDataBlockForm, toggleDataBlockForm] = useToggle(false);

  const [blocks, setBlocks] = useState<EditableBlock[]>(sectionContent);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const getChartFile = useGetChartFile(release.id);

  const addBlock = useCallback(async () => {
    await actions.addContentSectionBlock({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
    });
  }, [actions, release.id, sectionId, sectionContent.length]);

  const attachDataBlock = useCallback(
    async (contentBlockId: string) => {
      await actions.attachContentSectionBlock({
        releaseId: release.id,
        sectionId,
        sectionKey: 'content',
        block: {
          contentBlockId,
          order: sectionContent.length,
        },
      });
    },
    [actions, release.id, sectionId, sectionContent.length],
  );

  const updateBlock = useCallback(
    async (blockId: string, bodyContent: string) => {
      await actions.updateContentSectionBlock({
        releaseId: release.id,
        sectionId,
        blockId,
        sectionKey: 'content',
        bodyContent,
      });
    },
    [actions, release.id, sectionId],
  );

  const removeBlock = useCallback(
    async (blockId: string) => {
      await actions.deleteContentSectionBlock({
        releaseId: release.id,
        sectionId,
        blockId,
        sectionKey: 'content',
      });
    },
    [actions, release.id, sectionId],
  );

  const reorderBlocks = useCallback(async () => {
    const order = blocks.reduce<Dictionary<number>>((acc, block, newIndex) => {
      acc[block.id] = newIndex;
      return acc;
    }, {});

    await actions.updateSectionBlockOrder({
      releaseId: release.id,
      sectionId,
      sectionKey: 'content',
      order,
    });
  }, [blocks, actions, release.id, sectionId]);

  const handleHeadingChange = useCallback(
    async (title: string) => {
      await actions.updateContentSectionHeading({
        sectionId,
        title,
        releaseId: release.id,
      });
    },
    [actions, sectionId, release.id],
  );

  const handleRemoveSection = useCallback(async () => {
    await actions.removeContentSection({
      sectionId,
      releaseId: release.id,
    });
  }, [actions, sectionId, release.id]);

  const updateBlockComments: CommentsChangeHandler = useCallback(
    async (blockId, comments) => {
      await actions.updateBlockComments({
        sectionId,
        blockId,
        sectionKey: 'content',
        comments,
      });
    },
    [actions, sectionId],
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
              await reorderBlocks();
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
        releaseId={release.id}
        allowHeadings
        allowComments
        isReordering={isReordering}
        sectionId={sectionId}
        getInfographic={getChartFile}
        content={blocks}
        onBlockContentSave={updateBlock}
        onBlockDelete={removeBlock}
        onBlocksChange={setBlocks}
        onBlockCommentsChange={updateBlockComments}
      />

      {isEditing && !isReordering && (
        <>
          {showDataBlockForm && (
            <DataBlockSelectForm
              releaseId={release.id}
              onSelect={async selectedDataBlockId => {
                await attachDataBlock(selectedDataBlockId);
                toggleDataBlockForm.off();
              }}
              onCancel={toggleDataBlockForm.off}
            />
          )}

          <ButtonGroup className="govuk-!-margin-bottom-8 dfe-justify-content--center">
            <Button variant="secondary" onClick={addBlock}>
              Add text block
            </Button>
            {!showDataBlockForm && (
              <Button variant="secondary" onClick={toggleDataBlockForm.on}>
                Add data block
              </Button>
            )}
          </ButtonGroup>
        </>
      )}
    </EditableAccordionSection>
  );
};

export default memo(ReleaseContentAccordionSection);

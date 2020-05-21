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
import { CommentsChangeHandler } from './Comments';

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

  const actions = useReleaseActions();

  const [isReordering, setIsReordering] = useState(false);
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
    async (blockId, bodyContent) => {
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
    (blockId: string) =>
      actions.deleteContentSectionBlock({
        releaseId: release.id,
        sectionId,
        blockId,
        sectionKey: 'content',
      }),
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
    (title: string) =>
      actions.updateContentSectionHeading({
        sectionId,
        title,
        releaseId: release.id,
      }),
    [actions, sectionId, release.id],
  );

  const handleRemoveSection = useCallback(
    () =>
      actions.removeContentSection({
        sectionId,
        releaseId: release.id,
      }),
    [actions, sectionId, release.id],
  );

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
        <div className="govuk-!-margin-bottom-8 dfe-align--centre">
          <Button variant="secondary" onClick={addBlock}>
            Add text block
          </Button>
          <AddDataBlockButton onAddDataBlock={attachDataBlock} />
        </div>
      )}
    </EditableAccordionSection>
  );
};

export default memo(ReleaseContentAccordionSection);

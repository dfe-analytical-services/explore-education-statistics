import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import KeyStatistics from '@admin/pages/release/content/components/KeyStatistics';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useCallback } from 'react';
import AddSecondaryStats from './AddSecondaryStats';

interface Props {
  release: EditableRelease;
}

const ReleaseHeadlines = ({ release }: Props) => {
  const { editingMode } = useEditingContext();
  const actions = useReleaseContentActions();

  const getChartFile = useGetChartFile(release.id);

  const addBlock = useCallback(async () => {
    await actions.addContentSectionBlock({
      releaseId: release.id,
      sectionId: release.headlinesSection.id,
      sectionKey: 'headlinesSection',
      block: {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    });
  }, [actions, release.id, release.headlinesSection.id]);

  const updateBlock = useCallback(
    async (blockId, bodyContent) => {
      await actions.updateContentSectionBlock({
        releaseId: release.id,
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
        bodyContent,
      });
    },
    [actions, release.id, release.headlinesSection.id],
  );

  const removeBlock = useCallback(
    async (blockId: string) => {
      await actions.deleteContentSectionBlock({
        releaseId: release.id,
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
      });
    },
    [actions, release.id, release.headlinesSection.id],
  );

  const updateBlockComments = useCallback(
    async (blockId: string, comments: Comment[]) => {
      await actions.updateBlockComments({
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
        comments,
      });
    },
    [actions, release.headlinesSection.id],
  );

  const updateCommentsPendingDeletion = useCallback(
    async (blockId, commentId) => {
      await actions.setCommentsPendingDeletion({ blockId, commentId });
    },
    [actions],
  );

  const headlinesTab = (
    <TabsSection title="Headlines">
      <section id="releaseHeadlines-keyStatistics">
        <KeyStatistics release={release} isEditing={editingMode === 'edit'} />
      </section>
      <section id="releaseHeadlines-headlines">
        <EditableSectionBlocks
          blocks={release.headlinesSection.content}
          sectionId={release.headlinesSection.id}
          renderBlock={block => (
            <ReleaseBlock block={block} releaseId={release.id} />
          )}
          renderEditableBlock={block => (
            <ReleaseEditableBlock
              allowComments
              block={block}
              sectionId={release.headlinesSection.id}
              releaseId={release.id}
              onBlockCommentsChange={updateBlockComments}
              onCommentsPendingDeletionChange={updateCommentsPendingDeletion}
              onSave={updateBlock}
              onDelete={removeBlock}
            />
          )}
        />

        {editingMode === 'edit' &&
          release.headlinesSection.content?.length === 0 && (
            <div className="govuk-!-margin-bottom-8 dfe-align--centre">
              <Button variant="secondary" onClick={addBlock}>
                Add a headlines text block
              </Button>
            </div>
          )}
      </section>
    </TabsSection>
  );

  return (
    <section id="releaseHeadlines">
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      {release.keyStatisticsSecondarySection.content?.length ? (
        <>
          <AddSecondaryStats release={release} updating />
          <DataBlockTabs
            releaseId={release.id}
            id="releaseHeadlines-dataBlock"
            dataBlock={release.keyStatisticsSecondarySection.content[0]}
            getInfographic={getChartFile}
            firstTabs={headlinesTab}
          />
        </>
      ) : (
        <>
          <AddSecondaryStats release={release} />
          <Tabs id="releaseHeadlines-dataBlock">{headlinesTab}</Tabs>
        </>
      )}
    </section>
  );
};

export default ReleaseHeadlines;

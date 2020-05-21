import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import TabsSection from '@common/components/TabsSection';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';
import React, { useCallback } from 'react';
import AddSecondaryStats from './AddSecondaryStats';
import KeyStatistics from './KeyStatistics';
import { CommentsChangeHandler } from './Comments';

interface Props {
  release: EditableRelease;
}

const ReleaseHeadlines = ({ release }: Props) => {
  const { isEditing } = useEditingContext();
  const actions = useReleaseActions();

  const getChartFile = useGetChartFile(release.id);

  const addBlock = useCallback(() => {
    actions.addContentSectionBlock({
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
    (blockId, bodyContent) => {
      actions.updateContentSectionBlock({
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
    (blockId: string) => {
      actions.deleteContentSectionBlock({
        releaseId: release.id,
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
      });
    },
    [actions, release.id, release.headlinesSection.id],
  );

  const updateBlockComments: CommentsChangeHandler = useCallback(
    async (blockId, comments) => {
      await actions.updateBlockComments({
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
        comments,
      });
    },
    [actions, release.headlinesSection.id],
  );

  return (
    <section id="releaseHeadlines">
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      {release.keyStatisticsSecondarySection?.content?.length ? (
        <AddSecondaryStats release={release} isEditing={isEditing} updating />
      ) : (
        <AddSecondaryStats release={release} isEditing={isEditing} />
      )}

      <DataBlockRenderer
        id="releaseHeadlines-dataBlock"
        dataBlock={release.keyStatisticsSecondarySection.content[0]}
        getInfographic={getChartFile}
        firstTabs={
          <TabsSection title="Headlines">
            <section id="releaseHeadlines-keyStatistics">
              <KeyStatistics release={release} isEditing={isEditing} />
            </section>
            <section id="releaseHeadlines-headlines">
              <EditableSectionBlocks
                allowComments
                sectionId={release.headlinesSection.id}
                content={release.headlinesSection.content}
                onBlockContentSave={updateBlock}
                onBlockDelete={removeBlock}
                onBlockCommentsChange={updateBlockComments}
              />

              {isEditing && release.headlinesSection.content?.length === 0 && (
                <div className="govuk-!-margin-bottom-8 dfe-align--centre">
                  <Button variant="secondary" onClick={addBlock}>
                    Add a headlines text block
                  </Button>
                </div>
              )}
            </section>
          </TabsSection>
        }
      />
    </section>
  );
};

export default ReleaseHeadlines;

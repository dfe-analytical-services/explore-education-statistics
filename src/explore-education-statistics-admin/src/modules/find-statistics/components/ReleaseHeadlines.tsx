import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/publicationService';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import Button from '@common/components/Button';
import TabsSection from '@common/components/TabsSection';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useCallback, useContext } from 'react';
import AddSecondaryStats from './AddSecondaryStats';
import KeyStatistics from './KeyStatistics';

interface Props {
  release: EditableRelease;
}

const ReleaseHeadlines = ({ release }: Props) => {
  const { isEditing } = useContext(EditingContext);
  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
  } = useReleaseActions();

  const addHeadlinesBlock = useCallback(() => {
    addContentSectionBlock({
      releaseId: release.id,
      sectionId: release.headlinesSection.id,
      sectionKey: 'headlinesSection',
      block: {
        type: 'MarkDownBlock',
        order: 0,
        body: '',
      },
    });
  }, [release.id, release.headlinesSection.id, addContentSectionBlock]);

  const headlinesBlockUpdate = useCallback(
    (blockId, bodyContent) => {
      updateContentSectionBlock({
        releaseId: release.id,
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
        bodyContent,
      });
    },
    [release.id, release.headlinesSection.id, updateContentSectionBlock],
  );

  const headlinesBlockDelete = useCallback(
    (blockId: string) => {
      deleteContentSectionBlock({
        releaseId: release.id,
        sectionId: release.headlinesSection.id,
        blockId,
        sectionKey: 'headlinesSection',
      });
    },
    [release.id, release.headlinesSection.id, deleteContentSectionBlock],
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

      <DataBlockTabs
        id="releaseHeadlines-tabs"
        dataBlock={release.keyStatisticsSecondarySection.content[0]}
        releaseId={release.id}
        getInfographic={editReleaseDataService.downloadChartFile}
        firstTabs={
          <TabsSection title="Headlines">
            <section id="releaseHeadlines-keyStatistics">
              <KeyStatistics release={release} isEditing={isEditing} />
            </section>
            <section id="releaseHeadlines-headlines">
              <EditableSectionBlocks
                sectionId={release.headlinesSection.id}
                content={release.headlinesSection.content}
                onBlockContentChange={headlinesBlockUpdate}
                onBlockDelete={headlinesBlockDelete}
                allowComments
              />

              {release.headlinesSection.content?.length === 0 && (
                <div className="govuk-!-margin-bottom-8 dfe-align--center">
                  <Button variant="secondary" onClick={addHeadlinesBlock}>
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

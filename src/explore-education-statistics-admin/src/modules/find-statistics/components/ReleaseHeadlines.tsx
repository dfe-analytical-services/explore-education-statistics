import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import {
  EditableBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import TabsSection from '@common/components/TabsSection';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';
import React, { useCallback, useState } from 'react';
import AddSecondaryStats from './AddSecondaryStats';
import KeyStatistics from './KeyStatistics';

interface Props {
  release: EditableRelease;
}

const ReleaseHeadlines = ({ release }: Props) => {
  const { isEditing } = useEditingContext();
  const {
    addContentSectionBlock,
    deleteContentSectionBlock,
    updateContentSectionBlock,
  } = useReleaseActions();

  const [headlineBlocks, setHeadlineBlocks] = useState<EditableBlock[]>(
    release.headlinesSection.content,
  );

  const getChartFile = useGetChartFile(release.id);

  const addHeadlinesBlock = useCallback(() => {
    addContentSectionBlock({
      releaseId: release.id,
      sectionId: release.headlinesSection.id,
      sectionKey: 'headlinesSection',
      block: {
        type: 'HtmlBlock',
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
                content={headlineBlocks}
                onBlockContentSave={headlinesBlockUpdate}
                onBlockDelete={headlinesBlockDelete}
                onBlocksChange={setHeadlineBlocks}
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

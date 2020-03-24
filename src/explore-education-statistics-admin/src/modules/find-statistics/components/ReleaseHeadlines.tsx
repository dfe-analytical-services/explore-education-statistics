import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { TableDataQuery } from '@common/modules/table-tool/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/tableHeaders';
import dataBlockService, {
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { DataBlock } from '@common/services/types/blocks';
import React, { useCallback, useContext, useEffect, useState } from 'react';
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

  const [secondaryStatsDataBlock, setSecondaryStatsDataBlockData] = useState<{
    dataBlock: DataBlock;
    data: FullTable;
    chartProps: ChartRendererProps | undefined;
  }>();

  useEffect(() => {
    setSecondaryStatsDataBlockData(undefined);
    if (release.keyStatisticsSecondarySection?.content?.length) {
      const secondaryDataBlock =
        release.keyStatisticsSecondarySection.content[0];

      dataBlockService
        .getDataBlockForSubject(
          secondaryDataBlock.dataBlockRequest as TableDataQuery,
        )
        .then(data => {
          let chartProps;
          if (
            data &&
            secondaryDataBlock.charts &&
            secondaryDataBlock.charts[0]
          ) {
            chartProps = {
              data,
              meta: parseMetaData(data.metaData),
              ...secondaryDataBlock.charts[0],
            };
          }
          setSecondaryStatsDataBlockData({
            dataBlock: secondaryDataBlock as DataBlock,
            data: mapDataBlockResponseToFullTable(data as DataBlockResponse),
            chartProps: chartProps as ChartRendererProps,
          });
        });
    }
  }, [release.keyStatisticsSecondarySection]);

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
    <section id="headlines">
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      {release.keyStatisticsSecondarySection?.content?.length ? (
        <AddSecondaryStats release={release} isEditing={isEditing} updating />
      ) : (
        <AddSecondaryStats release={release} isEditing={isEditing} />
      )}

      <Tabs id="releaseHeadlingsTabs">
        <TabsSection id="headline-headlines" title="Headlines">
          <section id="keystats">
            {release.keyStatisticsSection && (
              <KeyStatistics release={release} isEditing={isEditing} />
            )}
          </section>
          <section id="headlines">
            <EditableSectionBlocks
              sectionId={release.headlinesSection.id}
              publication={release.publication}
              id={release.headlinesSection.id}
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
        {release.keyStatisticsSecondarySection?.content?.length && [
          <TabsSection key="table" id="headline-secondary-table" title="Table">
            {secondaryStatsDataBlock ? (
              <TimePeriodDataTable
                fullTable={secondaryStatsDataBlock.data}
                tableHeadersConfig={
                  secondaryStatsDataBlock?.dataBlock.tables?.[0]?.tableHeaders
                    ? mapTableHeadersConfig(
                        secondaryStatsDataBlock?.dataBlock.tables?.[0]
                          ?.tableHeaders,
                        secondaryStatsDataBlock.data.subjectMeta,
                      )
                    : getDefaultTableHeaderConfig(
                        secondaryStatsDataBlock.data.subjectMeta,
                      )
                }
              />
            ) : (
              <LoadingSpinner text="Loading secondary statistics" />
            )}
          </TabsSection>,
          release.keyStatisticsSecondarySection.content?.[0]?.charts.length && (
            <TabsSection
              key="chart"
              id="headline-secondary-chart"
              title="Chart"
            >
              {secondaryStatsDataBlock && secondaryStatsDataBlock.chartProps ? (
                <ChartRenderer {...secondaryStatsDataBlock.chartProps} />
              ) : (
                <LoadingSpinner text="Loading secondary statistics" />
              )}
            </TabsSection>
          ),
        ]}
      </Tabs>
    </section>
  );
};

export default ReleaseHeadlines;

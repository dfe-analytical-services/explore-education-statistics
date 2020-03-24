import ContentBlocks from '@admin/components/editable/EditableContentBlocks';
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
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext, useEffect, useState, useCallback } from 'react';
import KeyStatistics from './KeyStatistics';
import { AddSecondaryStats, hasSecondaryStats } from './KeyStatisticsSecondary';

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

  const [secondaryStatsDatablock, setSecondaryStatsDatablockData] = useState<{
    datablock: DataBlock;
    data: FullTable;
    chartProps: ChartRendererProps | undefined;
  }>();

  useEffect(() => {
    setSecondaryStatsDatablockData(undefined);
    if (hasSecondaryStats(release.keyStatisticsSecondarySection)) {
      const secondaryDatablock =
        // @ts-ignore above if statement ensures it is defined
        release.keyStatisticsSecondarySection.content[0];
      DataBlockService.getDataBlockForSubject(
        secondaryDatablock.dataBlockRequest as TableDataQuery,
      ).then(data => {
        let chartProps;
        if (data && secondaryDatablock.charts && secondaryDatablock.charts[0]) {
          chartProps = {
            data,
            meta: parseMetaData(data.metaData),
            ...secondaryDatablock.charts[0],
          };
        }
        setSecondaryStatsDatablockData({
          datablock: secondaryDatablock as DataBlock,
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
        type: 'MarkdownBlock',
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

      {hasSecondaryStats(release.keyStatisticsSecondarySection) ? (
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
            <ContentBlocks
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
        {release.keyStatisticsSecondarySection &&
          release.keyStatisticsSecondarySection.content &&
          release.keyStatisticsSecondarySection.content.length && [
            <TabsSection
              key="table"
              id="headline-secondary-table"
              title="Table"
            >
              {secondaryStatsDatablock ? (
                <TimePeriodDataTable
                  fullTable={secondaryStatsDatablock.data}
                  tableHeadersConfig={
                    secondaryStatsDatablock.datablock.tables?.[0]?.tableHeaders
                      ? mapTableHeadersConfig(
                          secondaryStatsDatablock.datablock.tables?.[0]
                            ?.tableHeaders,
                          secondaryStatsDatablock.data.subjectMeta,
                        )
                      : getDefaultTableHeaderConfig(
                          secondaryStatsDatablock.data.subjectMeta,
                        )
                  }
                />
              ) : (
                <LoadingSpinner text="Loading secondary statistics" />
              )}
            </TabsSection>,
            release.keyStatisticsSecondarySection.content &&
              release.keyStatisticsSecondarySection.content[0] &&
              release.keyStatisticsSecondarySection.content[0].charts &&
              release.keyStatisticsSecondarySection.content[0].charts
                .length && (
                <TabsSection
                  key="chart"
                  id="headline-secondary-chart"
                  title="Chart"
                >
                  {secondaryStatsDatablock &&
                  secondaryStatsDatablock.chartProps ? (
                    <ChartRenderer {...secondaryStatsDatablock.chartProps} />
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

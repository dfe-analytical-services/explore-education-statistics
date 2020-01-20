import ContentBlocks from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { EditableContentBlock } from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import { parseMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { TableDataQuery } from '@common/modules/full-table/services/tableBuilderService';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import React, { useContext, useEffect, useState } from 'react';
import { AddSecondaryStats, hasSecondaryStats } from './KeyStatisticsSecondary';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
}

const ReleaseHeadlines = ({ release, setRelease = () => {} }: Props) => {
  const { isEditing } = useContext(EditingContext);
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

  return (
    <section id="headlines">
      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      {hasSecondaryStats(release.keyStatisticsSecondarySection) ? (
        <AddSecondaryStats
          release={release}
          setRelease={setRelease}
          isEditing={isEditing}
          updating
        />
      ) : (
        <AddSecondaryStats
          release={release}
          setRelease={setRelease}
          isEditing={isEditing}
        />
      )}

      <Tabs id="releaseHeadlingsTabs">
        <TabsSection id="headline-summary" title="Summary">
          <section id="headlines">
            {release.headlinesSection && (
              <ContentBlocks
                sectionId={release.headlinesSection.id}
                publication={release.publication}
                id={release.headlinesSection.id as string}
                content={release.headlinesSection.content}
                canAddSingleBlock
                textOnly
                onContentChange={newContent =>
                  setRelease({
                    ...release,
                    headlinesSection: {
                      ...release.headlinesSection,
                      content: newContent,
                    },
                  })
                }
              />
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
                    (secondaryStatsDatablock.datablock.tables &&
                      secondaryStatsDatablock.datablock.tables[0] &&
                      secondaryStatsDatablock.datablock.tables[0]
                        .tableHeaders) ||
                    getDefaultTableHeaderConfig(
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

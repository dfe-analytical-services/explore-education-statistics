import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { AxesConfiguration } from '@common/modules/charts/types/chart';
import getLabelDataSetConfigurations from '@common/modules/charts/util/getLabelDataSetConfigurations';
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { DataBlock } from '@common/services/types/blocks';
import React, { MouseEvent, ReactNode } from 'react';

export interface DataBlockRendererProps {
  releaseId?: string;
  additionalTabContent?: ReactNode;
  dataBlock?: DataBlock;
  firstTabs?: ReactNode;
  lastTabs?: ReactNode;
  getInfographic?: GetInfographic;
  id: string;
  onToggle?: (section: { id: string; title: string }) => void;
  onSummaryDetailsToggle?: (
    isOpened: boolean,
    event: MouseEvent<HTMLElement>,
  ) => void;
}

const DataBlockRenderer = ({
  releaseId,
  additionalTabContent,
  dataBlock,
  firstTabs,
  lastTabs,
  getInfographic,
  id,
  onToggle,
}: DataBlockRendererProps) => {
  const { value: fullTable, isLoading } = useTableQuery(
    dataBlock
      ? {
          ...dataBlock.dataBlockRequest,
          includeGeoJson: dataBlock.charts.some(chart => chart.type === 'map'),
        }
      : undefined,
    releaseId || undefined,
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs id={id} onToggle={onToggle}>
        {firstTabs}

        {dataBlock?.tables?.length && fullTable && (
          <TabsSection id={`${id}-tables`} title="Table">
            {dataBlock?.tables.map((table, index) => {
              return (
                <TimePeriodDataTable
                  key={index}
                  fullTable={fullTable}
                  captionTitle={dataBlock?.heading}
                  source={dataBlock?.source}
                  tableHeadersConfig={
                    table.tableHeaders
                      ? mapTableHeadersConfig(
                          table.tableHeaders,
                          fullTable.subjectMeta,
                        )
                      : getDefaultTableHeaderConfig(fullTable.subjectMeta)
                  }
                />
              );
            })}

            {additionalTabContent}
          </TabsSection>
        )}

        {dataBlock?.charts?.length && fullTable && (
          <TabsSection id={`${id}-charts`} title="Chart">
            <a
              className="govuk-visually-hidden"
              href={`#${id}-tables`}
              aria-live="assertive"
            >
              If you are using a keyboard or a screen reader you may wish to
              view the accessible table instead. Press enter to switch to the
              data tables tab.
            </a>

            {dataBlock?.charts.map((chart, index) => {
              const key = index;

              const axes = { ...chart.axes } as Required<AxesConfiguration>;

              if (
                axes.major?.dataSets?.some(dataSet => !dataSet.config) &&
                chart.labels
              ) {
                axes.major.dataSets = getLabelDataSetConfigurations(
                  chart.labels,
                  axes.major.dataSets,
                );
              }

              if (chart.type === 'infographic') {
                return (
                  <ChartRenderer
                    {...chart}
                    key={key}
                    axes={axes}
                    data={fullTable?.results}
                    meta={fullTable?.subjectMeta}
                    source={dataBlock?.source}
                    getInfographic={getInfographic}
                  />
                );
              }

              return (
                <ChartRenderer
                  {...chart}
                  key={key}
                  axes={axes}
                  data={fullTable?.results}
                  meta={fullTable?.subjectMeta}
                  source={dataBlock?.source}
                />
              );
            })}

            {additionalTabContent}
          </TabsSection>
        )}

        {lastTabs}
      </Tabs>
    </LoadingSpinner>
  );
};

export default DataBlockRenderer;

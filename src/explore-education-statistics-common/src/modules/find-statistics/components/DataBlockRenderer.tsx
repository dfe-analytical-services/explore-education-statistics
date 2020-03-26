import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import TimePeriodDataTableRenderer
  from '@common/modules/find-statistics/components/TimePeriodDataTableRenderer';
import useDataBlockQuery from '@common/modules/find-statistics/hooks/useDataBlockQuery';
import { DataBlock } from '@common/services/types/blocks';
import React, { MouseEvent, ReactNode } from 'react';

export interface DataQuery {
  method: string;
  path: string;
  body: string;
}

export interface DataBlockRendererProps {
  additionalTabContent?: ReactNode;
  dataBlock?: DataBlock;
  firstTabs?: ReactNode;
  lastTabs?: ReactNode;
  getInfographic?: GetInfographic;
  id: string;
  releaseId?: string;
  onToggle?: (section: { id: string; title: string }) => void;
  onSummaryDetailsToggle?: (
    isOpened: boolean,
    event: MouseEvent<HTMLElement>,
  ) => void;
}

const DataBlockRenderer = ({
  additionalTabContent,
  dataBlock,
  firstTabs,
  lastTabs,
  getInfographic,
  id,
  releaseId,
  onToggle,
}: DataBlockRendererProps) => {
  const { value: dataBlockResponse, isLoading } = useDataBlockQuery(dataBlock);

  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs id={id} onToggle={onToggle}>
        {firstTabs}

        {dataBlock?.tables?.length && dataBlockResponse && (
          <TabsSection id={`${id}-tables`} title="Table">
            {dataBlock?.tables.map((table, index) => {
              return (
                <TimePeriodDataTableRenderer
                  key={index}
                  tableHeaders={table.tableHeaders}
                  heading={dataBlock?.heading}
                  data={dataBlockResponse}
                />
              );
            })}

            {additionalTabContent}
          </TabsSection>
        )}

        {dataBlock?.charts?.length && dataBlockResponse && (
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

              if (chart.type === 'infographic') {
                return (
                  <ChartRenderer
                    {...chart}
                    key={key}
                    data={dataBlockResponse}
                    meta={parseMetaData(dataBlockResponse.metaData)}
                    releaseId={releaseId}
                    getInfographic={getInfographic}
                  />
                );
              }

              return (
                <ChartRenderer
                  {...chart}
                  key={key}
                  data={dataBlockResponse}
                  meta={parseMetaData(dataBlockResponse.metaData)}
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

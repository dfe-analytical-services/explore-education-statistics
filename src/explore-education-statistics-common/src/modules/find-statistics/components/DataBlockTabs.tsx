import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import withLazyLoad from '@common/hocs/withLazyLoad';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { AxesConfiguration } from '@common/modules/charts/types/chart';
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { DataBlock } from '@common/services/types/blocks';
import isAxiosError from '@common/utils/error/isAxiosError';
import React, { ReactNode } from 'react';

const testId = (dataBlock: DataBlock) => `Data block - ${dataBlock.name}`;

export interface DataBlockTabsProps {
  additionalTabContent?:
    | ((props: { dataBlock: DataBlock }) => ReactNode)
    | ReactNode;
  dataBlock: DataBlock;
  firstTabs?: ReactNode;
  lastTabs?: ReactNode;
  getInfographic?: GetInfographic;
  id?: string;
  releaseId: string;
  onToggle?: (section: { id: string; title: string }) => void;
}

const DataBlockTabs = ({
  additionalTabContent,
  dataBlock,
  firstTabs,
  lastTabs,
  getInfographic,
  id = `dataBlock-${dataBlock.id}`,
  releaseId,
  onToggle,
}: DataBlockTabsProps) => {
  const {
    value: fullTable,
    isLoading,
    error,
  } = useTableQuery(releaseId, dataBlock.id);

  const errorMessage = <WarningMessage>Could not load content</WarningMessage>;

  if (error && isAxiosError(error) && error.response?.status === 403) {
    return null;
  }

  const additionTabContentElement =
    typeof additionalTabContent === 'function'
      ? additionalTabContent({ dataBlock })
      : additionalTabContent;
  return (
    <LoadingSpinner loading={isLoading}>
      <Tabs id={id} testId={testId(dataBlock)} onToggle={onToggle}>
        {firstTabs}

        {!!dataBlock.charts?.length && (
          <TabsSection
            id={`${id}-charts`}
            tabLabel={
              <>
                Chart
                <span className="govuk-visually-hidden">
                  {` for ${dataBlock.charts[0].title}`}
                </span>
              </>
            }
            title="Chart"
          >
            {error && errorMessage}

            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                {dataBlock.charts.map((chart, index) => {
                  const key = index;

                  const axes = { ...chart.axes } as Required<AxesConfiguration>;

                  if (chart.type === 'infographic') {
                    return (
                      <ChartRenderer
                        {...chart}
                        key={key}
                        id={`${id}-chart`}
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
                      id={`${id}-chart`}
                      axes={axes}
                      data={fullTable?.results}
                      meta={fullTable?.subjectMeta}
                      source={dataBlock?.source}
                    />
                  );
                })}

                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </TabsSection>
        )}

        {dataBlock.table && (
          <TabsSection
            id={`${id}-tables`}
            tabLabel={
              dataBlock.charts?.length ? (
                <>
                  Table
                  <span className="govuk-visually-hidden">
                    {` for ${dataBlock.heading}`}
                  </span>
                </>
              ) : undefined
            }
            title="Table"
          >
            {error && errorMessage}

            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                <TimePeriodDataTable
                  key={dataBlock.id}
                  captionTitle={dataBlock?.heading}
                  dataBlockId={dataBlock.id}
                  fullTable={fullTable}
                  source={dataBlock?.source}
                  tableHeadersConfig={
                    dataBlock.table.tableHeaders
                      ? mapTableHeadersConfig(
                          dataBlock.table.tableHeaders,
                          fullTable,
                        )
                      : getDefaultTableHeaderConfig(fullTable)
                  }
                />

                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </TabsSection>
        )}

        {lastTabs}
      </Tabs>
    </LoadingSpinner>
  );
};

export default withLazyLoad(DataBlockTabs, {
  offset: 100,
  placeholder: ({ dataBlock }) => <span data-testid={testId(dataBlock)} />,
});

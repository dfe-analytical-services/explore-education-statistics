import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import { DataBlockResponse } from '@common/services/dataBlockService';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  DataAddedEvent,
} from '@admin/modules/chart-builder/ChartDataSelector';
import { ChartDataSet } from '@common/services/publicationService';
import styles from './graph-builder.module.scss';
import ConstData from '../../pages/prototypes/PrototypeData';
import ChartTypeSelector from './ChartTypeSelector';
import ChartConfiguration from './ChartConfiguration';

interface Props {
  data: DataBlockResponse;
}

const ChartBuilder = ({ data }: Props) => {
  const [selectedChartType, selectChartType] = React.useState<
    ChartDefinition | undefined
  >();

  const { chartTypes } = ConstData;

  const indicatorIds = Object.keys(data.metaData.indicators);

  const filterIdCombinations: string[][] = Object.values(
    data.result.reduce((filterSet, result) => {
      const filterIds = Array.from(result.filters);

      return {
        ...filterSet,
        [filterIds.join('_')]: filterIds,
      };
    }, {}),
  );

  const [dataSets, setDataSets] = React.useState<ChartDataSet[]>([]);

  const onDataAddedToChart = (addedData: DataAddedEvent[]) => {
    setDataSets(addedData);
  };

  if (data === undefined) return <div />;

  return (
    <div className={styles.editor}>
      <Details summary="Select chart type" open>
        <ChartTypeSelector
          chartTypes={chartTypes}
          onSelectChart={selectChartType}
          selectedChartType={selectedChartType}
        />
      </Details>

      {selectedChartType && (
        <Details summary="Add data to chart" open>
          <ChartDataSelector
            onDataUpdated={onDataAddedToChart}
            metaData={data.metaData}
            indicatorIds={indicatorIds}
            filterIds={filterIdCombinations}
            chartType={selectedChartType}
          />
        </Details>
      )}

      {selectedChartType && dataSets.length > 0 && (
        <React.Fragment>
          <Details summary="Chart preview" open>
            <ChartRenderer
              type={selectedChartType.type}
              dataSets={dataSets}
              data={data}
              meta={data.metaData}
              xAxis={{ title: '' }}
              yAxis={{ title: '' }}
            />
          </Details>

          <Details summary="Chart configuration">
            <ChartConfiguration
              dataSets={dataSets}
              data={data}
              meta={data.metaData}
            />
          </Details>
        </React.Fragment>
      )}
    </div>
  );
};

export default ChartBuilder;

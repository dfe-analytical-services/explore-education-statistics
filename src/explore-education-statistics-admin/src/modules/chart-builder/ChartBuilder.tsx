import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import {DataBlockResponse} from '@common/services/dataBlockService';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  DataUpdatedEvent,
} from '@admin/modules/chart-builder/ChartDataSelector';
import {
  ChartDataSet,
  ChartConfigurationOptions,
  DataLabelConfigurationItem,
} from '@common/services/publicationService';
import {Dictionary} from '@common/types';
import styles from './graph-builder.module.scss';
import ConstData from '../../pages/prototypes/PrototypeData';
import ChartTypeSelector from './ChartTypeSelector';
import ChartDataConfiguration from './ChartDataConfiguration';
import ChartAxisConfiguration from './ChartAxisConfiguration';

interface Props {
  data: DataBlockResponse;
}

const ChartBuilder = ({data}: Props) => {
  const [selectedChartType, selectChartType] = React.useState<ChartDefinition | undefined>();

  const {chartTypes} = ConstData;

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

  const onDataUpdated = (addedData: DataUpdatedEvent[]) => {
    setDataSets(addedData);
  };

  const [chartDataLabels, setChartDataLabels] = React.useState<Dictionary<DataLabelConfigurationItem>>({});

  const [chartConfiguration, setChartConfiguration] = React.useState<ChartConfigurationOptions>({
    dataLabels: {},
    axes: {}
  });

  React.useEffect(() => {
    setChartConfiguration({
      dataLabels: chartDataLabels,
      axes: {},
    });
  }, [chartDataLabels]);

  React.useEffect(() => {
    selectChartType(chartTypes[0]);
    setDataSets([
      {
        indicator: '23',
        filters: ['1', '71'],
      },
    ]);
  }, []);

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
            onDataUpdated={onDataUpdated}
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
              xAxis={{title: ''}}
              yAxis={{title: ''}}
              configuration={chartConfiguration}
            />
          </Details>

          <Details summary="Data label options">
            <ChartDataConfiguration
              dataSets={dataSets}
              data={data}
              meta={data.metaData}
              onDataLabelsChange={setChartDataLabels}
            />
          </Details>

          <Details summary="Axes options" open>
            <p>
              Add / Remove and update the axes and how they display data ranges
            </p>
            {selectedChartType.axes.map(axis => (
              <ChartAxisConfiguration
                dataSets={dataSets}
                key={axis.id}
                meta={data.metaData}
                {...axis}
              />
            ))}
          </Details>
        </React.Fragment>
      )}
    </div>
  );
};

export default ChartBuilder;

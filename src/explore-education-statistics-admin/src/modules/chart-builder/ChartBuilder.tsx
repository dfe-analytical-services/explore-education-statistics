import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import {
  DataBlockResponse,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import {
  ChartDefinition,
  generateKeyFromDataSet,
  colours,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  SelectedData,
} from '@admin/modules/chart-builder/ChartDataSelector';
import {
  ChartDataSet,
  ChartConfiguration,
  AxisConfigurationItem,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import styles from './graph-builder.module.scss';
import ChartTypeSelector from './ChartTypeSelector';
import ChartDataConfiguration from './ChartDataConfiguration';
import ChartAxisConfiguration from './ChartAxisConfiguration';

interface Props {
  data: DataBlockResponse;
}

function dataName(meta: DataBlockMetadata, selectedData: SelectedData) {
  return [
    meta.indicators[selectedData.indicator].label,
    '(',
    ...selectedData.filters.map(filter => meta.filters[filter].label),
    ')',
  ].join(' ');
}

function getReduceMetaDataForAxis(data: DataBlockResponse) {
  return (
    items: Dictionary<ChartConfiguration>,
    groupName?: string,
  ): Dictionary<ChartConfiguration> => {
    if (groupName === 'timePeriod') {
      return {
        ...items,
        ...data.result.reduce<Dictionary<ChartConfiguration>>(
          (moreItems, result) => ({
            ...moreItems,
            [`${result.year}_${result.timeIdentifier}`]: data.metaData
              .timePeriods[`${result.year}_${result.timeIdentifier}`],
          }),
          {},
        ),
      };
    }
    return items;
  };
}

function generateAxesMetaData(
  axes: Dictionary<AxisConfigurationItem>,
  data: DataBlockResponse,
) {
  return Object.values(axes).reduce(
    (allValues, axis) => ({
      ...allValues,
      ...[axis.groupBy].reduce(getReduceMetaDataForAxis(data), {}),
    }),
    {},
  );
}

const chartTypes: ChartDefinition[] = [
  LineChartBlock.definition,
  VerticalBarBlock.definition,
  HorizontalBarBlock.definition,
  MapBlock.definition,
];

const ChartBuilder = ({ data }: Props) => {
  const [selectedChartType, setSelectedChartType] = React.useState<
    ChartDefinition | undefined
  >();

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
  const [chartDataConfiguration, setChartDataConfiguration] = React.useState<
    ChartConfiguration[]
  >([]);

  const onDataAdded = (addedData: SelectedData) => {
    setDataSets([...dataSets, addedData]);

    setChartDataConfiguration([
      ...chartDataConfiguration,
      {
        name: dataName(data.metaData, addedData),
        value: generateKeyFromDataSet(addedData),
        label: dataName(data.metaData, addedData),
        colour: colours[dataSets.length % colours.length],
        symbol: symbols[dataSets.length % symbols.length],
      },
    ]);
  };

  const onDataRemoved = (removedData: SelectedData, index: number) => {
    dataSets.splice(index, 1);
    setDataSets([...dataSets]);

    chartDataConfiguration.splice(index, 1);
    setChartDataConfiguration([...chartDataConfiguration]);
  };

  const [axes, setAxes] = React.useState<Dictionary<AxisConfigurationItem>>({});

  const [labels, setLabels] = React.useState<Dictionary<ChartConfiguration>>(
    {},
  );

  React.useEffect(() => {
    setLabels({
      ...chartDataConfiguration.reduce<Dictionary<ChartConfiguration>>(
        (mapped, item) => ({
          ...mapped,
          [item.value]: item,
        }),
        {},
      ),

      ...generateAxesMetaData(axes, data),
    });
  }, [axes, chartDataConfiguration, data]);

  React.useEffect(() => {
    if (selectedChartType) {
      const axiConfiguration = selectedChartType.axes.reduce<
        Dictionary<AxisConfigurationItem>
      >(
        (axesConfigurationDictionary, axisDefinition) => ({
          ...axesConfigurationDictionary,

          [axisDefinition.type]: {
            name: `${axisDefinition.title} (${axisDefinition.type} axis)`,
            groupBy: axisDefinition.defaultDataType,
            dataSets: axisDefinition.type === 'major' ? dataSets : [],
          },
        }),
        {},
      );

      setAxes(axiConfiguration);
    }
  }, [dataSets, selectedChartType]);

  return (
    <div className={styles.editor}>
      <Details summary="Select chart type" open>
        <ChartTypeSelector
          chartTypes={chartTypes}
          onSelectChart={setSelectedChartType}
          selectedChartType={selectedChartType}
        />
      </Details>

      {selectedChartType && (
        <React.Fragment>
          <Details summary="Add data to chart" open>
            <p>Add data from the existing dataset to the chart</p>
            <ChartDataSelector
              onDataAdded={onDataAdded}
              onDataRemoved={onDataRemoved}
              metaData={data.metaData}
              indicatorIds={indicatorIds}
              filterIds={filterIdCombinations}
              selectedData={dataSets}
              chartType={selectedChartType}
            />
          </Details>

          <Details summary="Chart preview" open>
            <ChartRenderer
              type={selectedChartType.type}
              axes={axes}
              data={data}
              meta={data.metaData}
              labels={labels}
            />
          </Details>

          <Details summary="Configure chart" open>
            <p>Configure the overall options for the chart</p>
            <p>Configure legend</p>
          </Details>

          <Details summary="Data label options" open>
            Update the configuration used for each dataset in the chart from the
            default
            {chartDataConfiguration.map((config, index) => (
              <ChartDataConfiguration
                key={config.value}
                configuration={config}
                onConfigurationChange={updatedConfig => {
                  const newConfig = [...chartDataConfiguration];
                  newConfig.splice(index, 1, updatedConfig);
                  setChartDataConfiguration(newConfig);
                }}
              />
            ))}
          </Details>

          <Details summary="Axes options">
            <p>
              Add / Remove and update the axes and how they display data ranges
            </p>
            {Object.values(axes).map(axis => (
              <ChartAxisConfiguration
                id={axis.name}
                axisConfiguration={axis}
                key={axis.name}
                meta={data.metaData}
              />
            ))}
          </Details>
        </React.Fragment>
      )}
    </div>
  );
};

export default ChartBuilder;

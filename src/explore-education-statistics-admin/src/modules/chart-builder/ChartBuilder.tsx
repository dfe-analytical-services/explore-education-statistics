import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import {
  DataBlockMetadata,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  ChartDefinition,
  colours,
  generateKeyFromDataSet,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  SelectedData,
} from '@admin/modules/chart-builder/ChartDataSelector';
import {
  AxisConfiguration,
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import {
  FormSelect,
  FormTextInput,
  FormCheckbox,
} from '@common/components/form';
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
    items: Dictionary<DataSetConfiguration>,
    groupName?: string,
  ): Dictionary<DataSetConfiguration> => {
    if (groupName === 'timePeriod') {
      return {
        ...items,
        ...data.result.reduce<Dictionary<DataSetConfiguration>>(
          (moreItems, result) => ({
            ...moreItems,
            [result.timePeriod]: data.metaData.timePeriods[result.timePeriod],
          }),
          {},
        ),
      };
    }
    return items;
  };
}

function generateAxesMetaData(
  axes: Dictionary<AxisConfiguration>,
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

interface ChartOptions {
  stacked: boolean;
  legend: 'none' | 'top' | 'bottom';
  legendHeight: string;
}

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

  const [chartOptions, setChartOptions] = React.useState<ChartOptions>({
    stacked: false,
    legend: 'top',
    legendHeight: '42',
  });
  const [dataSets, setDataSets] = React.useState<ChartDataSet[]>([]);
  const [chartDataConfiguration, setChartDataConfiguration] = React.useState<
    DataSetConfiguration[]
  >([]);

  const [axesConfiguration, setAxesConfiguration] = React.useState<
    Dictionary<AxisConfiguration>
  >({});

  const onDataAdded = (addedData: SelectedData) => {
    const newDataSets = [...dataSets, addedData];
    setDataSets(newDataSets);

    setChartDataConfiguration([
      ...chartDataConfiguration,
      {
        name: dataName(data.metaData, addedData),
        value: generateKeyFromDataSet(addedData),
        label: dataName(data.metaData, addedData),
        colour: colours[newDataSets.length % colours.length],
        symbol: symbols[newDataSets.length % symbols.length],
      },
    ]);
  };

  const onDataRemoved = (removedData: SelectedData, index: number) => {
    const newDataSets = [...dataSets];

    newDataSets.splice(index, 1);
    setDataSets(newDataSets);
    const newChartDataConfiguration = [...chartDataConfiguration];
    newChartDataConfiguration.splice(index, 1);
    setChartDataConfiguration(newChartDataConfiguration);
  };

  // build the properties that is used to render the chart from the selections made
  const [renderedChartProps, setRenderedChartProps] = React.useState<
    ChartRendererProps
  >();
  React.useEffect(() => {
    if (selectedChartType)
      setRenderedChartProps({
        type: selectedChartType.type,

        data,

        meta: data.metaData,

        axes: Object.entries(axesConfiguration).reduce<
          Dictionary<AxisConfiguration>
        >(
          (populatedData, [key, value]) => ({
            ...populatedData,
            [key]: {
              ...value,
              dataSets: value.type === 'major' ? dataSets : [],
            },
          }),
          {},
        ),
        labels: {
          ...chartDataConfiguration.reduce<Dictionary<DataSetConfiguration>>(
            (mapped, item) => ({
              ...mapped,
              [item.value]: item,
            }),
            {},
          ),

          ...generateAxesMetaData(axesConfiguration, data),
        },
        ...chartOptions,
      });
  }, [
    selectedChartType,
    axesConfiguration,
    dataSets,
    chartDataConfiguration,
    data,
    chartOptions,
  ]);

  const previousSelectionChartType = React.useRef<ChartDefinition>();
  // set defaults for a selected chart type
  React.useEffect(() => {
    if (previousSelectionChartType.current !== selectedChartType) {
      previousSelectionChartType.current = selectedChartType;

      if (selectedChartType) {
        const axisConfiguration = selectedChartType.axes.reduce<
          Dictionary<AxisConfiguration>
        >(
          (axesConfigurationDictionary, axisDefinition) => ({
            ...axesConfigurationDictionary,

            [axisDefinition.type]: {
              name: `${axisDefinition.title} (${axisDefinition.type} axis)`,
              type: axisDefinition.type,
              groupBy: axisDefinition.defaultDataType,

              dataSets: axisDefinition.type === 'major' ? dataSets : [],
              visible: true,
              showGrid: true,
              size: '50',
              referenceLines: [],
            },
          }),
          {},
        );

        setAxesConfiguration(axisConfiguration);
      }
    }
  }, [selectedChartType, dataSets]);

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

          {renderedChartProps && (
            <Details summary="Chart preview" open>
              <ChartRenderer {...renderedChartProps} />
            </Details>
          )}

          <Details summary="Configure chart" open>
            {selectedChartType.capabilities.stackable && (
              <FormCheckbox
                id="stacked"
                name="stacked"
                label="Stacked bars"
                checked={chartOptions.stacked}
                value="stacked"
                onChange={e => {
                  setChartOptions({
                    ...chartOptions,
                    stacked: e.target.checked,
                  });
                }}
              />
            )}
            <FormSelect
              id="legend-position"
              name="legend-position"
              value={chartOptions.legend}
              label="Legend Position"
              options={[
                { label: 'Top', value: 'top' },
                { label: 'Bottom', value: 'bottom' },
                { label: 'None', value: 'none' },
              ]}
              order={[]}
              onChange={e => {
                // @ts-ignore
                setChartOptions({ ...chartOptions, legend: e.target.value });
              }}
            />
            {chartOptions.legend !== 'none' && (
              <FormTextInput
                id="legend-height"
                name="legend-height"
                label="Legend Height (blank for automatic)"
                value={chartOptions.legendHeight}
                onChange={e => {
                  setChartOptions({
                    ...chartOptions,
                    legendHeight: e.target.value,
                  });
                }}
              />
            )}
          </Details>

          <Details summary="Data label options" open>
            Update the configuration used for each dataset in the chart from the
            default
            <div className={styles.axesOptions}>
              {chartDataConfiguration.map((config, index) => (
                <ChartDataConfiguration
                  key={config.value}
                  configuration={config}
                  capabilities={selectedChartType.capabilities}
                  onConfigurationChange={updatedConfig => {
                    const newConfig = [...chartDataConfiguration];
                    newConfig.splice(index, 1, updatedConfig);
                    setChartDataConfiguration(newConfig);
                  }}
                />
              ))}
            </div>
          </Details>

          <Details summary="Axes configuration">
            <div className={styles.axesOptions}>
              {Object.entries(axesConfiguration).map(([key, axis]) => (
                <ChartAxisConfiguration
                  key={key}
                  id={axis.name}
                  configuration={axis}
                  capabilities={selectedChartType.capabilities}
                  meta={data.metaData}
                  onConfigurationChange={updatedConfig => {
                    setAxesConfiguration({
                      ...axesConfiguration,
                      [key]: updatedConfig,
                    });
                  }}
                />
              ))}
            </div>
          </Details>
        </React.Fragment>
      )}
    </div>
  );
};

export default ChartBuilder;

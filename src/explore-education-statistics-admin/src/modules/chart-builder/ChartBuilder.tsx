import ChartDataSelector, {
  SelectedData,
  ChartDataSetAndConfiguration,
} from '@admin/modules/chart-builder/ChartDataSelector';

import Details from '@common/components/Details';
import {
  FormCheckbox,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import {
  ChartDefinition,
  colours,
  generateKeyFromDataSet,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import {
  DataBlockMetadata,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React from 'react';
import ChartAxisConfiguration from './ChartAxisConfiguration';
import ChartTypeSelector from './ChartTypeSelector';
import styles from './graph-builder.module.scss';

interface Props {
  data: DataBlockResponse;
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
  const previousAxesConfiguration = React.useRef<Dictionary<AxisConfiguration>>(
    {},
  );

  const [axesConfiguration, realSetAxesConfiguration] = React.useState<
    Dictionary<AxisConfiguration>
  >({});

  const [dataSetAndConfiguration, setDataSetAndConfiguration] = React.useState<
    ChartDataSetAndConfiguration[]
  >([]);

  const setAxesConfiguration = (config: Dictionary<AxisConfiguration>) => {
    previousAxesConfiguration.current = config;
    realSetAxesConfiguration(config);
  };

  const onDataAdded = (addedData: SelectedData) => {
    const newDataSetConfig = [...dataSetAndConfiguration, addedData];

    setDataSetAndConfiguration(newDataSetConfig);
  };

  const onDataRemoved = (removedData: SelectedData, index: number) => {
    const newDataSets = [...dataSetAndConfiguration];

    newDataSets.splice(index, 1);
    setDataSetAndConfiguration(newDataSets);
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
              dataSets:
                value.type === 'major'
                  ? dataSetAndConfiguration.map(dsc => dsc.dataSet)
                  : [],
            },
          }),
          {},
        ),
        labels: {
          ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
            (mapped, { configuration }) => ({
              ...mapped,
              [configuration.value]: configuration,
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
    dataSetAndConfiguration,
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

              dataSets:
                axisDefinition.type === 'major'
                  ? dataSetAndConfiguration.map(dsc => dsc.dataSet)
                  : [],
              visible: true,
              showGrid: true,
              size: '50',
              referenceLines: [],
              ...(previousAxesConfiguration.current &&
                previousAxesConfiguration.current[axisDefinition.type]),
            },
          }),
          {},
        );

        setAxesConfiguration(axisConfiguration);
      }
    }
  }, [selectedChartType, dataSetAndConfiguration]);

  return (
    <div className={styles.editor}>
      <ChartTypeSelector
        chartTypes={chartTypes}
        onSelectChart={setSelectedChartType}
        selectedChartType={selectedChartType}
      />

      {renderedChartProps && (
        <Details summary="Chart preview" open>
          <ChartRenderer {...renderedChartProps} />
        </Details>
      )}

      {selectedChartType && (
        <Tabs id="ChartTabs">
          <TabsSection title="Data">
            <p>Add data from the existing dataset to the chart</p>
            <ChartDataSelector
              onDataAdded={onDataAdded}
              onDataRemoved={onDataRemoved}
              onDataChanged={(newData: ChartDataSetAndConfiguration[]) => {
                setDataSetAndConfiguration([...newData]);
              }}
              metaData={data.metaData}
              indicatorIds={indicatorIds}
              filterIds={filterIdCombinations}
              selectedData={dataSetAndConfiguration}
              chartType={selectedChartType}
              capabilities={selectedChartType.capabilities}
            />
          </TabsSection>

          <TabsSection title="Chart">
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
                width={5}
                onChange={e => {
                  setChartOptions({
                    ...chartOptions,
                    legendHeight: e.target.value,
                  });
                }}
              />
            )}
          </TabsSection>

          <TabsSection title="Axes">
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
          </TabsSection>
        </Tabs>
      )}
    </div>
  );
};

export default ChartBuilder;

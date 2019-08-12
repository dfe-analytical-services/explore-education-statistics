import ChartDataSelector, {
  ChartDataSetAndConfiguration,
  SelectedData,
} from '@admin/modules/chart-builder/ChartDataSelector';

import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/find-statistics/components/ChartRenderer';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import {DataBlockResponse} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  DataSetConfiguration,
  ChartDataSet,
} from '@common/services/publicationService';
import {Dictionary} from '@common/types';
import React from 'react';
import ChartConfiguration, {
  ChartOptions,
} from '@admin/modules/chart-builder/ChartConfiguration';
import classnames from 'classnames';
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

const ChartBuilder = ({data}: Props) => {
  const [selectedChartType, setSelectedChartType] = React.useState<ChartDefinition | undefined>();

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
    height: 300,
    width: undefined,
    title: '',
  });
  const previousAxesConfiguration = React.useRef<Dictionary<AxisConfiguration>>(
    {},
  );

  const [axesConfiguration, realSetAxesConfiguration] = React.useState<Dictionary<AxisConfiguration>>({});

  const [dataSetAndConfiguration, setDataSetAndConfiguration] = React.useState<ChartDataSetAndConfiguration[]>([]);

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

  const [chartLabels, setChartLabels] = React.useState<Dictionary<DataSetConfiguration>>({});
  React.useEffect(() => {
    setChartLabels({
      ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
        (mapped, {configuration}) => ({
          ...mapped,
          [configuration.value]: configuration,
        }),
        {},
      ),
      ...generateAxesMetaData(axesConfiguration, data),
    });
  }, [dataSetAndConfiguration, axesConfiguration, data]);

  const [majorAxisDataSets, setMajorAxisDataSets] = React.useState<ChartDataSet[]>([]);
  React.useEffect(() => {
    setMajorAxisDataSets(dataSetAndConfiguration.map(dsc => dsc.dataSet));
  }, [dataSetAndConfiguration]);

  // build the properties that is used to render the chart from the selections made
  const [renderedChartProps, setRenderedChartProps] = React.useState<ChartRendererProps>();
  React.useEffect(() => {

    if (selectedChartType && majorAxisDataSets.length > 0) {
      setRenderedChartProps({
        type: selectedChartType.type,

        data,

        meta: data.metaData,

        axes: {
          major: {
            ...axesConfiguration.major,
            dataSets: majorAxisDataSets,
          },
          minor: {
            ...axesConfiguration.minor,
            dataSets: [],
          },
        },
        labels: chartLabels,
        ...chartOptions,
      });
    } else {
      setRenderedChartProps(undefined);
    }
  }, [
    selectedChartType,
    axesConfiguration,
    dataSetAndConfiguration,
    data,
    chartOptions,
    chartLabels,
    majorAxisDataSets,
  ]);

  const previousSelectionChartType = React.useRef<ChartDefinition>();
  // set defaults for a selected chart type
  React.useEffect(() => {
    if (previousSelectionChartType.current !== selectedChartType) {
      previousSelectionChartType.current = selectedChartType;

      if (selectedChartType) {
        const newAxesConfiguration = selectedChartType.axes.reduce<Dictionary<AxisConfiguration>>((axesConfigurationDictionary, axisDefinition) => {
          const previousConfig =
            (previousAxesConfiguration.current &&
              previousAxesConfiguration.current[axisDefinition.type]) ||
            {};

          return {
            ...axesConfigurationDictionary,

            [axisDefinition.type]: {
              referenceLines: [],
              min: '',
              max: '',
              tickSpacing: '',
              unit: '',
              tickConfig: 'default',

              ...previousConfig,

              // hard-coded defaults
              type: axisDefinition.type,
              name: `${axisDefinition.title} (${axisDefinition.type} axis)`,
              groupBy:
                axisDefinition.forcedDataType ||
                previousConfig.groupBy ||
                axisDefinition.defaultDataType,
              dataSets:
                axisDefinition.type === 'major'
                  ? dataSetAndConfiguration.map(dsc => dsc.dataSet)
                  : [],

              // defaults that can be undefined and may be overriden
              visible:
                previousConfig.visible === undefined
                  ? true
                  : previousConfig.visible,
              showGrid:
                previousConfig.showGrid === undefined
                  ? true
                  : previousConfig.showGrid,
              size:
                previousConfig.size === undefined ? '50' : previousConfig.size,
              sortBy: previousConfig.sortBy || 'name',
              sortAsc: previousConfig.sortAsc || true,
            },
          };
        }, {});

        setAxesConfiguration(newAxesConfiguration);
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
      <div className="govuk-!-margin-top-6 govuk-body-s dfe-align--right">
        <a href="#">Choose an infographic as alternative</a>
      </div>

      {selectedChartType && (
        <div className="govuk-width-container">
          {renderedChartProps === undefined ? (
            <div
              className={classnames(styles.preview)}
              style={
                {
                  width: chartOptions.width && `${chartOptions.width}px`,
                  height: chartOptions.height && `${chartOptions.height}px`,
                }
              }
            >
              <span>Add data to view a preview of the chart</span>
            </div>
          ) : (
            <ChartRenderer {...renderedChartProps} />
          )}
        </div>
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

          <TabsSection title="Chart Configuration">
            <ChartConfiguration
              selectedChartType={selectedChartType}
              chartOptions={chartOptions}
              onChange={setChartOptions}
            />
          </TabsSection>

          {Object.entries(axesConfiguration).map(([key, axis]) => (
            <TabsSection title={axis.name} key={key}>
              <div className={styles.axesOptions}>
                <ChartAxisConfiguration
                  id={key}
                  configuration={axis}
                  capabilities={selectedChartType.capabilities}
                  data={data}
                  meta={data.metaData}
                  labels={chartLabels}
                  dataSets={axis.type === 'major' ? majorAxisDataSets : []}
                  onConfigurationChange={updatedConfig => {
                    setAxesConfiguration({
                      ...axesConfiguration,
                      [key]: updatedConfig,
                    });
                  }}
                />
              </div>
            </TabsSection>
          ))}
        </Tabs>
      )}
    </div>
  );
};

export default ChartBuilder;

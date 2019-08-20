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
import {
  ChartDefinition,
  generateKeyFromDataSet,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import {
  DataBlockResponse,
  DataBlockRerequest,
} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  ChartDataSet,
  DataSetConfiguration,
  Chart,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React from 'react';
import ChartConfiguration, {
  ChartOptions,
} from '@admin/modules/chart-builder/ChartConfiguration';
import classnames from 'classnames';
import Infographic from '@common/modules/find-statistics/components/charts/Infographic';
import ChartAxisConfiguration from './ChartAxisConfiguration';
import ChartTypeSelector from './ChartTypeSelector';
import styles from './graph-builder.module.scss';

interface Props {
  data: DataBlockResponse;
  initialConfiguration?: Chart;
  onChartSave?: (props: ChartRendererProps) => void;

  onRequiresDataUpdate?: (parameters: DataBlockRerequest) => void;
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

// need a constant reference as there are dependencies on this not changing if it isn't set
const ChartBuilder = ({
  data,
  onChartSave,
  initialConfiguration,
  onRequiresDataUpdate,
}: Props) => {
  const [selectedChartType, setSelectedChartType] = React.useState<
    ChartDefinition | undefined
  >();

  const [indicatorIds] = React.useState<string[]>(
    Object.keys(data.metaData.indicators),
  );

  const [filterIdCombinations] = React.useState<string[][]>(
    Object.values(
      data.result.reduce((filterSet, result) => {
        const filterIds = Array.from(result.filters);

        return {
          ...filterSet,
          [filterIds.join('_')]: filterIds,
        };
      }, {}),
    ),
  );

  const [chartOptions, setChartOptions] = React.useState<ChartOptions>({
    stacked: false,
    legend: 'top',
    legendHeight: '42',
    height: 300,
    title: '',
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

  const [chartLabels, setChartLabels] = React.useState<
    Dictionary<DataSetConfiguration>
  >({});
  React.useEffect(() => {
    setChartLabels({
      ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
        (mapped, { configuration }) => ({
          ...mapped,
          [configuration.value]: configuration,
        }),
        {},
      ),
      ...generateAxesMetaData(axesConfiguration, data),
    });
  }, [dataSetAndConfiguration, axesConfiguration, data]);

  const [majorAxisDataSets, setMajorAxisDataSets] = React.useState<
    ChartDataSet[]
  >([]);
  React.useEffect(() => {
    setMajorAxisDataSets(dataSetAndConfiguration.map(dsc => dsc.dataSet));
  }, [dataSetAndConfiguration]);

  // build the properties that is used to render the chart from the selections made
  const [renderedChartProps, setRenderedChartProps] = React.useState<
    ChartRendererProps
  >();
  React.useEffect(() => {
    if (
      selectedChartType &&
      majorAxisDataSets.length > 0 &&
      axesConfiguration.major
    ) {
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
        const newAxesConfiguration = selectedChartType.axes.reduce<
          Dictionary<AxisConfiguration>
        >((axesConfigurationDictionary, axisDefinition) => {
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

  const extractInitialChartOptions = (
    {
      type,
      stacked = false,
      legend = 'top',
      legendHeight = '42',
      height = 300,
      width,
      title = '',
      fileId,
      geographicId,
      labels,
      axes,
    }: Chart = {
      stacked: false,
      legend: 'top',
      legendHeight: '42',
      height: 300,
      title: '',
    },
  ) => {
    return {
      type,
      options: {
        stacked,
        legend,
        legendHeight,
        height,
        width,
        title,
        fileId,
        geographicId,
      },
      axes,
      labels,
    };
  };

  // initial chart options set up
  React.useEffect(() => {
    const initial = extractInitialChartOptions(initialConfiguration);

    setSelectedChartType(
      () => initial && chartTypes.find(({ type }) => type === initial.type),
    );

    setChartOptions({ ...initial.options });

    if (initial.labels) {
      setChartLabels(initial.labels);
    }

    if (initial.axes && initial.labels) {
      setAxesConfiguration((initial.axes as unknown) as Dictionary<
        AxisConfiguration
      >);

      if (initial.axes.major && initial.axes.major.dataSets && initial.labels) {
        const dataSetAndConfig = initial.axes.major.dataSets
          .map(dataSet => {
            const key = generateKeyFromDataSet(dataSet);
            const configuration = initial.labels && initial.labels[key];
            return { dataSet, configuration };
          })
          .filter(dsc => dsc.configuration !== undefined);

        // @ts-ignore ... because Typescript is a pain
        setDataSetAndConfiguration(dataSetAndConfig);
      }
    }
  }, [initialConfiguration]);

  return (
    <div className={styles.editor}>
      <ChartTypeSelector
        chartTypes={chartTypes}
        onSelectChart={setSelectedChartType}
        selectedChartType={selectedChartType}
      />
      <div className="govuk-!-margin-top-6 govuk-body-s dfe-align--right">
        <a
          href="#"
          onClick={() => setSelectedChartType(Infographic.definition)}
        >
          Choose an infographic as alternative
        </a>
      </div>

      {selectedChartType && (
        <Details summary="Chart preview" open>
          {renderedChartProps === undefined ? (
            <div
              className={classnames(styles.preview)}
              style={{
                width: chartOptions.width && `${chartOptions.width}px`,
                height: chartOptions.height && `${chartOptions.height}px`,
              }}
            >
              <span>Add data to view a preview of the chart</span>
            </div>
          ) : (
            <ChartRenderer {...renderedChartProps} />
          )}
        </Details>
      )}

      {selectedChartType && (
        <Tabs id="ChartTabs">
          {selectedChartType.data.length > 0 && (
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
          )}

          <TabsSection title="Chart Configuration">
            <ChartConfiguration
              selectedChartType={selectedChartType}
              chartOptions={chartOptions}
              onChange={setChartOptions}
              onBoundaryLevelChange={boundaryLevel =>
                onRequiresDataUpdate &&
                onRequiresDataUpdate({
                  boundaryLevel: boundaryLevel
                    ? Number.parseInt(boundaryLevel, 10)
                    : undefined,
                })
              }
              meta={data.metaData}
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

      {selectedChartType && renderedChartProps && (
        <>
          <button
            type="button"
            className="govuk-button"
            onClick={() => {
              if (onChartSave) {
                onChartSave(renderedChartProps);
              }
            }}
          >
            Save chart options
          </button>
        </>
      )}
    </div>
  );
};

export default ChartBuilder;

import ChartAxisConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartAxisConfiguration';
import ChartConfiguration, {
  ChartOptions,
} from '@admin/pages/release/edit-release/manage-datablocks/components/ChartConfiguration';
import ChartDataSelector, {
  ChartDataSetAndConfiguration,
  SelectedData,
} from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataSelector';
import ChartTypeSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartTypeSelector';
import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';

import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import Infographic from '@common/modules/charts/components/Infographic';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import MapBlock from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import {
  ChartDefinition,
  ChartMetaData,
} from '@common/modules/charts/types/chart';
import {
  generateKeyFromDataSet,
  parseMetaData,
} from '@common/modules/charts/util/chartUtils';
import {
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  Chart,
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

interface Props {
  data: DataBlockResponse;
  initialConfiguration?: Chart;
  onChartSave?: (
    props: ChartRendererProps,
  ) => Promise<ChartRendererProps | undefined>;

  onRequiresDataUpdate?: (parameters: DataBlockRerequest) => void;
}

function getReduceMetaDataForAxis(
  data: DataBlockResponse,
  metaData: ChartMetaData,
) {
  return (
    items: Dictionary<DataSetConfiguration>,
    groupName?: string,
  ): Dictionary<DataSetConfiguration> => {
    if (groupName === 'timePeriod') {
      return {
        ...items,
        ...data.result.reduce<Dictionary<DataSetConfiguration>>(
          (moreItems, { timePeriod }) => ({
            ...moreItems,
            [timePeriod]: metaData.timePeriod[timePeriod],
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
  metaData: ChartMetaData,
) {
  return Object.values(axes).reduce(
    (allValues, axis) => ({
      ...allValues,
      ...[axis.groupBy].reduce(getReduceMetaDataForAxis(data, metaData), {}),
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

const emptyMetadata = {
  timePeriod: {},
  filters: {},
  indicators: {},
  locations: {},
};

enum SaveState {
  Unsaved,
  Error,
  Saved,
}

// need a constant reference as there are dependencies on this not changing if it isn't set
const ChartBuilder = ({
  data,
  onChartSave,
  initialConfiguration,
  onRequiresDataUpdate,
  handleApiErrors,
  handleManualErrors,
}: Props & ErrorControlProps) => {
  const [selectedChartType, setSelectedChartType] = useState<
    ChartDefinition | undefined
  >();

  const indicatorIds = Object.keys(data.metaData.indicators);
  const metaData = useMemo(
    () => (data.metaData && parseMetaData(data.metaData)) || emptyMetadata,
    [data.metaData],
  );

  const filterIdCombinations = useMemo<string[][]>(
    () =>
      Object.values(
        data.result.reduce((filterSet, result) => {
          const filterIds = Array.from(result.filters);

          return {
            ...filterSet,
            [filterIds.join('_')]: filterIds,
          };
        }, {}),
      ),
    [data.result],
  );

  const [chartOptions, setChartOptions] = useState<ChartOptions>({
    stacked: false,
    legend: 'top',
    legendHeight: '42',
    height: 300,
    title: '',
  });

  const previousAxesConfiguration = useRef<Dictionary<AxisConfiguration>>({});

  const [chartSaveState, setChartSaveState] = useState(SaveState.Unsaved);

  const [axesConfiguration, realSetAxesConfiguration] = useState<
    Dictionary<AxisConfiguration>
  >({});

  const [dataSetAndConfiguration, setDataSetAndConfiguration] = useState<
    ChartDataSetAndConfiguration[]
  >([]);

  const setAxesConfiguration = (config: Dictionary<AxisConfiguration>) => {
    previousAxesConfiguration.current = config;
    realSetAxesConfiguration(config);
  };

  const onDataAdded = (addedData: SelectedData) => {
    setChartSaveState(SaveState.Unsaved);

    const newDataSetConfig = [...dataSetAndConfiguration, addedData];

    setDataSetAndConfiguration(newDataSetConfig);
  };

  const onDataRemoved = (removedData: SelectedData, index: number) => {
    setChartSaveState(SaveState.Unsaved);

    const newDataSets = [...dataSetAndConfiguration];

    newDataSets.splice(index, 1);
    setDataSetAndConfiguration(newDataSets);
  };

  const [chartLabels, setChartLabels] = useState<
    Dictionary<DataSetConfiguration>
  >({});
  useEffect(() => {
    setChartLabels({
      ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
        (mapped, { configuration }) => ({
          ...mapped,
          [configuration.value]: configuration,
        }),
        {},
      ),
      ...generateAxesMetaData(axesConfiguration, data, metaData),
    });
  }, [dataSetAndConfiguration, axesConfiguration, data, metaData]);

  const [majorAxisDataSets, setMajorAxisDataSets] = useState<ChartDataSet[]>(
    [],
  );
  useEffect(() => {
    setMajorAxisDataSets(dataSetAndConfiguration.map(dsc => dsc.dataSet));
  }, [dataSetAndConfiguration]);

  // build the properties that is used to render the chart from the selections made
  const [renderedChartProps, setRenderedChartProps] = useState<
    ChartRendererProps
  >();
  useEffect(() => {
    if (
      selectedChartType &&
      (selectedChartType.axes.length === 0 ||
        (selectedChartType.axes.length > 0 &&
          majorAxisDataSets.length > 0 &&
          axesConfiguration.major))
    ) {
      setRenderedChartProps({
        ...chartOptions,
        type: selectedChartType.type,
        data,
        meta: metaData,
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
        releaseId: data.releaseId,
        getInfographic: editReleaseDataService.downloadChartFile,
      });
    } else {
      setRenderedChartProps(undefined);
    }
  }, [
    selectedChartType,
    axesConfiguration,
    dataSetAndConfiguration,
    data,
    metaData,
    chartOptions,
    chartLabels,
    majorAxisDataSets,
  ]);

  const previousSelectionChartType = useRef<ChartDefinition>();
  // set defaults for a selected chart type
  useEffect(() => {
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
              min: '0',
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
    let checkLabels = labels;

    if (labels) {
      checkLabels = Object.keys(labels).reduce<
        Dictionary<DataSetConfiguration>
      >(
        (newLabels, key) => ({
          ...newLabels,
          [key]: {
            ...labels[key],
            value: labels[key].value || key,
          },
        }),
        {},
      );
    }

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
      labels: checkLabels,
    };
  };

  const saveChart = useCallback(async () => {
    setChartSaveState(SaveState.Unsaved);
    if (renderedChartProps && onChartSave) {
      try {
        await onChartSave(renderedChartProps);
        setChartSaveState(SaveState.Saved);
      } catch (_) {
        setChartSaveState(SaveState.Error);
      }
    }
  }, [onChartSave, renderedChartProps]);

  // initial chart options set up
  useEffect(() => {
    const initial = extractInitialChartOptions(initialConfiguration);

    setChartSaveState(SaveState.Unsaved);

    setSelectedChartType(
      () => initial && chartTypes.find(({ type }) => type === initial.type),
    );

    setChartOptions({ ...initial.options });

    if (initial.labels) {
      setChartLabels(initial.labels);
    }

    if (initial.axes && initial.labels) {
      setAxesConfiguration(
        (initial.axes as unknown) as Dictionary<AxisConfiguration>,
      );

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
        onSelectChart={chart => {
          setChartSaveState(SaveState.Unsaved);
          setSelectedChartType(chart);
        }}
        selectedChartType={selectedChartType}
        geoJsonAvailable={data.metaData.geoJsonAvailable}
      />
      <div className="govuk-!-margin-top-6 govuk-body-s dfe-align--right">
        <a
          href="#"
          onClick={e => {
            e.preventDefault();
            setChartSaveState(SaveState.Unsaved);
            setSelectedChartType(Infographic.definition);
          }}
        >
          Choose an infographic as alternative
        </a>
      </div>

      {selectedChartType && (
        <Details summary="Chart preview" open>
          <div className="govuk-width-container">
            {renderedChartProps === undefined ? (
              <div className={styles.previewPlaceholder}>
                {selectedChartType.axes.length > 0 ? (
                  <p>Add data to view a preview of the chart</p>
                ) : (
                  <p>
                    Configure the {selectedChartType.name} to view a preview
                  </p>
                )}
              </div>
            ) : (
              <ChartRenderer {...renderedChartProps} />
            )}
          </div>
        </Details>
      )}

      {selectedChartType && (
        <Tabs id="ChartTabs">
          {selectedChartType.data.length > 0 && (
            <TabsSection title="Data">
              <h2 className="govuk-heading-m">
                Add data from the existing dataset to the chart
              </h2>
              <ChartDataSelector
                onDataAdded={onDataAdded}
                onDataRemoved={onDataRemoved}
                onDataChanged={(newData: ChartDataSetAndConfiguration[]) => {
                  setChartSaveState(SaveState.Unsaved);
                  setDataSetAndConfiguration([...newData]);
                }}
                metaData={metaData}
                indicatorIds={indicatorIds}
                filterIds={filterIdCombinations}
                selectedData={dataSetAndConfiguration}
                chartType={selectedChartType}
                capabilities={selectedChartType.capabilities}
              />
            </TabsSection>
          )}

          <TabsSection title="Chart configuration">
            <h2 className="govuk-heading-m">Chart configuration</h2>
            <ChartConfiguration
              selectedChartType={selectedChartType}
              chartOptions={chartOptions}
              onChange={chartOptionsValue => {
                setChartSaveState(SaveState.Unsaved);
                setChartOptions(chartOptionsValue);
              }}
              onBoundaryLevelChange={boundaryLevel => {
                setChartSaveState(SaveState.Unsaved);
                if (onRequiresDataUpdate)
                  onRequiresDataUpdate({
                    boundaryLevel: boundaryLevel
                      ? Number.parseInt(boundaryLevel, 10)
                      : undefined,
                  });
              }}
              meta={metaData}
              data={data}
              handleApiErrors={handleApiErrors}
              handleManualErrors={handleManualErrors}
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
                  meta={metaData}
                  labels={chartLabels}
                  dataSets={axis.type === 'major' ? majorAxisDataSets : []}
                  onConfigurationChange={updatedConfig => {
                    setChartSaveState(SaveState.Unsaved);
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
          <button type="button" className="govuk-button" onClick={saveChart}>
            Save chart options
          </button>

          {chartSaveState !== SaveState.Unsaved && (
            <div>
              {chartSaveState === SaveState.Saved && (
                <span>Chart has been saved</span>
              )}
              {chartSaveState === SaveState.Error && (
                <span>
                  An error occurred saving the chart, please try again later
                </span>
              )}
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default withErrorControl(ChartBuilder);

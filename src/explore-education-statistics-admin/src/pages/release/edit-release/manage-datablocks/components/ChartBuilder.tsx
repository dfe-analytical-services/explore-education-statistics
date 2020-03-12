import ChartAxisConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartAxisConfiguration';
import ChartConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartConfiguration';
import ChartDataSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataSelector';
import ChartTypeSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartTypeSelector';
import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import {
  ChartBuilderState,
  useChartBuilderReducer,
} from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import Infographic from '@common/modules/charts/components/Infographic';
import {
  ChartDefinition,
  chartDefinitions,
  ChartMetaData,
} from '@common/modules/charts/types/chart';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import {
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  Chart,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import React, { useCallback, useMemo, useState } from 'react';

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

const isChartRenderable = (
  currentProps: Partial<ChartRendererProps>,
  chartDefinition?: ChartDefinition,
): currentProps is ChartRendererProps => {
  // Chart definition may be an infographic
  // and can be rendered without any axes.
  if (chartDefinition && Object.keys(chartDefinition.axes).length === 0) {
    return true;
  }

  return Boolean(
    currentProps.type &&
      currentProps.labels &&
      currentProps.axes?.major?.dataSets.length &&
      currentProps.data &&
      currentProps.meta,
  );
};

const emptyMetadata = {
  timePeriod: {},
  filters: {},
  indicators: {},
  locations: {},
};

const ChartBuilder = ({
  data,
  onChartSave,
  initialConfiguration,
  onRequiresDataUpdate,
}: Props) => {
  const metaData = useMemo(
    () => (data.metaData && parseMetaData(data.metaData)) || emptyMetadata,
    [data.metaData],
  );

  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialConfiguration,
  );

  const createChartRendererProps = useCallback(
    (currentState: ChartBuilderState): ChartRendererProps => {
      const {
        axes: axesConfiguration,
        definition,
        options,
        dataSetAndConfiguration,
      } = currentState;

      const axes: Dictionary<AxisConfiguration> = {};

      if (axesConfiguration.major) {
        axes.major = {
          ...axesConfiguration.major,
          dataSets: dataSetAndConfiguration.map(dsc => dsc.dataSet),
        };
      }

      if (axesConfiguration.minor) {
        axes.minor = {
          ...axesConfiguration.minor,
          dataSets: [],
        };
      }

      return {
        ...options,
        data,
        axes,
        type: definition?.type ?? 'unknown',
        meta: metaData,
        labels: {
          ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
            (acc, { configuration }) => {
              acc[configuration.value] = configuration;

              return acc;
            },
            {},
          ),
          ...generateAxesMetaData(axes, data, metaData),
        },
        releaseId: data.releaseId,
        getInfographic: editReleaseDataService.downloadChartFile,
      };
    },
    [data, metaData],
  );

  const chartProps = useMemo<ChartRendererProps>(() => {
    return createChartRendererProps(chartBuilderState);
  }, [chartBuilderState, createChartRendererProps]);

  const [finalChartProps, setFinalChartProps] = useState(chartProps);

  const {
    axes: axesConfiguration,
    definition,
    options,
    dataSetAndConfiguration,
  } = chartBuilderState;

  const handleBoundaryLevelChange = useCallback(
    (boundaryLevel: string) => {
      if (onRequiresDataUpdate)
        onRequiresDataUpdate({
          boundaryLevel: boundaryLevel
            ? Number.parseInt(boundaryLevel, 10)
            : undefined,
        });
    },
    [onRequiresDataUpdate],
  );

  const saveChart = useCallback(
    async (nextChartProps: ChartRendererProps) => {
      if (!isChartRenderable(nextChartProps, definition)) {
        return;
      }

      if (onChartSave) {
        await onChartSave(nextChartProps);
        setFinalChartProps(nextChartProps);
      }
    },
    [definition, onChartSave],
  );

  const handleChartDataSubmit = useCallback(async () => {
    await saveChart(finalChartProps);
  }, [finalChartProps, saveChart]);

  const handleChartSave = useCallback(async () => {
    await saveChart(chartProps);
  }, [chartProps, saveChart]);

  return (
    <div className={styles.editor}>
      <ChartTypeSelector
        chartDefinitions={chartDefinitions}
        selectedChartDefinition={definition}
        geoJsonAvailable={data.metaData.geoJsonAvailable}
        onSelectChart={actions.updateChartDefinition}
      />
      <div className="govuk-!-margin-top-6 govuk-body-s dfe-align--right">
        <ButtonText
          onClick={() => {
            actions.updateChartDefinition(Infographic.definition);
          }}
        >
          Choose an infographic as alternative
        </ButtonText>
      </div>

      {definition && (
        <Details summary="Chart preview" open>
          <div className="govuk-width-container">
            {isChartRenderable(chartProps) ? (
              <ChartRenderer {...chartProps} />
            ) : (
              <div className={styles.previewPlaceholder}>
                {Object.keys(axesConfiguration).length > 0 ? (
                  <p>Add data to view a preview of the chart</p>
                ) : (
                  <p>Configure the {definition.name} to view a preview</p>
                )}
              </div>
            )}
          </div>
        </Details>
      )}

      {definition && (
        <Tabs id="chartBuilder-tabs">
          {definition.data.length > 0 && (
            <TabsSection
              title="Data"
              headingTitle="Add data from the existing dataset to the chart"
            >
              <ChartDataSelector
                metaData={metaData}
                selectedData={dataSetAndConfiguration}
                chartType={definition}
                capabilities={definition.capabilities}
                onDataAdded={actions.addDataSet}
                onDataRemoved={actions.removeDataSet}
                onDataChanged={actions.updateDataSet}
                onSubmit={handleChartDataSubmit}
              />
            </TabsSection>
          )}

          <TabsSection
            title="Chart configuration"
            headingTitle="Chart configuration"
          >
            <ChartConfiguration
              selectedChartType={definition}
              chartOptions={options}
              meta={metaData}
              data={data}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={actions.updateChartOptions}
              onSubmit={handleChartSave}
            />
          </TabsSection>

          {Object.entries(axesConfiguration).map(([key, axis]) => (
            <TabsSection
              key={key}
              id={`${key}-tab`}
              title={axis.name}
              headingTitle={axis.name}
            >
              <ChartAxisConfiguration
                id={key}
                configuration={axis}
                capabilities={definition.capabilities}
                data={data}
                meta={metaData}
                labels={chartProps.labels}
                dataSets={
                  axis.type === 'major'
                    ? dataSetAndConfiguration.map(dsc => dsc.dataSet)
                    : []
                }
                onChange={actions.updateChartAxis}
                onSubmit={handleChartSave}
              />
            </TabsSection>
          ))}
        </Tabs>
      )}
    </div>
  );
};

export default ChartBuilder;

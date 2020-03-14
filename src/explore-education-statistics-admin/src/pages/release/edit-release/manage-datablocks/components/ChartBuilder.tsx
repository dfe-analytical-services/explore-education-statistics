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
import { HorizontalBarProps } from '@common/modules/charts/components/HorizontalBarBlock';
import InfographicBlock from '@common/modules/charts/components/InfographicBlock';
import { LineChartProps } from '@common/modules/charts/components/LineChartBlock';
import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import { VerticalBarProps } from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxesConfiguration,
  AxisConfiguration,
  chartDefinitions,
  ChartMetaData,
  ChartProps,
  DataSetConfiguration,
} from '@common/modules/charts/types/chart';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import {
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import React, { useCallback, useMemo, useState } from 'react';

interface Props {
  data: DataBlockResponse;
  initialConfiguration?: ChartRendererProps;
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
  props: ChartRendererProps | undefined,
): props is ChartRendererProps => {
  if (!props) {
    return false;
  }

  // Chart definition may be an infographic
  // and can be rendered without any axes.
  if (props.type === 'infographic' && props.fileId && props.releaseId) {
    return true;
  }

  return Boolean(
    props.type &&
      props.labels &&
      props.axes?.major?.dataSets.length &&
      props.data &&
      props.meta,
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
    (currentState: ChartBuilderState): ChartRendererProps | undefined => {
      const {
        axes: axesConfiguration,
        definition,
        options,
        dataSetAndConfiguration,
      } = currentState;

      if (!definition) {
        return undefined;
      }

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

      const labels: Dictionary<DataSetConfiguration> = {
        ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
          (acc, { configuration }) => {
            acc[configuration.value] = configuration;

            return acc;
          },
          {},
        ),
        ...generateAxesMetaData(axes, data, metaData),
      };

      const baseProps: ChartProps = {
        ...options,
        data,
        axes: {},
        meta: metaData,
        labels,
      };

      switch (definition.type) {
        case 'infographic':
          return {
            ...baseProps,
            labels: {},
            type: 'infographic',
            releaseId: data.releaseId,
            getInfographic: editReleaseDataService.downloadChartFile,
          };
        case 'line':
          return {
            ...baseProps,
            type: 'line',
            axes: axes as LineChartProps['axes'],
          };
        case 'horizontalbar':
          return {
            ...baseProps,
            type: 'horizontalbar',
            axes: axes as HorizontalBarProps['axes'],
          };
        case 'verticalbar':
          return {
            ...baseProps,
            type: 'verticalbar',
            axes: axes as VerticalBarProps['axes'],
          };
        case 'map':
          return {
            ...baseProps,
            type: 'map',
            axes: axes as MapBlockProps['axes'],
          };
        default:
          return undefined;
      }
    },
    [data, metaData],
  );

  const chartProps = useMemo<ChartRendererProps | undefined>(() => {
    return createChartRendererProps(chartBuilderState);
  }, [chartBuilderState, createChartRendererProps]);

  const [finalChartProps, setFinalChartProps] = useState<
    ChartRendererProps | undefined
  >(chartProps);

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
      if (!isChartRenderable(nextChartProps)) {
        return;
      }

      if (onChartSave) {
        await onChartSave(nextChartProps);
        setFinalChartProps(nextChartProps);
      }
    },
    [onChartSave],
  );

  const handleChartDataSubmit = useCallback(async () => {
    if (finalChartProps) {
      await saveChart(finalChartProps);
    }
  }, [finalChartProps, saveChart]);

  const handleChartSave = useCallback(async () => {
    if (chartProps) {
      await saveChart(chartProps);
    }
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
            actions.updateChartDefinition(InfographicBlock.definition);
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

          {Object.entries(axesConfiguration as Required<AxesConfiguration>).map(
            ([key, axis]) => {
              const title = `${definition?.axes[axis.type].title} (${
                axis.type
              } axis)`;

              return (
                <TabsSection
                  key={key}
                  id={`${key}-tab`}
                  title={title}
                  headingTitle={title}
                >
                  <ChartAxisConfiguration
                    id={key}
                    configuration={axis}
                    capabilities={definition.capabilities}
                    data={data}
                    meta={metaData}
                    labels={chartProps?.labels}
                    dataSets={
                      key === 'major'
                        ? dataSetAndConfiguration.map(dsc => dsc.dataSet)
                        : []
                    }
                    onChange={actions.updateChartAxis}
                    onSubmit={handleChartSave}
                  />
                </TabsSection>
              );
            },
          )}
        </Tabs>
      )}
    </div>
  );
};

export default ChartBuilder;

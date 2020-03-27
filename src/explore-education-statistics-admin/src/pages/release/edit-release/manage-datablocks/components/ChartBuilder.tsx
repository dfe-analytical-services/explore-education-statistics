import useGetChartFile from '@admin/hooks/useGetChartFile';
import ChartAxisConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartAxisConfiguration';
import ChartConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartConfiguration';
import ChartDataSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataSelector';
import ChartTypeSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartTypeSelector';
import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { useChartBuilderReducer } from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import {
  horizontalBarBlockDefinition,
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import { infographicBlockDefinition } from '@common/modules/charts/components/InfographicBlock';
import {
  lineChartBlockDefinition,
  LineChartProps,
} from '@common/modules/charts/components/LineChartBlock';
import { mapBlockDefinition } from '@common/modules/charts/components/MapBlock';
import { MapBlockInternalProps } from '@common/modules/charts/components/MapBlockInternal';
import {
  verticalBarBlockDefinition,
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxesConfiguration,
  AxisType,
  ChartDefinition,
  ChartMetaData,
  ChartProps,
  DataSetConfiguration,
} from '@common/modules/charts/types/chart';
import isChartRenderable from '@common/modules/charts/util/isChartRenderable';
import {
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';
import React, { useCallback, useMemo } from 'react';

const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
];

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
  axes: AxesConfiguration,
  data: DataBlockResponse,
  metaData: ChartMetaData,
) {
  return Object.values(axes as Required<AxesConfiguration>).reduce(
    (allValues, axis) => ({
      ...allValues,
      ...[axis.groupBy].reduce(getReduceMetaDataForAxis(data, metaData), {}),
    }),
    {},
  );
}

interface Props {
  data: DataBlockResponse;
  meta: ChartMetaData;
  initialConfiguration?: Chart;
  onChartSave?: (chart: Chart) => void;
  onRequiresDataUpdate?: (parameters: DataBlockRerequest) => void;
}

const ChartBuilder = ({
  data,
  meta,
  onChartSave,
  initialConfiguration,
  onRequiresDataUpdate,
}: Props) => {
  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialConfiguration,
  );

  const {
    axes,
    definition,
    options,
    dataSetAndConfiguration,
    isValid,
  } = chartBuilderState;

  const getChartFile = useGetChartFile(data.releaseId);

  const labels: Dictionary<DataSetConfiguration> = useMemo(
    () => ({
      ...dataSetAndConfiguration.reduce<Dictionary<DataSetConfiguration>>(
        (acc, { configuration }) => {
          acc[configuration.value] = configuration;

          return acc;
        },
        {},
      ),
      ...generateAxesMetaData(axes, data, meta),
    }),
    [axes, data, dataSetAndConfiguration, meta],
  );

  const chartProps = useMemo<ChartRendererProps | undefined>(() => {
    if (!definition) {
      return undefined;
    }

    const baseProps: ChartProps = {
      ...options,
      data,
      axes,
      meta,
      labels,
    };

    switch (definition.type) {
      case 'infographic':
        return {
          ...baseProps,
          labels: {},
          type: 'infographic',
          fileId: options.fileId ?? '',
          getInfographic: getChartFile,
        };
      case 'line':
        return {
          ...(baseProps as LineChartProps),
          type: 'line',
        };
      case 'horizontalbar':
        return {
          ...(baseProps as HorizontalBarProps),
          type: 'horizontalbar',
        };
      case 'verticalbar':
        return {
          ...(baseProps as VerticalBarProps),
          type: 'verticalbar',
        };
      case 'map':
        return {
          ...(baseProps as MapBlockInternalProps),
          type: 'map',
        };
      default:
        return undefined;
    }
  }, [axes, data, definition, labels, meta, options]);

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

  const handleChartSave = useCallback(async () => {
    if (!isChartRenderable(chartProps) && !isValid) {
      return;
    }

    if (onChartSave) {
      await onChartSave(chartProps as ChartRendererProps);
    }
  }, [chartProps, isValid, onChartSave]);

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
            actions.updateChartDefinition(infographicBlockDefinition);
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
                {Object.keys(axes).length > 0 ? (
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
                canSaveChart={isValid}
                meta={meta}
                selectedData={dataSetAndConfiguration}
                chartType={definition}
                capabilities={definition.capabilities}
                onDataAdded={actions.addDataSet}
                onDataRemoved={actions.removeDataSet}
                onDataChanged={actions.updateDataSet}
                onSubmit={handleChartSave}
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
              meta={meta}
              data={data}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={actions.updateChartOptions}
              onSubmit={handleChartSave}
            />
          </TabsSection>

          {Object.entries(
            definition.axes as Required<ChartDefinition['axes']>,
          ).map(([key, axis]) => {
            const axisConfiguration = axes[key as AxisType];

            if (!axisConfiguration) {
              return null;
            }

            return (
              <TabsSection
                key={key}
                id={`${key}-tab`}
                title={axis.title}
                headingTitle={axis.title}
              >
                <ChartAxisConfiguration
                  id={key}
                  configuration={axisConfiguration}
                  capabilities={definition.capabilities}
                  data={data}
                  meta={meta}
                  labels={chartProps?.labels}
                  dataSets={axisConfiguration?.dataSets}
                  onChange={actions.updateChartAxis}
                  onSubmit={handleChartSave}
                />
              </TabsSection>
            );
          })}
        </Tabs>
      )}
    </div>
  );
};

export default ChartBuilder;

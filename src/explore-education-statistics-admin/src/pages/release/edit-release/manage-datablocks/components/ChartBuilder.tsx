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
  AxisType,
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import isChartRenderable from '@common/modules/charts/util/isChartRenderable';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { DataBlockRerequest } from '@common/services/dataBlockService';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import omit from 'lodash/omit';
import React, { useCallback, useMemo } from 'react';

const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
];

interface Props {
  data: TableDataResult[];
  meta: FullTableMeta;
  releaseId: string;
  initialConfiguration?: Chart;
  onChartSave?: (chart: Chart) => void;
  onRequiresDataUpdate?: (parameters: DataBlockRerequest) => void;
}

const ChartBuilder = ({
  data,
  meta,
  releaseId,
  onChartSave,
  initialConfiguration,
  onRequiresDataUpdate,
}: Props) => {
  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialConfiguration,
  );

  const { axes, definition, options, forms } = chartBuilderState;

  const getChartFile = useGetChartFile(releaseId);

  const canSaveChart = useMemo(
    () => Object.values(forms).every(form => form.isValid),
    [forms],
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
    };

    switch (definition.type) {
      case 'infographic':
        return {
          ...baseProps,
          labels: [],
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
  }, [axes, data, definition, getChartFile, meta, options]);

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
    if (chartProps && !isChartRenderable(chartProps)) {
      return;
    }

    if (!canSaveChart) {
      return;
    }

    if (onChartSave) {
      // We don't want to persist data set labels
      // anymore in the deprecated format.
      await onChartSave(omit(chartProps, ['data', 'meta', 'labels']) as Chart);
    }
  }, [chartProps, canSaveChart, onChartSave]);

  return (
    <div className={styles.editor}>
      <ChartTypeSelector
        chartDefinitions={chartDefinitions}
        selectedChartDefinition={definition}
        geoJsonAvailable={meta.geoJsonAvailable}
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
                canSaveChart={canSaveChart}
                meta={meta}
                dataSets={axes.major?.dataSets}
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
              canSaveChart={canSaveChart}
              selectedChartType={definition}
              chartOptions={options}
              meta={meta}
              releaseId={releaseId}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={actions.updateChartOptions}
              onFormStateChange={actions.updateFormState}
              onSubmit={handleChartSave}
            />
          </TabsSection>

          {Object.entries(
            definition.axes as Required<ChartDefinition['axes']>,
          ).map(([type, axis]) => {
            const axisConfiguration = axes[type as AxisType];

            if (!axisConfiguration) {
              return null;
            }

            return (
              <TabsSection
                key={type}
                id={`${type}-tab`}
                title={axis.title}
                headingTitle={axis.title}
              >
                <ChartAxisConfiguration
                  canSaveChart={canSaveChart}
                  id={type}
                  type={type as AxisType}
                  configuration={axisConfiguration}
                  capabilities={definition.capabilities}
                  data={data}
                  meta={meta}
                  onChange={actions.updateChartAxis}
                  onFormStateChange={actions.updateFormState}
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

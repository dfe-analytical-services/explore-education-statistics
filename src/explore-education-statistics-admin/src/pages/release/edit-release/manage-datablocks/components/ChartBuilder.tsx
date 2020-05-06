import useGetChartFile from '@admin/hooks/useGetChartFile';
import ChartAxisConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartAxisConfiguration';
import styles from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilder.module.scss';
import ChartConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartConfiguration';
import ChartDataSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataSelector';
import ChartDefinitionSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDefinitionSelector';
import {
  ChartOptions,
  useChartBuilderReducer,
} from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useToggle from '@common/hooks/useToggle';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import {
  horizontalBarBlockDefinition,
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
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
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import isChartRenderable from '@common/modules/charts/util/isChartRenderable';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  TableDataQuery,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import parseNumber from '@common/utils/number/parseNumber';
import omit from 'lodash/omit';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
];

const filterChartProps = (props: ChartProps): Chart => {
  // We don't want to persist data set labels
  // anymore in the deprecated format.
  return omit(props, ['data', 'meta', 'labels']) as Chart;
};

export type TableQueryUpdateHandler = (
  query: Partial<TableDataQuery>,
) => Promise<void>;

interface Props {
  data: TableDataResult[];
  meta: FullTableMeta;
  releaseId: string;
  initialConfiguration?: Chart;
  onChartSave: (chart: Chart) => void;
  onChartDelete: (chart: Chart) => void;
  onTableQueryUpdate: TableQueryUpdateHandler;
}

const ChartBuilder = ({
  data,
  meta,
  releaseId,
  onChartSave,
  onChartDelete,
  initialConfiguration,
  onTableQueryUpdate,
}: Props) => {
  const containerRef = useRef<HTMLDivElement>(null);

  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  const [isDataLoading, setDataLoading] = useState(false);
  const [shouldSave, setShouldSave] = useState(false);

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

  // Save the chart using an effect as it's easier to
  // ensure that the correct `chartProps` are passed
  // to the `onChartSave` callback.
  useEffect(() => {
    if (!shouldSave) {
      return;
    }

    if (!canSaveChart || !chartProps) {
      return;
    }

    setShouldSave(false);

    if (containerRef.current) {
      containerRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    }

    onChartSave(filterChartProps(chartProps));
  }, [canSaveChart, chartProps, onChartSave, shouldSave]);

  const handleChartDelete = useCallback(async () => {
    toggleDeleteModal.off();

    if (!chartProps) {
      return;
    }

    await onChartDelete(filterChartProps(chartProps));

    actions.resetState();
  }, [actions, chartProps, onChartDelete, toggleDeleteModal]);

  const handleChartDefinitionChange = useCallback(
    async (chartDefinition: ChartDefinition) => {
      actions.updateChartDefinition(chartDefinition);

      if (chartDefinition.type === 'map') {
        setDataLoading(true);

        await onTableQueryUpdate({
          includeGeoJson: true,
        });

        setDataLoading(false);
      }
    },
    [actions, onTableQueryUpdate],
  );

  const handleBoundaryLevelChange = useCallback(
    async (boundaryLevel: string) => {
      setDataLoading(true);

      await onTableQueryUpdate({
        boundaryLevel: parseNumber(boundaryLevel),
      });

      setDataLoading(false);
    },
    [onTableQueryUpdate],
  );

  const handleChartDataSubmit = useCallback(
    (nextDataSets: DataSetConfiguration[]) => {
      actions.updateDataSets(nextDataSets);
      setShouldSave(true);
    },
    [actions],
  );

  const handleChartConfigurationSubmit = useCallback(
    (nextChartOptions: ChartOptions) => {
      actions.updateChartOptions(nextChartOptions);
      setShouldSave(true);
    },
    [actions],
  );

  const handleAxisConfigurationSubmit = useCallback(
    (nextAxis: AxisConfiguration) => {
      actions.updateChartAxis(nextAxis);
      setShouldSave(true);
    },
    [actions],
  );

  const deleteButton = useMemo(
    () => (
      <Button variant="warning" onClick={toggleDeleteModal.on}>
        Delete chart
      </Button>
    ),
    [toggleDeleteModal.on],
  );

  return (
    <div ref={containerRef}>
      <ChartDefinitionSelector
        chartDefinitions={chartDefinitions}
        selectedChartDefinition={definition}
        geoJsonAvailable={meta.geoJsonAvailable}
        onChange={handleChartDefinitionChange}
      />

      {definition && (
        <Details summary="Chart preview" open>
          <div className="govuk-width-container">
            {isChartRenderable(chartProps) ? (
              <LoadingSpinner loading={isDataLoading} text="Loading chart data">
                <ChartRenderer {...chartProps} />
              </LoadingSpinner>
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
        <Tabs id="chartBuilder-tabs" modifyHash={false}>
          {definition.data.length > 0 && (
            <TabsSection
              title="Data"
              headingTitle="Add data from the existing dataset to the chart"
            >
              <ChartDataSelector
                buttons={deleteButton}
                canSaveChart={canSaveChart}
                meta={meta}
                dataSets={axes.major?.dataSets}
                chartType={definition}
                capabilities={definition.capabilities}
                onDataAdded={actions.addDataSet}
                onDataRemoved={actions.removeDataSet}
                onDataChanged={actions.updateDataSets}
                onSubmit={handleChartDataSubmit}
              />
            </TabsSection>
          )}

          <TabsSection
            title="Chart configuration"
            headingTitle="Chart configuration"
          >
            <ChartConfiguration
              buttons={deleteButton}
              canSaveChart={canSaveChart}
              definition={definition}
              chartOptions={options}
              meta={meta}
              releaseId={releaseId}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={actions.updateChartOptions}
              onFormStateChange={actions.updateFormState}
              onSubmit={handleChartConfigurationSubmit}
            />
          </TabsSection>

          {Object.entries(definition.axes as Required<ChartDefinition['axes']>)
            .filter(([, axis]) => !axis.hide)
            .map(([type, axis]) => {
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
                    buttons={deleteButton}
                    canSaveChart={canSaveChart}
                    id={`chartAxisConfiguration-${type}`}
                    type={type as AxisType}
                    configuration={axisConfiguration}
                    capabilities={definition.capabilities}
                    data={data}
                    meta={meta}
                    onChange={actions.updateChartAxis}
                    onFormStateChange={actions.updateFormState}
                    onSubmit={handleAxisConfigurationSubmit}
                  />
                </TabsSection>
              );
            })}
        </Tabs>
      )}

      <ModalConfirm
        title="Delete chart"
        mounted={showDeleteModal}
        onConfirm={handleChartDelete}
        onExit={toggleDeleteModal.off}
      >
        <p>Are you sure you want to delete this chart?</p>
      </ModalConfirm>
    </div>
  );
};

export default ChartBuilder;

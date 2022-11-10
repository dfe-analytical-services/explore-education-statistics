import useGetChartFile from '@admin/hooks/useGetChartFile';
import ChartAxisConfiguration from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import ChartBuilderPreview from '@admin/pages/release/datablocks/components/chart/ChartBuilderPreview';
import ChartConfiguration from '@admin/pages/release/datablocks/components/chart/ChartConfiguration';
import ChartDataSetsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration';
import ChartDefinitionSelector from '@admin/pages/release/datablocks/components/chart/ChartDefinitionSelector';
import ChartLegendConfiguration from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import ChartMapConfiguration from '@admin/pages/release/datablocks/components/chart/ChartMapConfiguration';
import { ChartBuilderFormsContextProvider } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { useChartBuilderReducer } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';
import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  horizontalBarBlockDefinition,
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import { InfographicChartProps } from '@common/modules/charts/components/InfographicBlock';
import {
  lineChartBlockDefinition,
  LineChartProps,
} from '@common/modules/charts/components/LineChartBlock';
import {
  mapBlockDefinition,
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import {
  verticalBarBlockDefinition,
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxisType,
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  ReleaseTableDataQuery,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import parseNumber from '@common/utils/number/parseNumber';
import {
  isServerValidationError,
  ServerValidationErrorResponse,
} from '@common/validation/serverValidations';
import produce from 'immer';
import omit from 'lodash/omit';
import React, { useCallback, useMemo, useRef, useState } from 'react';

const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
];

type ChartBuilderChartProps = ChartRendererProps & {
  file?: File;
};

const filterChartProps = (props: ChartBuilderChartProps): Chart => {
  const excludedProps: (
    | keyof ChartBuilderChartProps
    | keyof InfographicChartProps
  )[] = ['data', 'meta', 'getInfographic', 'file', 'titleType'];

  if (props.titleType === 'default') {
    excludedProps.push('title');
  }

  if (props.type !== 'infographic') {
    excludedProps.push('fileId');
  }

  let filteredProps = omit(props, excludedProps) as Chart;

  // Filter out deprecated data set configurations
  filteredProps = produce(filteredProps, draft => {
    if (draft.axes.major) {
      draft.axes.major.dataSets = draft.axes.major.dataSets.map(
        dataSet => omit(dataSet, ['config']) as DataSet,
      );
    }
  });

  return filteredProps;
};

export type TableQueryUpdateHandler = (
  query: Partial<ReleaseTableDataQuery>,
) => Promise<void>;

interface Props {
  data: TableDataResult[];
  meta: FullTableMeta;
  releaseId: string;
  initialChart?: Chart;
  tableTitle: string;
  onChartSave: (chart: Chart, file?: File) => Promise<void>;
  onChartDelete: (chart: Chart) => void;
  onTableQueryUpdate: TableQueryUpdateHandler;
}

const ChartBuilder = ({
  data,
  meta,
  releaseId,
  initialChart,
  tableTitle,
  onChartSave,
  onChartDelete,
  onTableQueryUpdate,
}: Props) => {
  const containerRef = useRef<HTMLDivElement>(null);

  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  const [isDataLoading, setDataLoading] = useState(false);
  const [isDeleting, setDeleting] = useState(false);

  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialChart,
    tableTitle,
  );

  const { axes, definition, options, legend } = chartBuilderState;

  const getChartFile = useGetChartFile(releaseId);

  const [submitError, setSubmitError] = useState<
    ServerValidationErrorResponse
  >();

  const dataSetsUnits = useMemo(
    () =>
      axes.major?.dataSets.reduce<string[]>((acc, dataSet) => {
        const foundIndicator = meta.indicators.find(
          indicator => indicator.value === dataSet.indicator,
        );
        if (foundIndicator) {
          acc.push(foundIndicator.unit);
        }
        return acc;
      }, []),
    [axes.major?.dataSets, meta.indicators],
  );

  const chartProps = useMemo<ChartBuilderChartProps | undefined>(() => {
    if (!definition || !options) {
      return undefined;
    }

    const baseProps: ChartProps = {
      ...options,
      data,
      legend,
      axes,
      meta,
    };

    switch (definition.type) {
      case 'infographic':
        return {
          ...baseProps,
          type: 'infographic',
          fileId: options.file ? options.file.name : options.fileId ?? '',
          getInfographic: options.file
            ? () => Promise.resolve(options.file as File)
            : getChartFile,
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
          ...(baseProps as MapBlockProps),
          boundaryLevel: options.boundaryLevel ?? 0,
          type: 'map',
        };
      default:
        return undefined;
    }
  }, [axes, data, definition, getChartFile, legend, meta, options]);

  const handleSubmit = useCallback(async () => {
    if (!chartProps) {
      return;
    }

    setSubmitError(undefined);

    if (containerRef.current) {
      containerRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    }

    try {
      await onChartSave(filterChartProps(chartProps), chartProps.file);
    } catch (error) {
      if (isServerValidationError(error) && error.response?.data) {
        setSubmitError(error.response.data);
      } else {
        throw error;
      }
    }
  }, [chartProps, onChartSave]);

  const handleChartDelete = useCallback(async () => {
    toggleDeleteModal.off();

    if (!chartProps) {
      return;
    }

    setDeleting(true);

    try {
      await onChartDelete(filterChartProps(chartProps));

      actions.resetState();
    } finally {
      setDeleting(false);
    }
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

  const [handleChartConfigurationChange] = useDebouncedCallback(
    actions.updateChartOptions,
    200,
  );

  const [handleLegendConfigurationChange] = useDebouncedCallback(
    actions.updateChartLegend,
    200,
  );

  const [handleAxisConfigurationChange] = useDebouncedCallback(
    actions.updateChartAxis,
    200,
  );

  const handleBoundaryLevelChange = useCallback(
    async (nextBoundaryLevel: string) => {
      setDataLoading(true);

      await onTableQueryUpdate({
        boundaryLevel: parseNumber(nextBoundaryLevel),
      });

      setDataLoading(false);
    },
    [onTableQueryUpdate],
  );

  const deleteButton = useMemo(
    () =>
      initialChart && (
        <Button
          variant="warning"
          onClick={toggleDeleteModal.on}
          disabled={isDeleting}
        >
          Delete chart
        </Button>
      ),
    [initialChart, isDeleting, toggleDeleteModal.on],
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
        <ChartBuilderPreview chart={chartProps} loading={isDataLoading} />
      )}

      {definition && (
        <ChartBuilderFormsContextProvider
          definition={definition}
          onSubmit={handleSubmit}
        >
          {({ forms }) => (
            <Tabs id="chartBuilder-tabs" modifyHash={false}>
              {options && (
                <TabsSection
                  title="Chart configuration"
                  headingTitle="Chart configuration"
                  id={forms.options.id}
                >
                  <ChartConfiguration
                    buttons={deleteButton}
                    submitError={submitError}
                    definition={definition}
                    chartOptions={options}
                    legendPosition={legend?.position}
                    onChange={handleChartConfigurationChange}
                    onSubmit={actions.updateChartOptions}
                  />
                </TabsSection>
              )}

              {forms.dataSets && definition.axes.major && (
                <TabsSection
                  title="Data sets"
                  headingTitle="Data sets"
                  id={forms.dataSets.id}
                >
                  <ChartDataSetsConfiguration
                    buttons={deleteButton}
                    dataSets={axes.major?.dataSets}
                    dataSetsUnits={dataSetsUnits}
                    meta={meta}
                    onChange={actions.updateDataSets}
                  />
                </TabsSection>
              )}

              {forms.map && definition?.type === 'map' && options && (
                <TabsSection
                  title="Map configuration"
                  headingTitle="Map configuration"
                  id={forms.map.id}
                >
                  <ChartMapConfiguration
                    buttons={deleteButton}
                    dataSetsUnits={dataSetsUnits}
                    meta={meta}
                    options={options}
                    onBoundaryLevelChange={handleBoundaryLevelChange}
                    onChange={handleChartConfigurationChange}
                    onSubmit={actions.updateChartOptions}
                  />
                </TabsSection>
              )}

              {forms.legend && axes.major && legend && (
                <TabsSection
                  title="Legend"
                  headingTitle="Legend"
                  id={forms.legend.id}
                >
                  <ChartLegendConfiguration
                    axisMajor={axes.major}
                    buttons={deleteButton}
                    data={data}
                    definition={definition}
                    legend={legend}
                    meta={meta}
                    showDataLabels={options?.showDataLabels}
                    onChange={handleLegendConfigurationChange}
                    onSubmit={actions.updateChartLegend}
                  />
                </TabsSection>
              )}

              {Object.entries(
                definition.axes as Required<ChartDefinition['axes']>,
              )
                .filter(([, axis]) => !axis.hide)
                .map(([type, axis]) => {
                  const form = forms[type as AxisType];
                  const axisConfiguration = axes[type as AxisType];

                  if (!axisConfiguration || !form) {
                    return null;
                  }

                  return (
                    <TabsSection
                      key={type}
                      id={form.id}
                      title={axis.title}
                      headingTitle={axis.title}
                    >
                      <ChartAxisConfiguration
                        axesConfiguration={axes}
                        buttons={deleteButton}
                        id={`chartAxisConfiguration-${type}`}
                        type={type as AxisType}
                        definition={definition}
                        data={data}
                        meta={meta}
                        includeNonNumericData={options?.includeNonNumericData}
                        onChange={handleAxisConfigurationChange}
                        onSubmit={actions.updateChartAxis}
                      />
                    </TabsSection>
                  );
                })}
            </Tabs>
          )}
        </ChartBuilderFormsContextProvider>
      )}

      <ModalConfirm
        title="Delete chart"
        open={showDeleteModal}
        onConfirm={handleChartDelete}
        onExit={toggleDeleteModal.off}
      >
        <p>Are you sure you want to delete this chart?</p>
      </ModalConfirm>
    </div>
  );
};

export default ChartBuilder;

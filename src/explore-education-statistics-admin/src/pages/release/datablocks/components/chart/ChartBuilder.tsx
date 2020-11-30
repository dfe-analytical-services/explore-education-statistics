import useGetChartFile from '@admin/hooks/useGetChartFile';
import ChartAxisConfiguration from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import ChartBuilderPreview from '@admin/pages/release/datablocks/components/chart/ChartBuilderPreview';
import ChartConfiguration from '@admin/pages/release/datablocks/components/chart/ChartConfiguration';
import ChartDataSetsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration';
import ChartDefinitionSelector from '@admin/pages/release/datablocks/components/chart/ChartDefinitionSelector';
import ChartLegendConfiguration from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import {
  ChartOptions,
  useChartBuilderReducer,
} from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { ChartBuilderForms } from '@admin/pages/release/datablocks/components/chart/types/chartBuilderForms';
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
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  ReleaseTableDataQuery,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import {
  isServerValidationError,
  ServerValidationErrorResponse,
} from '@common/validation/serverValidations';
import produce from 'immer';
import mapValues from 'lodash/mapValues';
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

type ChartBuilderChartProps = ChartRendererProps & {
  file?: File;
};

const filterChartProps = (props: ChartBuilderChartProps): Chart => {
  const excludedProps: (
    | keyof ChartBuilderChartProps
    | keyof InfographicChartProps
  )[] = ['data', 'meta', 'getInfographic', 'file'];

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
  initialConfiguration?: Chart;
  onChartSave: (chart: Chart, file?: File) => Promise<void>;
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

  const [isSaving, setSaving] = useState(false);
  const [isDeleting, setDeleting] = useState(false);

  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialConfiguration,
  );

  const {
    axes,
    definition,
    options,
    legend,
    forms: formStates,
  } = chartBuilderState;

  const getChartFile = useGetChartFile(releaseId);

  const forms: ChartBuilderForms = useMemo(() => {
    const formTitles: Dictionary<string> = {
      ...mapValues(
        (definition?.axes as Required<ChartDefinition['axes']>) ?? {},
        axis => axis.title,
      ),
      data: 'Data sets',
      legend: 'Legend',
      options: 'Chart configuration',
    };

    return mapValues(formStates, (form, formKey) => ({
      ...form,
      title: formTitles[formKey],
      id: `chartBuilder-${formKey}`,
    }));
  }, [definition, formStates]);

  const [submitError, setSubmitError] = useState<
    ServerValidationErrorResponse
  >();

  const canSaveChart = useMemo(
    () => Object.values(forms).every(form => form.isValid),
    [forms],
  );

  const hasSubmittedChart = useMemo(
    () => Object.values(forms).some(form => form.submitCount > 0),
    [forms],
  );

  const chartProps = useMemo<ChartBuilderChartProps | undefined>(() => {
    if (!definition) {
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
          type: 'map',
        };
      default:
        return undefined;
    }
  }, [axes, data, definition, getChartFile, legend, meta, options]);

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
    setSubmitError(undefined);

    if (containerRef.current) {
      containerRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    }

    setSaving(true);

    onChartSave(filterChartProps(chartProps), chartProps.file)
      .catch(error => {
        if (isServerValidationError(error) && error.response?.data) {
          setSubmitError(error.response.data);
        } else {
          throw error;
        }
      })
      .finally(() => {
        setSaving(false);
      });
  }, [canSaveChart, chartProps, onChartSave, shouldSave]);

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
    async (boundaryLevel: string) => {
      setDataLoading(true);

      await onTableQueryUpdate({
        boundaryLevel: parseNumber(boundaryLevel),
      });

      setDataLoading(false);
    },
    [onTableQueryUpdate],
  );

  const handleChartConfigurationSubmit = useCallback(
    (nextChartOptions: ChartOptions) => {
      actions.updateChartOptions(nextChartOptions);
      setShouldSave(true);
    },
    [actions],
  );

  const handleChartDataSetsSubmit = useCallback(() => {
    setShouldSave(true);
    actions.updateFormState({
      form: 'data',
      isValid: forms.data.isValid,
      submitCount: forms.data.submitCount + 1,
    });
  }, [actions, forms.data.isValid, forms.data.submitCount]);

  const handleLegendConfigurationSubmit = useCallback(
    (nextLegend: LegendConfiguration) => {
      actions.updateChartLegend(nextLegend);
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
      <Button
        variant="warning"
        onClick={toggleDeleteModal.on}
        disabled={isDeleting}
      >
        Delete chart
      </Button>
    ),
    [isDeleting, toggleDeleteModal.on],
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
        <ChartBuilderPreview
          axes={axes}
          chart={chartProps}
          definition={definition}
          loading={isDataLoading}
        />
      )}

      {definition && (
        <Tabs id="chartBuilder-tabs" modifyHash={false}>
          <TabsSection
            title="Chart configuration"
            headingTitle="Chart configuration"
            id={forms.options.id}
          >
            <ChartConfiguration
              buttons={deleteButton}
              hasSubmittedChart={hasSubmittedChart}
              isSaving={isSaving}
              submitError={submitError}
              forms={forms}
              definition={definition}
              chartOptions={options}
              meta={meta}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={handleChartConfigurationChange}
              onFormStateChange={actions.updateFormState}
              onSubmit={handleChartConfigurationSubmit}
            />
          </TabsSection>

          {definition.axes.major && (
            <TabsSection
              title="Data sets"
              headingTitle="Data sets"
              id={forms.data.id}
            >
              <ChartDataSetsConfiguration
                buttons={deleteButton}
                isSaving={isSaving}
                forms={forms}
                meta={meta}
                dataSets={axes.major?.dataSets}
                onChange={actions.updateDataSets}
                onSubmit={handleChartDataSetsSubmit}
              />
            </TabsSection>
          )}

          {definition.capabilities.hasLegend && axes.major && legend && (
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
                isSaving={isSaving}
                legend={legend}
                forms={forms}
                meta={meta}
                onChange={handleLegendConfigurationChange}
                onFormStateChange={actions.updateFormState}
                onSubmit={handleLegendConfigurationSubmit}
              />
            </TabsSection>
          )}

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
                  id={forms[type].id}
                  title={axis.title}
                  headingTitle={axis.title}
                >
                  <ChartAxisConfiguration
                    buttons={deleteButton}
                    hasSubmittedChart={hasSubmittedChart}
                    isSaving={isSaving}
                    forms={forms}
                    id={`chartAxisConfiguration-${type}`}
                    type={type as AxisType}
                    configuration={axisConfiguration}
                    definition={definition}
                    data={data}
                    meta={meta}
                    onChange={handleAxisConfigurationChange}
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
